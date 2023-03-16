namespace Teed.Plugin.Manager.Models.PurchaseOrder
{
    class PurchaseOrderListModel
    {
        public int Id { get; set; }
        public string RequestedDate { get; set; }
        public string RequestedBy { get; set; }
        public string PurchaseOrderStatus { get; set; }
        public string Comments { get; set; }
    }
}