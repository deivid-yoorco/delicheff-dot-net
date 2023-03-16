using FirebaseAdmin.Messaging;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Teed.Plugin.Api.Domain.Notifications;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Helper;
using Teed.Plugin.Api.Models;
using Teed.Plugin.Api.Services;
using Notification = Teed.Plugin.Api.Domain.Notifications.Notification;
using Teed.Plugin.Api.Security;
using Nop.Services.Helpers;

namespace Teed.Plugin.Api.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class TeedApiController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly NotificationService _notificationService;
        private readonly QueuedNotificationService _queuedNotificationService;
        private readonly CustomerSecurityTokenService _customerSecurityTokenService;
        private readonly ICustomerService _customerService;
        private readonly IProductTagService _tagService;

        public TeedApiController(IWorkContext workContext,
            IStoreService storeService,
            IPermissionService permissionService,
            IPictureService pictureService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IProductService productService,
            ICategoryService categoryService,
            NotificationService notificationService,
            QueuedNotificationService queuedNotificationService,
            CustomerSecurityTokenService customerSecurityTokenService,
            ICustomerService customerService,
            IProductTagService tagService
            )
        {
            _workContext = workContext;
            _storeService = storeService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _settingService = settingService;
            _localizationService = localizationService;
            _productService = productService;
            _categoryService = categoryService;
            _notificationService = notificationService;
            _customerSecurityTokenService = customerSecurityTokenService;
            _customerService = customerService;
            _queuedNotificationService = queuedNotificationService;
            _tagService = tagService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Configure))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<TeedApiPluginSettings>(storeScope);

            var path = System.IO.Directory.GetCurrentDirectory() + "/wwwroot/images/app-media/welcome-background.jpg";
            var PhysicalFile = new FileInfo(@path);
            var test = Image.FromFile(@path);

            FormFile formFile = null;
            string welcomeImgeB64 = null;
            using (var stream = new MemoryStream())
            {
                await PhysicalFile.OpenRead().CopyToAsync(stream);
                formFile = new FormFile(stream, 0, stream.Length, "welcome-background.jpg", @path);
                byte[] data = stream.ToArray();
                welcomeImgeB64 = "data:image/jpeg;base64," + Convert.ToBase64String(data);

                stream.Close();
            }

            var model = new ConfigurationModel
            {
                //Banner
                BannerPicture1Id = settings.BannerPicture1Id,
                BannerPicture1TypeId = settings.BannerPicture1TypeId,
                BannerPicture1AdditionalData = settings.BannerPicture1AdditionalData,

                BannerPicture2Id = settings.BannerPicture2Id,
                BannerPicture2TypeId = settings.BannerPicture2TypeId,
                BannerPicture2AdditionalData = settings.BannerPicture2AdditionalData,

                BannerPicture3Id = settings.BannerPicture3Id,
                BannerPicture3TypeId = settings.BannerPicture3TypeId,
                BannerPicture3AdditionalData = settings.BannerPicture3AdditionalData,

                BannerPicture4Id = settings.BannerPicture4Id,
                BannerPicture4TypeId = settings.BannerPicture4TypeId,
                BannerPicture4AdditionalData = settings.BannerPicture4AdditionalData,

                BannerPicture5Id = settings.BannerPicture5Id,
                BannerPicture5TypeId = settings.BannerPicture5TypeId,
                BannerPicture5AdditionalData = settings.BannerPicture5AdditionalData,

                FacebookAppId = settings.FacebookAppId,
                FacebookAppSecret = settings.FacebookAppSecret,
                SelectedProductsIds = settings.SelectedProductsIds,
                CategoryId = settings.CategoryId,
                ProductsHeader = settings.ProductsHeader,
                ActiveStoreScopeConfiguration = storeScope,
                AbandonedShoppingCartBody = settings.AbandonedShoppingCartBody,
                AbandonedShoppingCartTitle = settings.AbandonedShoppingCartTitle,
                OrderCompletedBody = settings.OrderCompletedBody,
                OrderCompletedTitle = settings.OrderCompletedTitle,
                OrderPaidBody = settings.OrderPaidBody,
                OrderPaidTitle = settings.OrderPaidTitle,
                OrderPlacedBody = settings.OrderPlacedBody,
                OrderPlacedTitle = settings.OrderPlacedTitle,
                WelcomeImage = formFile,
                WelcomeImageB64 = welcomeImgeB64,
                IsActiveCategories = settings.IsActiveCategories,
                CategoriesHeader = settings.CategoriesHeader,
                SelectedCategoriesIds = settings.SelectedCategoriesIds,
                SmartlookIsActive = settings.SmartlookIsActive,
                SmartlookProjectKey = settings.SmartlookProjectKey,
                ActionTypes = Enum.GetValues(typeof(ActionType)).Cast<ActionType>()
                .Where(x => x != ActionType.OpenApp)
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName()
                }).ToList()
            };

            if (storeScope > 0)
            {
                model.BannerPicture1Id_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture1Id, storeScope);
                model.BannerPicture1TypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture1TypeId, storeScope);
                model.BannerPicture1AdditionalData_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture1AdditionalData, storeScope);

                model.BannerPicture2Id_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture2Id, storeScope);
                model.BannerPicture2TypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture2TypeId, storeScope);
                model.BannerPicture2AdditionalData_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture2AdditionalData, storeScope);

                model.BannerPicture3Id_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture3Id, storeScope);
                model.BannerPicture3TypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture3TypeId, storeScope);
                model.BannerPicture3AdditionalData_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture3AdditionalData, storeScope);

                model.BannerPicture4Id_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture4Id, storeScope);
                model.BannerPicture4TypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture4TypeId, storeScope);
                model.BannerPicture4AdditionalData_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture4AdditionalData, storeScope);

                model.BannerPicture5Id_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture5Id, storeScope);
                model.BannerPicture5TypeId_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture5TypeId, storeScope);
                model.BannerPicture5AdditionalData_OverrideForStore = _settingService.SettingExists(settings, x => x.BannerPicture5AdditionalData, storeScope);
            }

            ViewData["Products"] = SelectListHelper.GetProductsList(_productService);
            ViewData["Categories"] = SelectListHelper.GetCategoriesList(_categoryService);

            return View("~/Plugins/Teed.Plugin.Api/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Configure))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var teedApiSettings = _settingService.LoadSetting<TeedApiPluginSettings>(storeScope);

            //get previous picture identifiers
            var previousPictureIds = new[]
            {
                teedApiSettings.BannerPicture1Id,
                teedApiSettings.BannerPicture2Id,
                teedApiSettings.BannerPicture3Id,
                teedApiSettings.BannerPicture4Id,
                teedApiSettings.BannerPicture5Id,
            };

            var path = System.IO.Directory.GetCurrentDirectory() + "/wwwroot/images/app-media/welcome-background.jpg";

            if (model.WelcomeImage != null)
            {
                using (var stream = new MemoryStream())
                {
                    await model.WelcomeImage.CopyToAsync(stream);
                    byte[] data = stream.ToArray();
                    if (data.Length > 0)
                    {
                        MemoryStream ms = new MemoryStream(data);
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.Close();
                    }
                    stream.Close();
                }
            }


            teedApiSettings.BannerPicture1Id = model.BannerPicture1Id;
            teedApiSettings.BannerPicture1TypeId = model.BannerPicture1TypeId;
            teedApiSettings.BannerPicture1AdditionalData = model.BannerPicture1AdditionalData;

            teedApiSettings.BannerPicture2Id = model.BannerPicture2Id;
            teedApiSettings.BannerPicture2TypeId = model.BannerPicture2TypeId;
            teedApiSettings.BannerPicture2AdditionalData = model.BannerPicture2AdditionalData;

            teedApiSettings.BannerPicture3Id = model.BannerPicture3Id;
            teedApiSettings.BannerPicture3TypeId = model.BannerPicture3TypeId;
            teedApiSettings.BannerPicture3AdditionalData = model.BannerPicture3AdditionalData;

            teedApiSettings.BannerPicture4Id = model.BannerPicture4Id;
            teedApiSettings.BannerPicture4TypeId = model.BannerPicture4TypeId;
            teedApiSettings.BannerPicture4AdditionalData = model.BannerPicture4AdditionalData;

            teedApiSettings.BannerPicture5Id = model.BannerPicture5Id;
            teedApiSettings.BannerPicture5TypeId = model.BannerPicture5TypeId;
            teedApiSettings.BannerPicture5AdditionalData = model.BannerPicture5AdditionalData;

            teedApiSettings.FacebookAppId = model.FacebookAppId;
            teedApiSettings.FacebookAppSecret = model.FacebookAppSecret;
            teedApiSettings.CategoryId = model.CategoryId;
            teedApiSettings.ProductsHeader = model.ProductsHeader;
            teedApiSettings.SelectedProductsIds = model.SelectedProductsIds.ToList();
            teedApiSettings.AbandonedShoppingCartBody = model.AbandonedShoppingCartBody;
            teedApiSettings.AbandonedShoppingCartTitle = model.AbandonedShoppingCartTitle;
            teedApiSettings.OrderCompletedBody = model.OrderCompletedBody;
            teedApiSettings.OrderCompletedTitle = model.OrderCompletedTitle;
            teedApiSettings.OrderPaidBody = model.OrderPaidBody;
            teedApiSettings.OrderPaidTitle = model.OrderPaidTitle;
            teedApiSettings.OrderPlacedBody = model.OrderPlacedBody;
            teedApiSettings.OrderPlacedTitle = model.OrderPlacedTitle;
            teedApiSettings.IsActiveCategories = model.IsActiveCategories;
            teedApiSettings.CategoriesHeader = model.CategoriesHeader;
            teedApiSettings.SelectedCategoriesIds = model.SelectedCategoriesIds.ToList();
            teedApiSettings.SmartlookProjectKey = model.SmartlookProjectKey;
            teedApiSettings.SmartlookIsActive = model.SmartlookIsActive;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture1Id, model.BannerPicture1Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture1TypeId, model.BannerPicture1TypeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture1AdditionalData, model.BannerPicture1AdditionalData_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture2Id, model.BannerPicture2Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture2TypeId, model.BannerPicture2TypeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture2AdditionalData, model.BannerPicture2AdditionalData_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture3Id, model.BannerPicture3Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture3TypeId, model.BannerPicture3TypeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture3AdditionalData, model.BannerPicture3AdditionalData_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture4Id, model.BannerPicture4Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture4TypeId, model.BannerPicture4TypeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture4AdditionalData, model.BannerPicture4AdditionalData_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture5Id, model.BannerPicture5Id_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture5TypeId, model.BannerPicture5TypeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.BannerPicture5AdditionalData, model.BannerPicture5AdditionalData_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.FacebookAppId, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.FacebookAppSecret, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.CategoryId, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.ProductsHeader, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.SelectedProductsIds, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.IsActiveCategories, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.CategoriesHeader, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.SelectedCategoriesIds, true, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.OrderPlacedTitle, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.OrderPlacedBody, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.OrderPaidTitle, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.OrderPaidBody, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.OrderCompletedTitle, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.OrderCompletedBody, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.AbandonedShoppingCartBody, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.AbandonedShoppingCartTitle, true, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.SmartlookIsActive, true, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(teedApiSettings, x => x.SmartlookProjectKey, true, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            //get current picture identifiers
            var currentPictureIds = new[]
            {
                teedApiSettings.BannerPicture1Id,
                teedApiSettings.BannerPicture2Id,
                teedApiSettings.BannerPicture3Id,
                teedApiSettings.BannerPicture4Id,
                teedApiSettings.BannerPicture5Id,
            };

            //delete an old picture (if deleted or updated)
            foreach (var pictureId in previousPictureIds.Except(currentPictureIds))
            {
                var previousPicture = _pictureService.GetPictureById(pictureId);
                if (previousPicture != null)
                    _pictureService.DeletePicture(previousPicture);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return await Configure();
        }

        [HttpPost]
        public IActionResult GetActionSelect(int typeId)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Configure))
                return AccessDeniedView();

            if (typeId > 0)
            {
                switch (typeId)
                {
                    case (int)ActionType.OpenCategory:
                        var categories = _categoryService.GetAllCategories()
                            .Where(x => x.Published)
                            .Select(x => new {
                                x.Id,
                                Name = x.GetFormattedBreadCrumb(_categoryService)
                            }).ToList();
                        return Json(categories);
                    case (int)ActionType.OpenProduct:
                        var products = _productService.GetAllProductsQuery()
                            .Where(x => x.Published && !x.Deleted)
                            .Select(x => new {
                                x.Id,
                                x.Name
                            }).ToList();
                        return Json(products);
                    case (int)ActionType.SearchTagOrKeyword:
                        var tags = _tagService.GetAllProductTags()
                            .Select(x => new {
                                Id = x.Name,
                                x.Name
                            }).ToList();
                        return Json(tags);
                    default:
                        break;
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult CategoryListData()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Configure))
                return AccessDeniedView();

            var categories = _categoryService.GetAllCategories();
            var elements = categories.Select(x => new CategoriesListModel
            {
                Id = x.Id,
                Category = x.Name
            }).ToList();

            return Json(elements);
        }

        public IActionResult NotificationList()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Api/Views/Notifications/NotificationList.cshtml");
        }

        [HttpPost]
        public IActionResult NotificationListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            DateTime today = DateTime.Now.Date;
            var notificationsQuery = _notificationService.GetAll().Where(x => !x.IsSystemNotification).OrderByDescending(x => x.CreatedOnUtc);
            var pagedList = new PagedList<Notification>(notificationsQuery, command.Page - 1, command.PageSize);
            var notificationIds = pagedList.Select(x => x.Id).ToList();
            List<QueuedNotificationListModel> queuedNotifications = _queuedNotificationService.GetAll()
                .Where(x => notificationIds.Contains(x.NotificationId))
                .Select(x => new QueuedNotificationListModel()
                {
                    SentAtUtc = x.SentAtUtc,
                    NotificationId = x.NotificationId
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new NotificationData()
                {
                    Id = x.Id,
                    Date = x.DontSendBeforeDateUtc.ToString("D", new CultureInfo("es-MX")),
                    Title = x.Title,
                    Content = x.Body,
                    Status = GetStatusNotification(x, queuedNotifications)
                }).ToList(),
                Total = notificationsQuery.Count()
            };

            return Json(gridModel);
        }

        public string GetStatusNotification(Notification notification, List<QueuedNotificationListModel> queuedNotifications)
        {
            string status;
            var filteredQueuedNotifications = queuedNotifications.Where(x => x.NotificationId == notification.Id);
            if (notification.IsCancelled)
            {
                status = "CANCELADO";
            }
            else if (filteredQueuedNotifications.Where(x => !x.SentAtUtc.HasValue).Any())
            {
                status = "PENDIENTE";
            }
            else if (filteredQueuedNotifications.Where(x => x.SentAtUtc.HasValue).Any())
            {
                status = "ENVIADA";
            }
            else
            {
                status = "NO ENVIADA";
            }
            return status;
        }

        public IActionResult NotificationCreate()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            var model = new NotificationModel()
            {
                DontSendBeforeDate = DateTime.Now,
                SendImmediately = true,
                ActionTypes = Enum.GetValues(typeof(ActionType)).Cast<ActionType>()
                .Where(x => x != ActionType.None)
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName()
                }).ToList()
            };

            ViewData["Customers"] = GetCustomersToSelect();
            return View("~/Plugins/Teed.Plugin.Api/Views/Notifications/NotificationCreate.cshtml", model);
        }

        [HttpPost]
        public IActionResult NotificationCreate(NotificationModel model)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó una notificación.\n";
                Notification newNotification = new Notification()
                {
                    Title = model.Title,
                    Body = model.Body,
                    DontSendBeforeDateUtc = model.SendImmediately ? DateTime.UtcNow : model.DontSendBeforeDate.ToUniversalTime(),
                    Log = logText,
                    ActionTypeId = model.ActionTypeId,
                    CustomerIds = model.CustomersIds != null && model.CustomersIds.Count() > 0 ? string.Join(",", model.CustomersIds.ToList()) : null,
                    AdditionalData = model.AdditionalData,
                };
                _notificationService.Insert(newNotification);

                return RedirectToAction("NotificationEdit", new { id = newNotification.Id });
            }

            return View(model);
        }

        public IActionResult NotificationEdit(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            var notification = _notificationService.GetAll().Where(x => x.Id == id).Include(x => x.QueuedNotifications).FirstOrDefault();
            if (notification == null) return NotFound();

            var queuedNotifications = notification.QueuedNotifications.Where(x => !x.Deleted);
            var model = new NotificationModel()
            {
                Id = notification.Id,
                Title = notification.Title,
                Body = notification.Body,
                DontSendBeforeDate = notification.DontSendBeforeDateUtc.ToLocalTime(),
                IsCancelled = notification.IsCancelled,
                Log = notification.Log,
                ActionTypeId = notification.ActionTypeId,
                AdditionalData = notification.AdditionalData,
                CustomersIds = notification.CustomerIds?.Split(',').Select(x => int.Parse(x)).ToList(),
                ActionTypes = Enum.GetValues(typeof(ActionType)).Cast<ActionType>()
                .Where(x => x != ActionType.None)
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName()
                }).ToList()
            };

            if (DateTime.UtcNow > notification.DontSendBeforeDateUtc || notification.IsCancelled)
                model.BlockChanges = true;

            var customers = GetCustomersToSelect();
            model.CustomersToSend = notification.CustomerIds != null ? model.CustomersIds.Count : model.BlockChanges ? queuedNotifications.Count() == 0 ? customers.Count : queuedNotifications.Count() : customers.Count;
            model.CustomersSent = queuedNotifications.Where(x => x.SentAtUtc.HasValue).Count();
            model.ErrorsCount = queuedNotifications.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Count();
            model.CompletedAt = queuedNotifications.OrderByDescending(x => x.SentAtUtc).Select(x => x.SentAtUtc).FirstOrDefault();

            ViewData["Customers"] = customers;
            return View("~/Plugins/Teed.Plugin.Api/Views/Notifications/NotificationEdit.cshtml", model);
        }

        public IActionResult CancelNotification(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            var notification = _notificationService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            if (notification == null) return NotFound();
            notification.IsCancelled = true;
            notification.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) canceló la notificación.\n";
            _notificationService.Update(notification);
            return RedirectToAction("NotificationEdit", new { id });
        }

        [HttpGet]
        public IActionResult DownloadNotificationReport(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            var queuedNotifications = _queuedNotificationService.GetAll().Where(x => x.NotificationId == id).Include(x => x.Notification).ToList();
            var customers = _customerService.GetAllCustomers();

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("CONFIDENCIAL");
                    int row = 1;

                    worksheet.Cells[row, 1].Value = "Título";
                    worksheet.Cells[row, 2].Value = "Mensaje";
                    worksheet.Cells[row, 3].Value = "Usuario";
                    worksheet.Cells[row, 4].Value = "Fecha de envío";
                    worksheet.Cells[row, 5].Value = "Mensaje de error";
                    worksheet.Cells[row, 6].Value = "Id del mensaje";

                    foreach (var item in queuedNotifications)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = item.Notification.Title;
                        worksheet.Cells[row, 2].Value = item.Notification.Body;
                        worksheet.Cells[row, 3].Value = customers.Where(x => x.Id == item.CustomerId).FirstOrDefault()?.GetFullName();
                        worksheet.Cells[row, 4].Value = item.SentAtUtc.Value.ToLocalTime();
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mm-yyyy hh:mm:ss AM/PM";
                        worksheet.Cells[row, 5].Value = item.ErrorMessage;
                        worksheet.Cells[row, 6].Value = item.MessageId;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return File(stream.ToArray(), MimeTypes.TextXlsx, $"notificaciones_{id}.xlsx");
            }
        }

        [HttpPost]
        public IActionResult NotificationEdit(NotificationModel model)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.Notifications))
                return AccessDeniedView();

            DateTime now = DateTime.Now;
            if (model.SendImmediately)
            {
                model.DontSendBeforeDate = now;
            }

            if (ModelState.IsValid)
            {
                if (model.DontSendBeforeDate < now)
                {
                    ModelState.AddModelError(null, "No puedes ingresar como fecha de envío una fecha que ya pasó.");
                    return View(model);
                }

                var notification = _notificationService.GetAll().Where(x => x.Id == model.Id).Include(x => x.QueuedNotifications).FirstOrDefault();

                var logText = string.Empty;
                if (notification.Title != model.Title)
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})  cambió el título de la notificación de {notification.Title} a {model.Title}.\n";
                    notification.Title = model.Title;
                    notification.Log += logText;
                }
                if (notification.Body != model.Body)
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})  cambió el contenido de la notificación de {notification.Body} a {model.Body}.\n";
                    notification.Body = model.Body;
                    notification.Log += logText;
                }

                if ((notification.DontSendBeforeDateUtc.ToLocalTime() != model.DontSendBeforeDate))
                {
                    logText = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id})  cambió la fecha de envío de la notificación de {notification.DontSendBeforeDateUtc.ToLocalTime()} a {model.DontSendBeforeDate}.\n";
                    notification.DontSendBeforeDateUtc = model.DontSendBeforeDate.ToUniversalTime();
                    notification.Log += logText;
                }

                notification.CustomerIds = model.CustomersIds != null ? string.Join(",", model.CustomersIds) : null;
                _notificationService.Update(notification);

                return RedirectToAction("NotificationEdit", new { id = model.Id });
            }

            return View(model);
        }

        public List<SelectListItem> GetCustomersToSelect()
        {
            List<int> secureCustumerIds = _customerSecurityTokenService.GetAll()
                .Where(x => x.FirebaseToken != null)
                .GroupBy(x => x.CustomerId)
                .Select(x => x.Key)
                .ToList();

            var customers = _customerService.GetAllCustomersQuery()
                .Where(x => secureCustumerIds.Contains(x.Id))
                .AsEnumerable()
                .Select(x => new SelectListItem()
                {
                    Text = x.Email,
                    Value = x.Id.ToString(),
                }).ToList();

            return customers;
        }
    }
}
