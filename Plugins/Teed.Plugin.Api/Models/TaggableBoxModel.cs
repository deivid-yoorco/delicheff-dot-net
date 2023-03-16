using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class TaggableBoxModel : BaseNopModel
    {
        public TaggableBoxModel()
        {
            Positions = new List<SelectListItem>();

            Types = new List<SelectListItem>();

            Elements = new List<SelectListItem>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Position { get; set; }

        public int Type { get; set; }

        public int ElementId { get; set; }

        [UIHint("Picture")]
        public int PictureId { get; set; }

        public List<int> SelectElementId { get; set; }

        public List<SelectListItem> Positions { get; set; }

        public List<SelectListItem> Types { get; set; }

        public List<SelectListItem> Elements { get; set; }

        public string Log { get; set; }
    }
}
