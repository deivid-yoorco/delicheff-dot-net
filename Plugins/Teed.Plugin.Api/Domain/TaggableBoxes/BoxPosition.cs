using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Domain.TaggableBoxes
{
    public enum BoxPosition
    {
        [Display(Name = "Superior")]
        Upper = 0,

        [Display(Name = "Inferior")]
        Lower = 1,
    }
}
