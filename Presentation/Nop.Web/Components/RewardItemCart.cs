using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Services.Catalog;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Web.Components
{
    public class RewardItemCart : NopViewComponent
    {
        private readonly IRewardItemService _rewardItemService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly RewardItemSettings _rewardItemSettings;

        public RewardItemCart(IRewardItemService rewardItemService, IProductService productService, IWorkContext workContext, RewardItemSettings rewardItemSettings)
        {
            this._rewardItemService = rewardItemService;
            this._productService = productService;
            this._workContext = workContext;
            this._rewardItemSettings = rewardItemSettings;
        }

        public IViewComponentResult Invoke()
        {
                var shoppingProductsIds = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ShoppingCartType == Core.Domain.Orders.ShoppingCartType.ShoppingCart).Select(x => x.ProductId);
                var rewardsItems = _rewardItemService.GetAllRewardItemQuery().Where(x => !shoppingProductsIds.Contains(x.ProductId) && x.IsActive).ToList();
                var productIds = rewardsItems.Select(z => z.ProductId).Distinct().ToList();
                var products = _productService.GetAllProductsQuery().Where(x => productIds.Contains(x.Id) && x.Published).ToList();
                return View(products);
        }
    }
}
