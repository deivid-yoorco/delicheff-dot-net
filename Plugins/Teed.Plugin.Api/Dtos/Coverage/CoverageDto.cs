using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Coverage
{
    public class CoverageDto
    {
        public string State { get; set; }
        public string City { get; set; }
        public string Suburbs { get; set; }
        public string PostalCode { get; set; }
    }
}
