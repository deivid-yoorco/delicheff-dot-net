using MessageBird;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Identity;
using Teed.Plugin.Api.Dtos.Auth;
using Teed.Plugin.Api.Models;
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Controllers
{
    public class AuthController : ApiBaseController
    {
        private IConfiguration _config { get; }

        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISettingService _settingService;
        private readonly IDbContext _dbContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CustomerSecurityTokenService _customerSecurityTokenService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IPictureService _pictureService;
        private readonly ISmsVerificationService _smsVerificationService;

        private const int TOKEN_EXPIRATION_MINUTES = 30;

        public AuthController(ICustomerRegistrationService customerRegistrationService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IStoreContext storeContext,
            IStoreService storeService,
            ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            ICustomerActivityService customerActivityService,
            IWorkflowMessageService workflowMessageService,
            IConfiguration config,
            IDbContext dbContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPictureService pictureService,
            LocalizationSettings localizationSettings,
            CustomerSecurityTokenService customerSecurityTokenService,
            CustomerSettings customerSettings,
            ISmsVerificationService smsVerificationService)
        {
            _smsVerificationService = smsVerificationService;
            _customerRegistrationService = customerRegistrationService;
            _localizationService = localizationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _customerSecurityTokenService = customerSecurityTokenService;
            _customerActivityService = customerActivityService;
            _storeContext = storeContext;
            _settingService = settingService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _customerSettings = customerSettings;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _config = config;
            _dbContext = dbContext;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _pictureService = pictureService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            var customer = _customerService.GetCustomerByEmail(email);
            if (customer != null)
                return Ok(new { exists = true, message = "" });
            else
                return Ok(new { exists = false, message = _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist") });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetToken([FromBody] LoginDto dto, bool legacy = true, bool fromPartner = false)
        {
            if (dto == null) return BadRequest();
            if (dto.Device == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loginResult = _customerRegistrationService.ValidateCustomer(dto.Email, dto.Password);
            Customer customer = null;
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        customer = _customerService.GetCustomerByEmail(dto.Email);
                        if (!customer.Active) return Unauthorized();
                        if (fromPartner)
                        {
                            bool hasAccess = customer.IsInCustomerRole("buyer") || customer.IsInCustomerRole("delivery") || customer.IsInCustomerRole("Administrators") || customer.IsInCustomerRole("partnerapp");
                            if (!hasAccess) return Unauthorized();
                        }
                        _eventPublisher.Publish(new CustomerLoggedinEvent(customer));
                        _customerActivityService.InsertActivity(customer, "PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"));
                        break;
                    }
                case CustomerLoginResults.CustomerNotExist:
                case CustomerLoginResults.Deleted:
                case CustomerLoginResults.NotActive:
                case CustomerLoginResults.NotRegistered:
                case CustomerLoginResults.LockedOut:
                case CustomerLoginResults.WrongPassword:
                    return Unauthorized();
                default:
                    return BadRequest("No fue posible completar el inicio de sesión");
            }

            var tokens = GetTokens(customer);
            SaveSecurityToken(customer, tokens.RefreshToken, dto.Device.Uuid, dto.Device.Model, dto.Device.Platform, dto.Device.Version, dto.Device.Manufacturer, dto.Device.Serial);

            var birthDate = customer.GetAttribute<DateTime>(SystemCustomerAttributeNames.DateOfBirth);
            var registrationResult = new RegistrationResultDto()
            {
                Tokens = tokens,
                BirthDate = birthDate == default ? null : birthDate.ToString("dd-MM-yyyy"),
                Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender),
                NewsletterCheck = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id) != null,
                PhoneNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                ProfilePictureId = customer.ProfilePictureId ?? 0
            };

            dynamic result;
            if (fromPartner || legacy) result = tokens;
            else result = registrationResult;

            return Ok(result);
        }

        [HttpGet]
        public IActionResult RefreshAppToken()
        {
            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshDto dto)
        {
            if (dto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string issuer = _config["Api:Issuer"] + Enum.GetName(typeof(TeedStores), TeedCommerceStores.CurrentStore);

            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters =
                new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Api:RefreshKey"] + Enum.GetName(typeof(TeedStores), TeedCommerceStores.CurrentStore)))
                };


            ClaimsPrincipal userData;

            try
            {
                userData = handler.ValidateToken(dto.RefreshToken, validationParameters, out validatedToken);
            }
            catch
            {
                return Unauthorized();
            }

            int customerId = int.Parse(userData.Claims.FirstOrDefault(c => c.Type == "user_id").Value);
            var user = _customerSecurityTokenService.GetAll().Where(x => x.CustomerId == customerId).ToList();
            if (user.Count == 0 || !user.Any(x => x.RefreshToken == dto.RefreshToken)) return Unauthorized();

            var customer = _customerService.GetCustomerById(int.Parse(dto.UserId));
            if (!customer.Active) return Unauthorized();
            var claims = GetCustomerClaims(customer);

            TokensInfoDto tokens = new TokensInfoDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(CreateToken(issuer, claims, _config["Api:Key"] + Enum.GetName(typeof(TeedStores), TeedCommerceStores.CurrentStore), false))
            };

            //only create a new access token 
            return Ok(tokens);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult RegisterCustomerValidation([FromBody] RegisterCustomerValidationDto dto)
        {
            var customer = _customerService.GetCustomerByEmail(dto.Email);
            if (customer != null && !dto.VerifyOnlyNumber)
                return Ok(new RegisterCustomerValidationResultDto()
                {
                    Code = string.Empty,
                    ShouldValidatePhone = true,
                    ErrorMessage = "Ya existe una cuenta registrada con el correo electrónico ingresado."
                });
            Regex rg = new Regex(@"^\d{10}$");
            string parsedPhoneNumber = dto.PhoneNumer.Trim();
            if (IsDigitsOnly(parsedPhoneNumber) && rg.Matches(parsedPhoneNumber).Count > 0)
            {
                var alreadyValidate = _smsVerificationService
                    .GetSmsVerificationsByPhoneNumberAndCode(dto.PhoneNumer)
                    .Where(x => x.IsVerified).ToList();

                string numberExistsInDb =
                    @"SELECT TOP (1) [Value] 
	                        FROM [dbo].[GenericAttribute] 
	                        where KeyGroup = 'Customer' 
	                        AND [Key] = 'Phone'
	                        AND [Value] = '" + parsedPhoneNumber + "'";
                string numberExists = _dbContext.SqlQuery<string>(numberExistsInDb).FirstOrDefault();
                if (alreadyValidate.Any() || !string.IsNullOrEmpty(numberExists))
                    return Ok(new RegisterCustomerValidationResultDto()
                    {
                        Code = string.Empty,
                        ShouldValidatePhone = true,
                        ErrorMessage = "Ya existe una cuenta registrada con el número de teléfono ingresado."
                    });
            }
            else
                return Ok(new RegisterCustomerValidationResultDto()
                {
                    Code = string.Empty,
                    ShouldValidatePhone = true,
                    ErrorMessage = "El número ingresado no es válido."
                });

            using (HttpClient httpClient = new HttpClient())
            {
                var url = _storeContext.CurrentStore.SecureUrl;
                var branchResult = httpClient.GetAsync(url + "/MessageBird/SetCurrentKeysForNopWeb").Result;
                var settings = _settingService.LoadSetting<SmsVerificationSettings>();
                if (settings.IsActive)
                {
                    var key = settings.IsSandbox ? settings.SandboxApiKey : settings.ApiKey;
                    if (!string.IsNullOrEmpty(key))
                    {
                        string code = CommonHelper.GenerateRandomDigitCode(6);
                        var smsVerification = new SmsVerification
                        {
                            CustomerEmail = dto.Email,
                            PhoneNumber = parsedPhoneNumber,
                            VerificationCode = code,
                            Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - Se creó el código de verificación para el pre-usuario con mail ingresado de registro \"{dto.Email}\".\n"
                        };
                        _smsVerificationService.InsertSmsVerification(smsVerification);

                        Client client = Client.CreateDefault(key);
                        long.TryParse("+521" + parsedPhoneNumber, out long Msisdn);
                        if (Msisdn == 0)
                            return Ok(new RegisterCustomerValidationResultDto()
                            {
                                Code = string.Empty,
                                ShouldValidatePhone = true,
                                ErrorMessage = "Couldn't convert phone number to long value"
                            });
                        string body = $"[{code}] - ¡Hola desde Central en Línea! Este es tu código de verificación: {code}";

                        try
                        {
                            var result = client.SendMessage("CEL",
                                body, new[] { Msisdn });

                            //Message sent succesfully
                            return Ok(new RegisterCustomerValidationResultDto()
                            {
                                Code = code,
                                ShouldValidatePhone = true,
                                ErrorMessage = string.Empty,
                                MinutesForCodeRequest = settings.MinutesForCodeRequest * 60
                            });
                        }
                        catch (Exception e)
                        {
                            return Ok(new RegisterCustomerValidationResultDto()
                            {
                                Code = string.Empty,
                                ShouldValidatePhone = true,
                                ErrorMessage = "[C] - No fue posible enviar el SMS: " + e.Message
                            });
                        }
                    }
                    else
                        return Ok(new RegisterCustomerValidationResultDto()
                        {
                            Code = string.Empty,
                            ShouldValidatePhone = true,
                            ErrorMessage = "[C] - No API Key registered"
                        });
                }
                else
                    return Ok(new RegisterCustomerValidationResultDto()
                    {
                        Code = string.Empty,
                        ShouldValidatePhone = false,
                        ErrorMessage = string.Empty
                    });
            }

        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ValidateSmsVerificationCode([FromBody] IRegisterCodeValidationDto dto)
        {
            var number = dto.PhoneNumer?.Trim();
            var settings = _settingService.LoadSetting<SmsVerificationSettings>();
            if (!string.IsNullOrEmpty(number))
            {
                if (!string.IsNullOrEmpty(dto.Code))
                {
                    Regex rg = new Regex(@"^\d{10}$");
                    if (IsDigitsOnly(number) && rg.Matches(number).Count > 0)
                    {
                        var alreadyValidate = _smsVerificationService.GetSmsVerificationsByPhoneNumberAndCode(number)
                            .Where(x => x.IsVerified).ToList();

                        string numberExistsInDb =
                            @"SELECT TOP (1) [Value] 
	                        FROM [dbo].[GenericAttribute] 
	                        where KeyGroup = 'Customer' 
	                        AND [Key] = 'Phone'
	                        AND [Value] = '" + number + "'";
                        string numberExists = _dbContext.SqlQuery<string>(numberExistsInDb).FirstOrDefault();
                        if (!alreadyValidate.Any() && string.IsNullOrEmpty(numberExists))
                        {
                            var smsValidation = _smsVerificationService.GetSmsVerificationsByPhoneNumberAndCode(number, dto.Code).FirstOrDefault();
                            if (smsValidation != null)
                            {
                                var time = (DateTime.UtcNow - smsValidation.CreatedOnUtc).TotalMinutes;
                                if (time < settings.MinutesForCodeRequest)
                                {
                                    smsValidation.IsVerified = true;
                                    _smsVerificationService.UpdateSmsVerification(smsValidation);

                                    return Ok(new IRegisterCodeValidationResultDto()
                                    {
                                        ValidatedCorrectly = true,
                                        ErrorMessage = string.Empty
                                    });
                                }
                                else
                                    return Ok(new IRegisterCodeValidationResultDto()
                                    {
                                        ValidatedCorrectly = false,
                                        ErrorMessage = "El código de verificación ha expirado, por favor solicita otro."
                                    });
                            }
                            else
                                return Ok(new IRegisterCodeValidationResultDto()
                                {
                                    ValidatedCorrectly = false,
                                    ErrorMessage = "El código de verificación es inválido"
                                });
                        }
                        else
                            return Ok(new IRegisterCodeValidationResultDto()
                            {
                                ValidatedCorrectly = false,
                                ErrorMessage = "El número de teléfono ingresado ya está siendo utilizado otra cuenta."
                            });
                    }
                    else
                        return Ok(new IRegisterCodeValidationResultDto()
                        {
                            ValidatedCorrectly = false,
                            ErrorMessage = "Número de celular inválido, asegúrate de que el número ingresado no contenga símbolos, letras o espacios y que tenga 10 dígitos"
                        });
                }
                else
                    return Ok(new IRegisterCodeValidationResultDto()
                    {
                        ValidatedCorrectly = false,
                        ErrorMessage = "Código de verificación inválido, por favor ingresa el código de verificación."
                    });
            }
            else
                return Ok(new IRegisterCodeValidationResultDto()
                {
                    ValidatedCorrectly = false,
                    ErrorMessage = "Número de teléfono vacío, ingresa un número de teléfono válido."
                });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PasswordRecovery([FromBody] PasswordRecoveryDto dto)
        {
            var customer = _customerService.GetCustomerByEmail(dto.Email);
            if (customer != null && customer.Active && !customer.Deleted)
            {
                var passwordRecoveryToken = Guid.NewGuid();
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, passwordRecoveryToken.ToString());
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, DateTime.UtcNow);

                _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer, customer.Id);

                return Ok();
            }

            return BadRequest("No tenemos registros de ningún usuario con el correo electrónico indicado.");
        }

        [HttpPost]
        public IActionResult UpdateAccount([FromBody] UpdateAccountDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();
            int userId = int.Parse(UserId);
            var customer = _customerService.GetCustomerById(userId);
            if (customer == null) return NotFound();

            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, dto.FirstName);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, dto.LastName);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, dto.PhoneNumber);

            if (!string.IsNullOrWhiteSpace(dto.Gender))
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, dto.Gender);

            if (!string.IsNullOrWhiteSpace(dto.BirthDate))
            {
                bool success = DateTime.TryParseExact(dto.BirthDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOfBirth);
                if (success)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
            }

            //newsletter
            if (_customerSettings.NewsletterEnabled)
            {
                //save newsletter value
                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                if (newsletter != null)
                {
                    if (dto.NewsletterCheck && !newsletter.Active)
                    {
                        newsletter.Active = true;
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                    }
                    else if (!dto.NewsletterCheck)
                    {
                        _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                    }
                }
                else if (dto.NewsletterCheck)
                {
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = customer.Email,
                        Active = true,
                        StoreId = _storeContext.CurrentStore.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.ProfilePictureBase64))
            {
                var customerAvatar = _pictureService.GetPictureById(customer.ProfilePictureId ?? 0);
                var customerPictureBinary = Convert.FromBase64String(dto.ProfilePictureBase64);

                if (customerAvatar != null)
                    customerAvatar = _pictureService.UpdatePicture(customerAvatar.Id, customerPictureBinary, dto.ProfilePictureMimeType, null);
                else
                    customerAvatar = _pictureService.InsertPicture(customerPictureBinary, dto.ProfilePictureMimeType, null);

                customer.ProfilePictureId = customerAvatar.Id;
                _customerService.UpdateCustomer(customer);
            }

            if (!string.IsNullOrWhiteSpace(dto.OldPassword) && !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, dto.NewPassword, dto.OldPassword);

                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);

                if (!changePasswordResult.Success) return BadRequest(string.Join(", ", changePasswordResult.Errors));
            }

            return Ok(customer.ProfilePictureId);
        }

        [HttpGet]
        public IActionResult GetProfilePicture()
        {
            int.TryParse(UserId, out int userId);
            var profilePictureId = _customerService.GetAllCustomersQuery().Where(x => x.Id == userId).Select(x => x.ProfilePictureId).FirstOrDefault();
            return Ok(profilePictureId);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (_customerService.GetCustomerByEmail(dto.Email) == null)
            {
                Customer customer = new Customer();
                customer.CreatedOnUtc = DateTime.UtcNow;
                customer.LastActivityDateUtc = DateTime.UtcNow;

                var registrationRequest = new CustomerRegistrationRequest(customer,
                    dto.Email, dto.Email,
                    dto.Password,
                    PasswordFormat.Hashed,
                    _storeContext.CurrentStore.Id,
                    true);

                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (!registrationResult.Success)
                    return BadRequest("Ha ocurrido un problema al intentar hacer el registro. Por favor inténtalo de nuevo, si el problema persiste contáctanos para ayudarte a solucionarlo.");

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, dto.FirstName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, dto.LastName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, dto.PhoneNumber);

                if (!string.IsNullOrWhiteSpace(dto.Gender))
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, dto.Gender);

                if (!string.IsNullOrWhiteSpace(dto.BirthDate))
                {
                    bool success = DateTime.TryParseExact(dto.BirthDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOfBirth);
                    if (success)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                }
                _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                if (_customerSettings.NotifyNewCustomerRegistration)
                    _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer, _localizationSettings.DefaultAdminLanguageId);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    //save newsletter value
                    var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(dto.Email, _storeContext.CurrentStore.Id);
                    if (newsletter != null)
                    {
                        if (dto.NewsletterCheck && !newsletter.Active)
                        {
                            newsletter.Active = true;
                            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                        }
                    }
                    else if (dto.NewsletterCheck)
                    {
                        _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                        {
                            NewsLetterSubscriptionGuid = Guid.NewGuid(),
                            Email = dto.Email,
                            Active = true,
                            StoreId = _storeContext.CurrentStore.Id,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    }
                }

                TokensInfoDto tokens = GetTokens(customer);
                SaveSecurityToken(customer, tokens.RefreshToken, dto.Device.Uuid, dto.Device.Model, dto.Device.Platform, dto.Device.Version, dto.Device.Manufacturer, dto.Device.Serial);
                var result = new RegistrationResultDto()
                {
                    Tokens = tokens,
                    BirthDate = dto.BirthDate,
                    Gender = dto.Gender,
                    NewsletterCheck = dto.NewsletterCheck,
                    PhoneNumber = dto.PhoneNumber,
                    SuccessNote = "Con tu nueva cuenta ya puedes completar y administrar tus pedidos directamente desde tu dispositivo."
                };

                return Ok(result);
            }

            return Unauthorized();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithFacebook([FromBody] LoginWithFacebookDto dto)
        {
            if (_customerService.GetCustomerByEmail(dto.Email) == null)
            {
                Customer customer = new Customer();
                customer.CreatedOnUtc = DateTime.UtcNow;
                customer.LastActivityDateUtc = DateTime.UtcNow;

                var registrationRequest = new CustomerRegistrationRequest(customer,
                    dto.Email, dto.Email,
                    CommonHelper.GenerateRandomDigitCode(20),
                    PasswordFormat.Hashed,
                    _storeContext.CurrentStore.Id,
                    true);

                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (!registrationResult.Success)
                    return BadRequest("Ha ocurrido un problema al intentar hacer el registro.");

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, dto.FirstName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, dto.LastName);

                _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                if (_customerSettings.NotifyNewCustomerRegistration)
                    _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer, _localizationSettings.DefaultAdminLanguageId);
            }

            // Customer exists or register successfully
            var facebookSettings = _settingService.LoadSetting<TeedApiPluginSettings>();
            HttpClient client = new HttpClient();

            string fbAccessToken = await GetFacebookAccessToken(facebookSettings, client);
            if (fbAccessToken == null) return BadRequest("Ocurrió un problema al intentar conectarse con Facebook");

            if (await ValidateFacebookAccesToken(dto.AccessToken, fbAccessToken, client))
            {
                Customer customer = _customerService.GetCustomerByEmail(dto.Email);
                var tokens = GetTokens(customer);
                SaveSecurityToken(customer, tokens.RefreshToken, dto.Device.Uuid, dto.Device.Model, dto.Device.Platform, dto.Device.Version, dto.Device.Manufacturer, dto.Device.Serial);

                return Ok(tokens);
            }

            return BadRequest("Ocurrió un problema al intentar iniciar sesión con Facebook");
        }

        #region Private Methods

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private async Task<string> GetFacebookAccessToken(TeedApiPluginSettings facebookSettings, HttpClient client)
        {
            var rawResult = await client.GetAsync($"https://graph.facebook.com/oauth/access_token?client_id={facebookSettings.FacebookAppId}&client_secret={facebookSettings.FacebookAppSecret}&grant_type=client_credentials");
            if (rawResult.IsSuccessStatusCode)
            {
                var rawResultJson = await rawResult.Content.ReadAsStringAsync();
                var accessToken = JsonConvert.DeserializeObject<FacebookTokenModel>(rawResultJson);
                return accessToken.access_token;
            }
            return null;
        }

        private async Task<bool> ValidateFacebookAccesToken(string userToken, string fbAccessToken, HttpClient client)
        {
            var validationRawResult = await client.GetAsync($"https://graph.facebook.com/debug_token?input_token={userToken}&access_token={fbAccessToken}");
            if (validationRawResult.IsSuccessStatusCode)
            {
                string validationResultJson = await validationRawResult.Content.ReadAsStringAsync();
                var validationResult = JsonConvert.DeserializeObject<FacebookTokenValidationModel>(validationResultJson);
                return validationResult.data.is_valid;
            }
            return false;
        }

        private TokensInfoDto GetTokens(Customer customer)
        {
            var claims = GetCustomerClaims(customer);
            string issuer = _config["Api:Issuer"] + Enum.GetName(typeof(TeedStores), TeedCommerceStores.CurrentStore);
            return new TokensInfoDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(CreateToken(issuer, claims, _config["Api:Key"] + Enum.GetName(typeof(TeedStores), TeedCommerceStores.CurrentStore), false)),
                RefreshToken = new JwtSecurityTokenHandler().WriteToken(CreateToken(issuer, claims, _config["Api:RefreshKey"] + Enum.GetName(typeof(TeedStores), TeedCommerceStores.CurrentStore), true))
            };
        }

        private Claim[] GetCustomerClaims(Customer customer)
        {
            string[] userRoles = customer.CustomerRoles.Select(x => x.Name).ToArray();

            return new[]
            {
                new Claim("user_id", customer.Id.ToString()),
                new Claim("first_name", customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName)),
                new Claim("last_name", customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName)),
                new Claim("roles", string.Join(",", userRoles)),
                new Claim(JwtRegisteredClaimNames.Sub, customer.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
        }

        private JwtSecurityToken CreateToken(string issuer, Claim[] claims, string key, bool isRefresh)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            return new JwtSecurityToken(
                issuer: issuer,
                audience: issuer,
                claims: claims,
                expires: isRefresh ? DateTime.Now.AddYears(10) : DateTime.Now.AddMinutes(TOKEN_EXPIRATION_MINUTES),
                signingCredentials: creds);
        }

        private void SaveSecurityToken(Customer customer, string refreshToken, string deviceUuid, string deviceModel, string devicePlatform, string deviceVersion, string deviceManufacturer, string serial)
        {
            CustomerSecurityToken securityToken = _customerSecurityTokenService.GetAllByUuid(deviceUuid).Where(x => x.CustomerId == customer.Id).FirstOrDefault();

            //var query = "DELETE FROM [dbo].[CustomerSecurityToken] WHERE CustomerId = @customerId;";
            //_dbContext.ExecuteSqlCommand(query, false, int.MaxValue, new SqlParameter[] { new SqlParameter("@customerId", customer.Id) });
            //query = @"
            //        INSERT INTO [dbo].[CustomerSecurityToken] ([GuidId]
            //          ,[CreatedOnUtc]
            //          ,[UpdatedOnUtc]
            //          ,[Deleted]
            //          ,[CustomerId]
            //          ,[Uuid]
            //          ,[Model]
            //          ,[Platform]
            //          ,[Version]
            //          ,[Manufacturer]
            //          ,[Serial]
            //          ,[RefreshToken]
            //          ,[FirebaseToken])
            //      VALUES (@guidId, @createdOnUtc, @updatedOnUtc, @deleted, @customerId, @uuid, @model, @platform, @version, @manufacturer, @serial, @refreshToken, @firebaseToken);
            //    ";

            //var uuidValue = string.IsNullOrEmpty(deviceUuid) ? DBNull.Value : (object)deviceUuid;
            //var modelValue = string.IsNullOrEmpty(deviceModel) ? DBNull.Value : (object)deviceModel;
            //var platformValue = string.IsNullOrEmpty(devicePlatform) ? DBNull.Value : (object)devicePlatform;
            //var versionValue = string.IsNullOrEmpty(deviceVersion) ? DBNull.Value : (object)deviceVersion;
            //var manufacturerValue = string.IsNullOrEmpty(deviceManufacturer) ? DBNull.Value : (object)deviceManufacturer;
            //var serialValue = string.IsNullOrEmpty(serial) ? DBNull.Value : (object)serial;

            //_dbContext.ExecuteSqlCommand(query, false, int.MaxValue,
            //        new SqlParameter[] {
            //            new SqlParameter("@guidId", Guid.NewGuid()),
            //            new SqlParameter("@createdOnUtc", DateTime.UtcNow),
            //            new SqlParameter("@updatedOnUtc", DateTime.UtcNow),
            //            new SqlParameter("@deleted", false),
            //            new SqlParameter("@customerId", customer.Id),
            //            new SqlParameter("@uuid", uuidValue),
            //            new SqlParameter("@model", modelValue),
            //            new SqlParameter("@platform", platformValue),
            //            new SqlParameter("@version", versionValue),
            //            new SqlParameter("@manufacturer", manufacturerValue),
            //            new SqlParameter("@serial", serialValue),
            //            new SqlParameter("@refreshToken", refreshToken),
            //            new SqlParameter("@firebaseToken", DBNull.Value)
            //    });

            if (securityToken == null)
            {
                CustomerSecurityToken newCustomerSecurityToken = new CustomerSecurityToken()
                {
                    CustomerId = customer.Id,
                    Manufacturer = deviceManufacturer,
                    Model = deviceModel,
                    Platform = devicePlatform,
                    RefreshToken = refreshToken,
                    Serial = serial,
                    Uuid = deviceUuid,
                    Version = deviceVersion
                };
                _customerSecurityTokenService.Insert(newCustomerSecurityToken);
            }
            else
            {
                //var query = @"
                //    UPDATE [dbo].[CustomerSecurityToken] 
                //    SET RefreshToken = @refreshToken, UpdatedOnUtc = @updatedOnUtc
                //    WHERE Uuid = @deviceUuid AND Deleted = 0 AND CustomerId = @customerId
                //";

                //_dbContext.ExecuteSqlCommand(query, false, int.MaxValue,
                //    new SqlParameter[] {
                //    new SqlParameter("@refreshToken", refreshToken),
                //    new SqlParameter("@updatedOnUtc", DateTime.UtcNow),
                //    new SqlParameter("@deviceUuid", deviceUuid),
                //    new SqlParameter("@customerId", customer.Id),
                //    });

                _customerSecurityTokenService.UpdateRefreshToken(securityToken, refreshToken);
            }
        }

        #endregion
    }
}
