using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Shipping.Estafeta;
using Nop.Plugin.Shipping.Estafeta.Domain;
using Nop.Plugin.Shipping.Estafeta.Domain.Shopping;
using Nop.Plugin.Shipping.Estafeta.Models;
using Nop.Plugin.Shipping.Estafeta.Services;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Nop.Pluggin.Shipping.Estafeta.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ShippingEstafetaController : BasePluginController
    {
        #region Fields

        private readonly EstafetaSettings _estafetaSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly IAddressService _addressService;

        private readonly ShoppingService _shoppingService;

        #endregion

        #region Ctor

        public ShippingEstafetaController(EstafetaSettings estafetaSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService, IOrderService orderService,
            IProductService productService, IStoreService storeService,
            ICustomerService customerService, IShippingService shippingService,
            IAddressService addressService, ShoppingService shoppingService,
            IShipmentService shipmentService)
        {
            this._estafetaSettings = estafetaSettings;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._productService = productService;
            this._storeService = storeService;
            this._customerService = customerService;
            this._shippingService = shippingService;
            this._addressService = addressService;
            this._shoppingService = shoppingService;
            this._shipmentService = shipmentService;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new EstafetaShippingModel()
            {
                CotizadorId = _estafetaSettings.CotizadorId,
                EnviosId = _estafetaSettings.EnviosId,
                GuiasId = _estafetaSettings.GuiasId,
                Login = _estafetaSettings.Login,
                OfficeNum = _estafetaSettings.OfficeNum,
                Password = _estafetaSettings.Password,
                ServiceTypeId = _estafetaSettings.ServiceTypeId,
                UrlCotizador = _estafetaSettings.UrlCotizador,
                UrlEnvios = _estafetaSettings.UrlEnvios,
                UrlGuias = _estafetaSettings.UrlGuias
            };

            return View("~/Plugins/Shipping.Estafeta/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(EstafetaShippingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _estafetaSettings.CotizadorId = model.CotizadorId;
            _estafetaSettings.EnviosId = model.EnviosId;
            _estafetaSettings.GuiasId = model.GuiasId;
            _estafetaSettings.Login = model.Login;
            _estafetaSettings.OfficeNum = model.OfficeNum;
            _estafetaSettings.Password = model.Password;
            _estafetaSettings.ServiceTypeId = model.ServiceTypeId;
            _estafetaSettings.UrlCotizador = model.UrlCotizador;
            _estafetaSettings.UrlEnvios = model.UrlEnvios;
            _estafetaSettings.UrlGuias = model.UrlGuias;

            _settingService.SaveSetting(_estafetaSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion

        #region Guias

        public IActionResult GenerarGuia(int shipmentId)
        {
            var order = _shipmentService.GetShipmentById(shipmentId);

            var model = _orderService.GetOrderById(order.OrderId);
            var s = _shoppingService.GetAll().Where(x => x.numOrder == model.CustomOrderNumber).FirstOrDefault();
            if (s != null)
            {
                //PDF exists in database
                byte[] bytes = null;
                bytes = s.guiaPdf;
                return File(bytes, "application/pdf");
            }

            List<int> warehouseIds = model.OrderItems.Select(x => x.Product.WarehouseId).Distinct().ToList();
            if (warehouseIds.Where(x => x == 0).Count() > 0)
            {
                return BadRequest("Uno de los productos no tiene almacén asignado.");
            }

            #region connection

            EstafetaLabelService.EstafetaLabelClient client = new EstafetaLabelService.EstafetaLabelClient(EstafetaLabelService.EstafetaLabelClient.EndpointConfiguration.EstafetaLabelWS);
            EstafetaLabelService.EstafetaLabelRequestExtended req = new EstafetaLabelService.EstafetaLabelRequestExtended();
            EstafetaLabelService.LabelDescriptionListExtended Descripcionlista = new EstafetaLabelService.LabelDescriptionListExtended();
            EstafetaLabelService.DestinationInfoExtended destino = new EstafetaLabelService.DestinationInfoExtended();
            EstafetaLabelService.OriginInfoExtended origen = new EstafetaLabelService.OriginInfoExtended();
            EstafetaLabelService.EstafetaLabelResponseExtended response = new EstafetaLabelService.EstafetaLabelResponseExtended();

            req.suscriberId = _estafetaSettings.GuiasId;
            req.login = _estafetaSettings.Login;
            req.password = _estafetaSettings.Password;
            req.customerNumber = _estafetaSettings.Login;
            req.paperType = 2;
            req.quadrant = 0;
            req.labelDescriptionListCount = model.OrderItems.Count;
            req.valid = true;

            #endregion

            foreach (var wareId in warehouseIds)
            {
                List<EstafetaLabelService.LabelDescriptionListExtended> list = new List<EstafetaLabelService.LabelDescriptionListExtended>();
                foreach (var item in model.OrderItems.Where(x => x.Product.WarehouseId == wareId))
                {
                    if (item.Product.Weight == 0 || item.Product.Width == 0 || item.Product.Length == 0 || item.Product.Height == 0)
                    {
                        //ErrorNotification("Se deben especificar las dimensiones de los productos para poder generar la guía");
                        return BadRequest("Se deben especificar las dimensiones de los productos para poder generar la guía");
                    }

                    list.Add(new EstafetaLabelService.LabelDescriptionListExtended
                    {
                        originInfo = origen,
                        destinationInfo = destino,
                        //costCenter = "CCtos",
                        deliveryToEstafetaOffice = false,
                        destinationCountryId = "MX",
                        //Tipo de envio 1=SOBRE 4=PAQUETE       
                        parcelTypeId = 4,
                        numberOfLabels = model.OrderItems.Count,
                        originZipCodeForRouting = model.ShippingAddress.ZipPostalCode,
                        serviceTypeId = _estafetaSettings.ServiceTypeId,
                        officeNum = _estafetaSettings.OfficeNum,
                        returnDocument = false,
                        serviceTypeIdDocRet = "50",
                        effectiveDate = DateTime.Now.AddDays(10).ToString("yyyyMMdd"),
                        aditionalInfo = FormatForShipping(model.ShippingAddress.Address1, 25),
                        valid = true,
                        content = item.Product.Sku.Length > 24 ? item.Product.Sku.Substring(0, 24) : item.Product.Sku,
                        weight = (float)_productService.GetProductById(item.ProductId).Weight < 1 ? 1 : (float)_productService.GetProductById(item.ProductId).Weight,
                        contentDescription = item.Product.Name.Length > 24 ? item.Product.Name.Substring(0, 24) : item.Product.Name
                        //_productService.GetProductById(item.ProductId).ShortDescription.Substring(0,24)
                    });
                }

                EstafetaLabelService.LabelDescriptionListExtended[] listArray = list.ToArray();

                //Informacion Destino        
                destino.address1 = FormatForShipping(model.ShippingAddress.Address1, 30);
                destino.address2 = FormatForShipping(model.ShippingAddress.Address2, 30);
                destino.city = FormatEmpty(model.ShippingAddress.City);
                destino.contactName = FormatEmpty(_customerService.GetCustomerById(model.CustomerId).GetFullName());
                destino.corporateName = FormatEmpty(_storeService.GetStoreById(model.StoreId).Name);
                destino.customerNumber = FormatEmpty(_estafetaSettings.Login);
                destino.neighborhood = FormatEmpty(model.ShippingAddress.StateProvince.Name);
                destino.phoneNumber = FormatEmpty(model.ShippingAddress.PhoneNumber);
                destino.cellPhone = FormatEmpty(model.ShippingAddress.PhoneNumber);
                destino.state = FormatEmpty(model.ShippingAddress.StateProvince.Name);
                //Código postal destino
                destino.zipCode = FormatEmpty(model.ShippingAddress.ZipPostalCode);

                var warehouse = _shippingService.GetWarehouseById(wareId);
                var warehouseAddress = _addressService.GetAddressById(warehouse.AddressId);
                //Informacion Origen       
                origen.address1 = FormatForShipping(warehouseAddress.Address1, 30);
                origen.address2 = FormatForShipping(warehouseAddress.Address2, 30);
                origen.city = FormatEmpty(warehouseAddress.City);
                origen.contactName = FormatEmpty(warehouse.Name);
                origen.corporateName = FormatEmpty(warehouse.Name);
                origen.customerNumber = FormatEmpty(_estafetaSettings.Login);
                origen.neighborhood = FormatEmpty(warehouseAddress.StateProvince.Name);
                origen.phoneNumber = FormatEmpty(warehouseAddress.PhoneNumber);
                origen.cellPhone = FormatEmpty(warehouseAddress.PhoneNumber);
                origen.state = FormatEmpty(warehouseAddress.StateProvince.Name);
                //Código postal origen. Solo se usa para impresión       
                origen.zipCode = FormatEmpty(warehouseAddress.ZipPostalCode);
                req.labelDescriptionList = listArray;

                try
                {
                    response = client.createLabelExtendedAsync(req).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return BadRequest("Error al conectarse con Estafeta.");
                }
            }

            if (response.globalResult != null && response.globalResult.resultCode == 0 && response.labelPDF != null)
            {
                byte[] bytes = null;

                bytes = response.labelPDF;

                var record = new Shopping();

                record.Name = _customerService.GetCustomerById(model.CustomerId).GetFullName();
                record.TrackingCode = response.globalResult.resultDescription;
                record.GuideNumber = response.globalResult.resultDescription;
                record.guiaPdf = bytes;
                record.numOrder = model.CustomOrderNumber;
                _shoppingService.Insert(record);

                var shipment = _shipmentService.GetShipmentById(shipmentId);
                if (shipment == null)
                    return BadRequest("No se pudo actualizar el número de guía.");

                shipment.TrackingNumber = response.globalResult.resultDescription;
                _shipmentService.UpdateShipment(shipment);

                return File(bytes, "application/pdf");
            }
            else
            {
                //Hubo un error y debe ser mostrado
                Console.WriteLine("Result with ERROR");
                Console.WriteLine(response.globalResult.resultDescription);
                //Console.WriteLine(response.labelResultList[0].resultDescription);
                return BadRequest(response.globalResult.resultSpanishDescription);
            }
        }

        public string FormatForShipping(string main, int until)
        {
            if (!string.IsNullOrEmpty(main))
            {
                if (main.Length > until)
                    main = main.Substring(0, until);
            }
            main = FormatEmpty(main);
            return (main);
        }

        public string FormatEmpty(string main)
        {
            if (string.IsNullOrEmpty(main) || string.IsNullOrWhiteSpace(main))
            {
                main = "Sin especificar";
            }
            return (main);
        }
    }
    #endregion
}