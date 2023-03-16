using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.SaleRecord.Domain.SaleRecords
{
    /// <summary>
    /// Represents credit cards type enumeration
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// Visa
        /// </summary>
        [Display(Name = "Visa" )]
        Visa = 10,
        /// <summary>
        /// MasterCard
        /// </summary>
        [Display(Name = "Master Card" )]
        MasterCard = 20,
        /// <summary>
        /// American
        /// </summary>
        [Display(Name = "American Express" )]
        AmericanExpress = 30,
        /// <summary>
        /// Otro
        /// </summary>
        [Display(Name = "Otro" )]
        Otro = 40
    }
}
