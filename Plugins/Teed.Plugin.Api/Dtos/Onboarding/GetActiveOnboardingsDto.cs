using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Onboarding
{
    public class GetActiveOnboardingsDto
    {
        public string BackgroundColor { get; set; }
        public string Subtitle { get; set; }
        public string Title { get; set; }
        public int ImageId { get; set; }
    }
}
