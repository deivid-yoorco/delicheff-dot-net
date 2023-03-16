using Nop.Web.Framework.Mvc.Models;
using Nop.Core;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Teed.Plugin.CategorySearch.Models
{
    public class CategorySearchModel : BaseNopModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
        public string PictureUrl { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
