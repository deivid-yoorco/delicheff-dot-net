using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.TaggableBoxes
{
    public enum ElementType
    {
        [Display(Name = "Tag")]
        Tag = 0,

        [Display(Name = "Producto")]
        Product = 1,

        [Display(Name = "Categoría")]
        Category = 2,
    }
}
