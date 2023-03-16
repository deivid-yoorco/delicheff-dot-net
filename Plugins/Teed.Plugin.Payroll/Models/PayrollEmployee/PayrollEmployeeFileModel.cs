using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;

namespace Teed.Plugin.Payroll.Models.PayrollEmployee
{
    public class PayrollEmployeeFileModel
    {
        public virtual int Id { get; set; }
        public virtual int Size { get; set; }
        public virtual string Extension { get; set; }
        public virtual string FileMimeType { get; set; }
        public virtual FileType FileType { get; set; }
        public virtual IFormFile File { get; set; }
        public virtual byte[] FileArray { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual int PayrollEmployeeId { get; set; }
    }
}
