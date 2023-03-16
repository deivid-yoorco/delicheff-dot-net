using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Validators.Catalog;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Areas.Admin.Models.Rewards
{
    //[Validator(typeof(CategoryValidator))]
    public class BadgeModel
        //, ILocalizedModel<CategoryLocalizedModel>
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Nombre")]
        public string Name { get; set; }

        [NopResourceDisplayName("Descripción")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Imagen bronce")]
        public int BronzeImageId { get; set; }

        [NopResourceDisplayName("Monto bronce")]
        public decimal BronzeAmount { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Imagen plata")]
        public int SilverImageId { get; set; }

        [NopResourceDisplayName("Monto plata")]
        public decimal SilverAmount { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Imagen oro")]
        public int GoldImageId { get; set; }

        [NopResourceDisplayName("Monto oro")]
        public decimal GoldAmount { get; set; }

        [NopResourceDisplayName("Tipo de elemento para obtener la recompensa")]
        public int ElementTypeId { get; set; }
        public IList<SelectListItem> ElementType { get; set; }

        //categories
        [NopResourceDisplayName("Subcategorías")]
        public IList<int> SelectedSubcategoryIds { get; set; }
        public IList<SelectListItem> AvailableSubcategories { get; set; }

        //products
        [NopResourceDisplayName("Productos")]
        public IList<int> SelectedProductIds { get; set; }
        public IList<SelectListItem> AvailableProducts { get; set; }

        //tags
        [NopResourceDisplayName("Tags")]
        public IList<int> SelectedTagIds { get; set; }
        public IList<SelectListItem> AvailableTags { get; set; }

        [NopResourceDisplayName("Bitácora")]
        public string Log { get; set; }
    }
}