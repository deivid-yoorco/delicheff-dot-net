using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.SimplePedido
{
    public class SimplePedidoBaseModel
    {
        public int CustomerId { get; set; }
        public DateTime? SelectedShippingDate { get; set; }
        public string SelectedShippingTime { get; set; }
        public string ShippingAddress { get; set; }
    }

    public class GenerateExcel46Model : SimplePedidoBaseModel
    {
        public decimal OrderTotal { get; set; }
    }

    public class GenerateExcel14Model : SimplePedidoBaseModel
    {
        public int Id { get; set; }
        public decimal OrderTotal { get; set; }
    }

    public class GenerateExcel125Model : SimplePedidoBaseModel
    {
        public List<GenerateExcel125OrderitemModel> OrderItems { get; set; }
        public int Id { get; set; }
    }

    public class GenerateExcel125OrderitemModel
    {
        public int Id { get; set; }
        public decimal PriceInclTax { get; set; }
    }

    public class GenerateExcel128Model : SimplePedidoBaseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal CustomerBalanceUsedAmount { get; set; }
        public string Email { get; set; }
    }
}
