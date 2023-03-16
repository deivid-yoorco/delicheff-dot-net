using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.CornerForm.Domain;

namespace Teed.Plugin.CornerForm
{
    public class CornerFormSettings : ISettings
    {
        public string MinimizedText { get; set; }

        public string Question { get; set; }

        public string ButtonColor { get; set; }

        public string ButtonTextColor { get; set; }
    }
}
