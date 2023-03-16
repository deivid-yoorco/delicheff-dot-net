using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Home
{
    public class GetSliderPicturesDto
    {
        public string SliderPictureUrl { get; set; }
    }

    public class GetSliderPicturesFullDto
    {
        public string SliderPictureUrl { get; set; }
        public int ActionTypeId { get; set; }
        public object AdditionalData { get; set; }
    }
}
