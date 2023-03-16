using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Teed.Plugin.CustomerComments.Models
{
    public class CustomerCommentModel : BaseNopModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public int CreatedByCustomerId { get; set; }
        public string Log { get; set; }
    }
}
