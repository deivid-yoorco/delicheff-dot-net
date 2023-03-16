using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.Estafeta.Controllers;
using Nop.Plugin.Shipping.Estafeta.Data;
using Nop.Plugin.Shipping.Estafeta.Domain.Shopping;
using Nop.Plugin.Shipping.Estafeta.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Tasks;
using System;

namespace Nop.Plugin.Shipping.Estafeta
{
    public class EstafetaComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IMeasureService _measureService;
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly EstafetaSettings _estafetaSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;

        public ShippingRateComputationMethodType ShippingRateComputationMethodType => throw new NotImplementedException();

        private readonly ShoppingObjectContext _context;

        private readonly ShoppingService _shoppingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor
        public EstafetaComputationMethod(IMeasureService measureService,
            IShippingService shippingService, ISettingService settingService,
            EstafetaSettings estafetaSettings, IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            IWebHelper webHelper, ShoppingObjectContext context, ShoppingService shoppingService,
            IWorkContext workContext, IScheduleTaskService scheduleTaskService)
        {
            this._measureService = measureService;
            this._shippingService = shippingService;
            this._settingService = settingService;
            this._estafetaSettings = estafetaSettings;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._context = context;
            this._shoppingService = shoppingService;
            this._workContext = workContext;
            this._scheduleTaskService = scheduleTaskService;
        }
        #endregion

        #region Utilities

        private FrecuenciaCotizadorEstafeta.Respuesta[] CreateRateRequest(GetShippingOptionRequest getShippingOptionRequest)
        {
            FrecuenciaCotizadorEstafeta.ServiceSoapClient client = new FrecuenciaCotizadorEstafeta.ServiceSoapClient(FrecuenciaCotizadorEstafeta.ServiceSoapClient.EndpointConfiguration.ServiceSoap12);

            string[] envio = { getShippingOptionRequest.ZipPostalCodeFrom };
            string[] destino = { getShippingOptionRequest.ShippingAddress.ZipPostalCode };

            FrecuenciaCotizadorEstafeta.Respuesta[] result = null;

            for (int i = 0; i < getShippingOptionRequest.Items.Count; i++)
            {
                var alto = getShippingOptionRequest.Items[i].ShoppingCartItem.Product.Height;
                var ancho = getShippingOptionRequest.Items[i].ShoppingCartItem.Product.Width;
                var largo = getShippingOptionRequest.Items[i].ShoppingCartItem.Product.Length;
                var peso = getShippingOptionRequest.Items[i].ShoppingCartItem.Product.Weight;

                if (alto == 0 || ancho == 0 || largo == 0 || peso == 0)
                {
                    return null;
                }

                FrecuenciaCotizadorEstafeta.TipoEnvio tEnvio = new FrecuenciaCotizadorEstafeta.TipoEnvio
                {
                    Alto = (Double)alto,
                    Ancho = (Double)ancho,
                    EsPaquete = true,
                    Largo = (Double)largo,
                    Peso = (Double)peso
                };

                FrecuenciaCotizadorEstafeta.Respuesta[] res = client.FrecuenciaCotizadorAsync(_estafetaSettings.CotizadorId, _estafetaSettings.Login, _estafetaSettings.Password, false, true, tEnvio, envio, destino).Result;

                if (getShippingOptionRequest.Items[i].ShoppingCartItem.Quantity > 1)
                {
                    for (int j = 0; j < res.Length; j++)
                    {
                        for (int it = 0; it < res[j].TipoServicio.Length; it++)
                        {
                            res[j].TipoServicio[it].CostoTotal *= getShippingOptionRequest.Items[i].ShoppingCartItem.Quantity;
                        }
                    }
                }

                if (i == 0)
                {
                    result = res;
                }
                else
                {
                    for (int j = 0; j < result.Length; j++)
                    {
                        for (int k = 0; k < result[j].TipoServicio.Length; k++)
                        {
                            result[j].TipoServicio[k].CostoTotal += res[j].TipoServicio[k].CostoTotal;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Methods

        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null)
            {
                response.AddError("No shipment items");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress.Country == null)
            {
                response.AddError("Shipping country is not set");
                return response;
            }

            FrecuenciaCotizadorEstafeta.Respuesta[] result = CreateRateRequest(getShippingOptionRequest);

            if (result == null)
            {
                response.AddError("Para calcular el envío notifique al administrador que algún producto no tiene propiedades establecidas");
                return response;
            }

            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[i].TipoServicio.Length; j++)
                {
                    if (result[i].TipoServicio[j].DescripcionServicio == "Terrestre")
                    {
                        ShippingOption options = new ShippingOption
                        {
                            Description = result[i].TipoServicio[j].DescripcionServicio,
                            Name = "Estafeta",
                            Rate = Convert.ToDecimal(result[i].TipoServicio[j].CostoTotal)
                        };
                        response.ShippingOptions.Add(options);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ShippingEstafeta/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            ScheduleTask scheduleTask = new ScheduleTask()
            {
                Enabled = true,
                Name = "Cosulta de envíos Estafeta",
                Seconds = 1800,
                StopOnError = false,
                Type = "Nop.Plugin.Shipping.Estafeta.Controllers.ScheduleTaskEstafeta"
            };
            _scheduleTaskService.InsertTask(scheduleTask);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Url", "URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Url.Hint", "Specify Estafeta URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Key.Hint", "Specify Estafeta key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Password", "Password");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Password.Hint", "Specify Estafeta password.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AccountNumber", "Account number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AccountNumber.Hint", "Specify Estafeta account number.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.MeterNumber", "Meter number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.MeterNumber.Hint", "Specify Estafeta meter number.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.UseResidentialRates", "Use residential rates");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.UseResidentialRates.Hint", "Check to use residential rates.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.ApplyDiscounts", "Use discounted rates");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.ApplyDiscounts.Hint", "Check to use discounted rates (instead of list rates).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AdditionalHandlingCharge", "Additional handling charge");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AdditionalHandlingCharge.Hint", "Enter additional handling fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.CarrierServices", "Carrier Services Offered");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.CarrierServices.Hint", "Select the services you want to offer to customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PassDimensions", "Pass dimensions");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PassDimensions.Hint", "Check if you want to pass package dimensions when requesting rates.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingType", "Packing type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingType.Hint", "Choose preferred packing type.");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.PackingType.PackByDimensions", "Pack by dimensions");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.PackingType.PackByOneItemPerPackage", "Pack by one item per package");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.PackingType.PackByVolume", "Pack by volume");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingPackageVolume", "Package volume");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingPackageVolume.Hint", "Enter your package volume.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.DropoffType", "Dropoff Type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.DropoffType.Hint", "Choose preferred dropoff type.");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.BusinessServiceCenter", "Business service center");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.DropBox", "Drop box");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.RegularPickup", "Regular pickup");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.RequestCourier", "Request courier");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.Station", "Station");

            _context.Install();

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            //_settingService.DeleteSetting<EstafetaSettings>();

            ScheduleTask scheduleTask = _scheduleTaskService.GetTaskByType("Nop.Plugin.Shipping.Estafeta.Controllers.ScheduleTaskEstafeta");

            _scheduleTaskService.DeleteTask(scheduleTask);

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Url");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Url.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Key");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Password");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.Password.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AccountNumber");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AccountNumber.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.MeterNumber");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.MeterNumber.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.UseResidentialRates");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.UseResidentialRates.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.ApplyDiscounts");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.ApplyDiscounts.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AdditionalHandlingCharge");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.AdditionalHandlingCharge.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.CarrierServices");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.CarrierServices.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PassDimensions");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PassDimensions.Hint");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.PackingType.PackByDimensions");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.PackingType.PackByOneItemPerPackage");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.PackingType.PackByVolume");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingType");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingType.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingPackageVolume");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.PackingPackageVolume.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.DropoffType");
            this.DeletePluginLocaleResource("Plugins.Shipping.Estafeta.Fields.DropoffType.Hint");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.BusinessServiceCenter");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.DropBox");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.RegularPickup");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.RequestCourier");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Estafeta.DropoffType.Station");

            _context.Uninstall();

            base.Uninstall();
        }

        #endregion

        #region Properties

        ///// <summary>
        ///// Gets a shipping rate computation method type
        ///// </summary>
        //public ShippingRateComputationMethodType ShippingRateComputationMethodType
        //{
        //    get { return ShippingRateComputationMethodType.Realtime; }
        //}

        ///// <summary>
        ///// Gets a shipment tracker
        ///// </summary>
        //public IShipmentTracker ShipmentTracker
        //{
        //    get { return new FedexShipmentTracker(_logger, _fedexSettings); }
        //}

        public IShipmentTracker ShipmentTracker
        {
            get { return null; }
        }

        #endregion
    }
}
