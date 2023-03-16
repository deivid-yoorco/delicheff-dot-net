using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Services.Helpers
{
    /// <summary>
    /// Product helper interface
    /// </summary>
    public partial interface IProductHelper
    {
        bool AnyProductHasSpecificationOptionId(List<Product> products, int specificationOptionId);
    }
}