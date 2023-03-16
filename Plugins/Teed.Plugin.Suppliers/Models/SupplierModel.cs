﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Suppliers.Models
{
    public class SupplierModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public int StateProvinceId { get; set; }
        public int CountryId { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Comment { get; set; }
        public SelectList States { get; set; }
    }
}
