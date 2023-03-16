using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Models
{
    public class PopupModel : BaseNopModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [UIHint("Picture")]
        public int? ImageId { get; set; }

        [UIHint("Picture")]
        public int? ImageForDesktopId { get; set; }

        public bool Mondays { get; set; }

        public bool Tuesdays { get; set; }

        public bool Wednesdays { get; set; }

        public bool Thursdays { get; set; }

        public bool Fridays { get; set; }

        public bool Saturdays { get; set; }

        public bool Sundays { get; set; }

        public bool FirstTimeOnly { get; set; }

        public bool Active { get; set; }

        public string ViewableDeadlineDateString { get; set; }

        public string Log { get; set; }
    }
}
