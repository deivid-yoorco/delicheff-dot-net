using System.ComponentModel.DataAnnotations;

namespace Nop.Core.Domain.Rewards
{
    /// <summary>
    /// Represents a type of element used in badges enumeration
    /// </summary>
    public enum ElementType
    {
        [Display(Name = "Subcategoría")]
        Subcategory = 1,

        [Display(Name = "Tag")]
        Tag = 2,

        [Display(Name = "Producto")]
        Product = 3
    }
}
