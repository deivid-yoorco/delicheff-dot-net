using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion
{
    public class LogisfashionSettings : ISettings
    {
        public bool Sandbox { get; set; }
        public int ClientCode { get; set; }
        public string ApiKey { get; set; }
    }
}
