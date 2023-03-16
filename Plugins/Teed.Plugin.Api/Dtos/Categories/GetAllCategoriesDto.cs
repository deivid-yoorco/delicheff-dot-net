using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Categories
{
    public class GetAllCategoriesDto
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string ImageUrl { get; set; }

        public bool IsParent { get; set; }

        public int ProductsCount { get; set; }
    }
}
