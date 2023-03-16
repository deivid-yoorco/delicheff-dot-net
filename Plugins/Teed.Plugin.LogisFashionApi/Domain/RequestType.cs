using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Logisfashion.Domain
{
    public enum RequestType
    {
        MasterData = 1,
        Inbound = 2,
        Return = 3,
        Outbound = 4,
        Check = 5,
        Incoming = 6,
        CheckSku = 7,
        UpdateDimensions = 8,
    }
}
