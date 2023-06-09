﻿using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Catalog
{
    public partial class SearchBoxModel : BaseNopModel
    {
        public bool AutoCompleteEnabled { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public int SearchTermMinimumLength { get; set; }

        public bool ShowAsNavBar { get; set; }
        public bool ShowAsDiv { get; set; }
    }
}