using System.Collections.Generic;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public class CategorySimpleModel : BaseNopEntityModel
    {
        public CategorySimpleModel()
        {
            SubCategories = new List<CategorySimpleModel>();
            PictureModel = new PictureModel();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public PictureModel PictureModel { get; set; }

        public List<CategorySimpleModel> SubCategories { get; set; }
    }
}