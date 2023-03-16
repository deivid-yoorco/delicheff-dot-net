using Nop.Core.Configuration;
using System;

namespace Teed.Plugin.Careers
{
    public class CareersSettings : ISettings
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public bool ShowInFooter1 { get; set; }
        public bool ShowInFooter2 { get; set; }
        public bool ShowInFooter3 { get; set; }
        public bool ShowInMobileMenu { get; set; }
        public bool ShowInHeader { get; set; }
        public bool Published { get; set; }
    }
}
