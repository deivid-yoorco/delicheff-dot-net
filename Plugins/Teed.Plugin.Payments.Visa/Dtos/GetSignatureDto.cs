using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Visa.Dtos
{
    public class GetSignatureDto
    {
        public string Digest { get; set; }

        public string Method { get; set; }

        public string Resource { get; set; }    

        public DateTime GmtDateTime { get; set; }
    }
}