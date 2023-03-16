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
    public class OnboardingModel : BaseNopModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [UIHint("Picture")]
        public int? ImageId { get; set; }

        public string BackgroundColor { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public bool Active { get; set; }

        public string Log { get; set; }
    }
}
