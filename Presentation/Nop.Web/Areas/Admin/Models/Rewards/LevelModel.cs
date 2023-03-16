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
    public class LevelModel
    //, ILocalizedModel<CategoryLocalizedModel>
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Nombre")]
        public string Name { get; set; }

        [NopResourceDisplayName("Descripción")]
        public string Description { get; set; }

        [NopResourceDisplayName("Monto necesario")]
        public decimal RequiredAmount { get; set; }

        //customer roles
        [NopResourceDisplayName("Rol a asignar")]
        public int CustomerRoleId { get; set; }
        public IList<SelectListItem> CustomerRoles { get; set; }

        [NopResourceDisplayName("Bitácora")]
        public string Log { get; set; }
    }
}