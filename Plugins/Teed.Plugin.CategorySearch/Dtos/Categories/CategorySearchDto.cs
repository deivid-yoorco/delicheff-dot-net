using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CategorySearch.Dtos.Categories
{
    public class CategorySearchDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
