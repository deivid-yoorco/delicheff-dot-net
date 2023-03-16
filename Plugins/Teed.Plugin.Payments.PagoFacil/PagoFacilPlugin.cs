using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payments.PagoFacil.Models;

namespace Teed.Plugin.Payments.PagoFacil
{
    public class PagoFacilPlugin : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly PagoFacilPaymentSettings _pagoFacilPaymentSettings;

        #endregion

        #region Ctor

        public PagoFacilPlugin(IOrderTotalCalculationService orderTotalCalculationService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            PagoFacilPaymentSettings pagoFacilPaymentSettings)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _localizationService = localizationService;
            _pagoFacilPaymentSettings = pagoFacilPaymentSettings;
            _webHelper = webHelper;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPagoFacil/Configure";
        }

        public override void Install()
        {
            _settingService.SaveSetting(new PagoFacilPaymentSettings
            {
                IdServicio = 1
            });

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<PagoFacilPaymentSettings>();

            base.Uninstall();
        }

        #endregion

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPagoFacil";
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _pagoFacilPaymentSettings.AdditionalFee, _pagoFacilPaymentSettings.AdditionalFeePercentage);
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public bool CanRePostProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            throw new NotImplementedException();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            #region payu

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var s = "{\"language\": \"es\"," +
                        "\"command\":\"SUBMIT_TRANSACTION\"," +
                        "\"merchant\":{" +
                            "\"apiKey\":\"4Vj8eK4rloUd272L48hsrarnUA\"," +
                            "\"apiLogin\":\"pRRXKOl8ikMmt9u\"" +
                        "}," +
                        "\"transaction\":{" +
                            "\"order\": {" +
                                "\"accountId\": \"512324\"," +
                                "\"referenceCode\":\"payment_test_00000001\"," +
                                "\"description\": \"payment test\"," +
                                "\"language\":\"es\"," +
                                "\"signature\":\"a88cba16c6fc54a4d31f696cfcbd41fc\"," +
                                "\"notifyUrl\": \"http://www.tes.com/confirmation\"," +
                                "\"additionalValues\": {" +
                                "\"TX_VALUE\": {" +
                                    "\"value\": 100," +
                                    "\"currency\": \"MXN\"" +
                                 "}" +
                            "}," +
                            "\"buyer\": {" +
                                "\"merchantBuyerId\": \"1\"," +
                                "\"fullName\": \"First name and second buyer  name\"," +
                                "\"emailAddress\": \"dvasquez@teed.com.mx\"," +
                                "\"contactPhone\": \"7563126\"," +
                                "\"dniNumber\": \"5415668464654\"," +
                                "\"shippingAddress\": {" +
                                    "\"street1\": \"Calle Salvador Alvarado\"," +
                                    "\"street2\": \"8 int 103\"," +
                                    "\"city\": \"Guadalajara\"," +
                                    "\"state\": \"Jalisco\"," +
                                    "\"country\": \"MX\"," +
                                    "\"postalCode\": \"000000\"," +
                                    "\"phone\": \"7563126\"" +
                                 "}" +
                             "}," +
                             "\"shippingAddress\": {" +
                                "\"street1\": \"Calle Salvador Alvarado\"," +
                                "\"street2\": \"8 int 103\"," +
                                "\"city\": \"Guadalajara\"," +
                                "\"state\": \"Jalisco\"," +
                                "\"country\": \"MX\"," +
                                "\"postalCode\": \"0000000\"," +
                                "\"phone\": \"7563126\"" +
                              "}" +
                         "}," +
                         "\"payer\": {" +
                            "\"merchantPayerId\": \"1\"," +
                            "\"fullName\": \"First name and second payer name\"," +
                            "\"emailAddress\": \"dvasquez@teed.com.mx\"," +
                            "\"birthdate\": \"1985-05-25\"," +
                            "\"contactPhone\": \"7563126\"," +
                            "\"dniNumber\": \"5415668464654\"," +
                            "\"billingAddress\": {" +
                                "\"street1\": \"Calle Zaragoza esquina\"," +
                                "\"street2\": \"calle 5 de Mayo\"," +
                                "\"city\": \"Monterrey\"," +
                                "\"state\": \"Nuevo Leon\"," +
                                "\"country\": \"MX\"," +
                                "\"postalCode\": \"64000\"," +
                                "\"phone\": \"7563126\"" +
                             "}" +
                          "}," +
                          "\"creditCard\": {" +
                          "\"number\": \"5513550994092123\"," +
                          "\"securityCode\": \"271\"," +
                          "\"expirationDate\": \"2022/08\"," +
                          "\"name\": \"REJECTED\"" +
                        "}," +
                        "\"extraParameters\": {" +
                            "\"INSTALLMENTS_NUMBER\": 1" +
                        "}," +
                        "\"type\": \"AUTHORIZATION_AND_CAPTURE\"," +
                        "\"paymentMethod\": \"VISA\"," +
                        "\"paymentCountry\": \"MX\"," +
                        "\"deviceSessionId\": \"vghs6tvkcle931686k1900o6e1\"," +
                        "\"ipAddress\": \"127.0.0.1\"," +
                        "\"cookie\": \"pt1t38347bs6jc9ruv2ecpv7o2\"," +
                        "\"userAgent\":\"Mozilla/5.0 (Windows NT 5.1; rv:18.0) Gecko/20100101 Firefox/18.0\"" +
                      "}," +
                      "\"test\": false" +
                    "}";

                streamWriter.Write(s);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var response = streamReader.ReadToEnd();
            }

            #endregion

            #region pagofacil

            //var client = new RestClient("https://sandbox.pagofacil.tech/Wsrtransaccion/index/format/json");
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("Postman-Token", "b3e229a9-a450-46c7-8ae8-ccf0c10ea74f");
            //request.AddHeader("cache-control", "no-cache");
            //request.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
            //request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW",
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"method\"\r\n\r\ntransaccion\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[nombre]\"\r\n\r\n" + processPaymentRequest.CreditCardName.Split(' ')[0] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[apellidos]\"\r\n\r\n" + processPaymentRequest.CreditCardName.Split(' ')[1] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[numeroTarjeta]\"\r\n\r\n" + processPaymentRequest.CreditCardNumber + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[cvt]\"\r\n\r\n" + processPaymentRequest.CreditCardCvv2 + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[cp]\"\r\n\r\n" + processPaymentRequest.CustomValues["zcp"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[mesExpiracion]\"\r\n\r\n" + processPaymentRequest.CreditCardExpireMonth + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[anyoExpiracion]\"\r\n\r\n" + processPaymentRequest.CreditCardExpireYear + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[monto]\"\r\n\r\n" + processPaymentRequest.OrderTotal + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[idSucursal]\"\r\n\r\ne147ee31531d815e2308d6d6d39929ab599deb98\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[idUsuario]\"\r\n\r\nf541b3f11f0f9b3fb33499684f22f6d711f2af58\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[idServicio]\"\r\n\r\n3\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[email]\"\r\n\r\n" + processPaymentRequest.CustomValues["email"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[telefono]\"\r\n\r\n" + processPaymentRequest.CustomValues["phone"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[celular]\"\r\n\r\n" + processPaymentRequest.CustomValues["phone"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[calleyNumero]\"\r\n\r\n" + processPaymentRequest.CustomValues["address1"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[colonia]\"\r\n\r\n" + processPaymentRequest.CustomValues["address2"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[municipio]\"\r\n\r\n" + processPaymentRequest.CustomValues["city"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[estado]\"\r\n\r\n" + processPaymentRequest.CustomValues["state"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[pais]\"\r\n\r\n" + processPaymentRequest.CustomValues["country"] + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[idPedido]\"\r\n\r\n" + processPaymentRequest.OrderGuid + "\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[param1]\"\r\n\r\n" + processPaymentRequest.StoreId + "\r\n" +
            //    //"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[param2]\"\r\n\r\n\r\n" +
            //    //"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[param3]\"\r\n\r\n\r\n" +
            //    //"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[param4]\"\r\n\r\n\r\n" +
            //    //"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[param5]\"\r\n\r\n\r\n" +
            //    //"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[httpUserAgent]\"\r\n\r\n\r\n" +
            //    //"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"data[ip]\"\r\n\r\n\r\n" +
            //    "------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

            //var res = JsonConvert.DeserializeObject<JsonModel>(response.Content);

            //if(res.WebServices_Transacciones.transaccion.pf_message == "Transaccion exitosa")
            //{
            //    result.AllowStoringCreditCardNumber = false;
            //    result.AuthorizationTransactionCode = res.WebServices_Transacciones.transaccion.transaccion;
            //    result.AuthorizationTransactionId = res.WebServices_Transacciones.transaccion.idTransaccion;
            //    result.AuthorizationTransactionResult = res.WebServices_Transacciones.transaccion.pf_message;
            //    result.AvsResult = "";
            //    result.CaptureTransactionId = res.WebServices_Transacciones.transaccion.idTransaccion;
            //    result.CaptureTransactionResult = res.WebServices_Transacciones.transaccion.transaccion;
            //    result.Cvv2Result = "";
            //    result.Errors = new List<string>();
            //    result.NewPaymentStatus = Nop.Core.Domain.Payments.PaymentStatus.Paid;
            //    result.RecurringPaymentFailed = false;
            //    result.SubscriptionTransactionId = "";
            //}
            //else
            //{
            //    var error = new List<string>();
            //    error.Add(res.WebServices_Transacciones.transaccion.pf_message);
            //    result.Errors = error;
            //    result.NewPaymentStatus = Nop.Core.Domain.Payments.PaymentStatus.Pending;
            //}

            #endregion

            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        #region Properties

        public bool SupportCapture
        {
            get { return false; }
        }

        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        public bool SupportRefund
        {
            get { return false; }
        }

        public bool SupportVoid
        {
            get { return false; }
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Standard; }
        }

        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        public string PaymentMethodDescription
        {
            get { return "Pago con tarjeta."; }
        }

        #endregion
    }

    public class Principal
    {
        public string method;
        public Data data;
    }

    public class Data
    {
        public string nombre;
        public string apellidos;
        public string numeroTarjeta;
        public string cvt;
        public string mesExpiracion;
        public string anyoExpiracion;
        public string monto;
        public string idSucursal;
        public string idUsuario;
        public string idServicio;
        public string cp;
        public string email;
        public string telefono;
        public string celular;
        public string calleyNumero;
        public string colonia;
        public string municipio;
        public string estado;
        public string pais;
        public string idPedido;
    }
}
