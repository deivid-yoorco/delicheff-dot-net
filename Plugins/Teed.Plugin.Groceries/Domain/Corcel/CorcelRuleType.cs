using Nop.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Groceries.Domain.Corcel
{
    public enum CorcelRuleType
    {
        [Display(Name = "Regla 1",
            Description = "Que compre productos de repostería y panadería de más de 1 kg (se toma de equivalencia manual).")]
        Rule1 = 1,

        [Display(Name = "Regla 2",
            Description = "Que en un mismo pedido compre 3 o más piezas de un mismo producto de Repostería y panadería, al menos 1 vez al mes.")]
        Rule2 = 2,

        [Display(Name = "Regla 3",
            Description = "Que en un mismo pedido compre 5 o más productos diferentes de la categoría de Repostería y panadería, al menos 1 vez al mes.")]
        Rule3 = 3,

        [Display(Name = "Regla 4",
            Description = "Que la dirección contenga la palabra \"local\", \"panadería\", \"repostería\", \"cafetería\".")]
        Rule4 = 4,
    }
}
