using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tasks;
using System.Linq;

namespace Nop.Plugin.Shipping.Estafeta.Controllers
{
    public class ScheduleTaskEstafeta : IScheduleTask
    {
        private readonly IShipmentService _shipmentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly EstafetaSettings _estafetaSettings;

        public ScheduleTaskEstafeta(IShipmentService shipmentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService, ICustomerActivityService customerActivityService,
            ILocalizationService localizationService, EstafetaSettings estafetaSettings)
        {
            this._shipmentService = shipmentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._estafetaSettings = estafetaSettings;
        }

        protected virtual void LogEditOrder(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), order.CustomOrderNumber);
        }

        public void Execute()
        {
            var shipment = _shipmentService.GetAllShipments().Where(x => x.Order.ShippingStatusId != 40).ToArray();
            string[] guiasShipment = shipment.Select(x => x.TrackingNumber).ToArray();

            EstafetaConsultaEnvios.ArrayOfString guias = new EstafetaConsultaEnvios.ArrayOfString();

            foreach (var item in guiasShipment)
            {
                guias.Add(item);
            }

            #region ConsultaEnvios

            EstafetaConsultaEnvios.ServiceSoapClient client = new EstafetaConsultaEnvios.ServiceSoapClient(EstafetaConsultaEnvios.ServiceSoapClient.EndpointConfiguration.ServiceSoap12);

            EstafetaConsultaEnvios.SearchType searchType = new EstafetaConsultaEnvios.SearchType();
            EstafetaConsultaEnvios.SearchConfiguration searchConfiguration = new EstafetaConsultaEnvios.SearchConfiguration();
            //EstafetaConsultaEnvios.WaybillRange waybillRange = new EstafetaConsultaEnvios.WaybillRange();
            EstafetaConsultaEnvios.WaybillList waybillList = new EstafetaConsultaEnvios.WaybillList();
            EstafetaConsultaEnvios.HistoryConfiguration historyConfiguration = new EstafetaConsultaEnvios.HistoryConfiguration();
            EstafetaConsultaEnvios.Filter filter = new EstafetaConsultaEnvios.Filter();

            searchType.type = "L";
            searchType.waybillList = waybillList;
            //searchType.waybillRange = waybillRange;

            searchConfiguration.includeDimensions = false;
            searchConfiguration.includeWaybillReplaceData = false;
            searchConfiguration.includeReturnDocumentData = false;
            searchConfiguration.includeMultipleServiceData = false;
            searchConfiguration.includeInternationalData = false;
            searchConfiguration.includeSignature = true;
            searchConfiguration.includeCustomerInfo = true;
            searchConfiguration.historyConfiguration = historyConfiguration;
            searchConfiguration.filterType = filter;

            //waybillRange.initialWaybill = "";
            //waybillRange.finalWaybill = "";

            waybillList.waybillType = "G";
            waybillList.waybills = guias;

            historyConfiguration.includeHistory = true;
            historyConfiguration.historyType = "ALL";

            filter.filterInformation = false;
            //filter.filterType = "ON_TRANSIT";

            var response = client.ExecuteQueryAsync(_estafetaSettings.EnviosId, _estafetaSettings.Login, _estafetaSettings.Password, searchType, searchConfiguration).Result;

            var tracking = response.Body.ExecuteQueryResult.trackingData;

            if (tracking != null)
            {
                foreach (var item in tracking)
                {
                    for (int i = 0; i < shipment.Length; i++)
                    {
                        if (item.waybill == shipment[i].TrackingNumber)
                        {
                            if (item.statusENG == "DELIVERED")
                            {
                                _orderProcessingService.Deliver(shipment[i], true);
                                LogEditOrder(shipment[i].OrderId);
                                break;
                            }
                            else if (item.statusENG == "ON_TRANSIT")
                            {
                                _orderProcessingService.Ship(shipment[i], true);
                                LogEditOrder(shipment[i].OrderId);
                                break;
                            }
                        }
                    }
                }
            }

            #endregion
        }
    }
}
