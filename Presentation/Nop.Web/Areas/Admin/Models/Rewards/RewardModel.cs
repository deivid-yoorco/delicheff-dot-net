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
    public class RewardModel
    {
        #region General

        [NopResourceDisplayName("Activar o desactivar las recompensas en general")]
        public bool IsActive { get; set; }

        #endregion

        #region Rewards Page

        [NopResourceDisplayName("Título")]
        public string Title { get; set; }

        [NopResourceDisplayName("Descripción")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Imagen")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Activo")]
        public bool Active { get; set; }

        #endregion

        #region Points

        [NopResourceDisplayName("Puntos por peso que se otorgan al completar una orden")]
        public decimal OrderPoints { get; set; }

        [NopResourceDisplayName("Puntos que se otorgan al usuario si inicia sesión por primera vez con Facebook")]
        public decimal FacebookPoints { get; set; }

        #endregion

        #region Levels

        [NopResourceDisplayName("Cantidad de meses para calcular nivel")]
        public int LevelMonthsCount { get; set; }

        #endregion
    }
}