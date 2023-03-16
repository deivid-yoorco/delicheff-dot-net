using System.ComponentModel.DataAnnotations;

namespace Nop.Core.Domain.Catalog
{
    public enum TypeChangeEnum
    {
        [Display(Name = "Cancelación")]
        Cancel = 1,

        [Display(Name = "Compra")]
        Buy = 2,

        [Display(Name = "Devolución")]
        Back = 3,

        [Display(Name = "Excel")]
        Excel = 4,

        [Display(Name = "Manual")]
        Edit = 5,
    }
}