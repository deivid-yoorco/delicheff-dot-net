using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.Notifications
{
    public enum ActionType
    {
        // For banners only
        [Display(Name = "Ninguna")]
        None = 0,

        // For notifications only
        [Display(Name = "Abrir la app")]
        OpenApp = 10,

        [Display(Name = "Abrir carrito")]
        OpenShoppingCart = 20,

        [Display(Name = "Abrir wishlist")]
        OpenWishlist = 30,

        [Display(Name = "Abrir registro")]
        OpenRegister = 40,

        [Display(Name = "Abrir categoría")]
        OpenCategory = 50,

        [Display(Name = "Abrir producto")]
        OpenProduct = 60,

        [Display(Name = "Buscar tag o palabra clave")]
        SearchTagOrKeyword = 70,

        [Display(Name = "Abrir link externo")]
        OpenExternalLink = 80,
    }
}
