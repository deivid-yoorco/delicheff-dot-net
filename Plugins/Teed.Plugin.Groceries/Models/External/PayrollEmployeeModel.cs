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

namespace Teed.Plugin.Groceries.Models.External
{
    public class PayrollEmployeeModel
    {
        public virtual int Id { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual string FullName { get; set; }
        public virtual string Rfc { get; set; }
        public virtual string Curp { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Job { get; set; }
    }
}