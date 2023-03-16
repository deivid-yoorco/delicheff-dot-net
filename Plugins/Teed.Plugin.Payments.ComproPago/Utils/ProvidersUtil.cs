using CompropagoSdk.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.ComproPago.Utils
{
    public static class ProvidersUtil
    {
        public static object[] GetAllProviders()
        {
            return Factory.ListProviders();
        }
    }
}
