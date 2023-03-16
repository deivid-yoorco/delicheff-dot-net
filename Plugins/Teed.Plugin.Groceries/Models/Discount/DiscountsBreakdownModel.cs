using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Orders;

namespace Teed.Plugin.Groceries.Models.Discount
{
    public class DiscountsBreakdownModel
    {
        public virtual string DateString { get; set; }
        public virtual List<Discount> Discounts { get; set; }
    }

    public class Discount
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int TimesUsed { get; set; }
        public virtual List<Order> Orders { get; set; }
    }

    public class Order
    {
        public virtual int Id { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual string Customer { get; set; }
    }
}