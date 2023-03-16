using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Facturify.Domain;

namespace Teed.Plugin.Facturify
{
    public class FacturifySettings : ISettings
    {
        public string BusinessName { get; set; }

        public string RFC { get; set; }
        
        public FiscalRegime Regime { get; set; }
        
        public string Email { get; set; }
        
        public string AddressPostalCode { get; set; }
        
        public string AddressStreet { get; set; }
        
        public string AddressExteriorNumber { get; set; }
        
        public string AddressInternalNumber { get; set; }
        
        public string AddressSuburb { get; set; }
        
        public string AddressMunicipality { get; set; }
        
        public string AddressCity { get; set; }
        
        public string AddressState { get; set; }
        
        public string CsdKeyFileBase64 { get; set; }
        
        public string CsdCerFileBase64 { get; set; }
        
        public string CsdKeyFilePassword { get; set; }
        
        public string FielKeyFileBase64 { get; set; }
        
        public string FielCerFileBase64 { get; set; }
        
        public string FielKeyFilePassword { get; set; }

        //public string IncomeSerie { get; set; }

        //public string IncomeInitialFolio { get; set; }

        //public string ExpenseSerie { get; set; }

        //public string ExpenseInitialFolio { get; set; }

        public string FacturifyUuid { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public string Token { get; set; }

        public bool Sandbox { get; set; }

        public int DaysAllowed { get; set; }

        public bool AllowBillGeneration { get; set; }
    }
}
