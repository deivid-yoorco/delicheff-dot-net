using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Models.Discounts;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.ExportImport;
using System.Text.RegularExpressions;
using Nop.Services.Media;
using Nop.Services.Customers;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class DiscountController : BaseAdminController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly CurrencySettings _currencySettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IExportManager _exportManager;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;
        private readonly ISpecialDiscountTakeXPayYService _specialDiscountTakeXPayYService;

        private static Random random = new Random();

        #endregion

        #region Ctor

        public DiscountController(IDiscountService discountService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            ICategoryService categoryService,
            IProductService productService,
            IWebHelper webHelper,
            IDateTimeHelper dateTimeHelper,
            ICustomerActivityService customerActivityService,
            CurrencySettings currencySettings,
            CatalogSettings catalogSettings,
            IPermissionService permissionService,
            IWorkContext workContext,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IStaticCacheManager cacheManager,
            IExportManager exportManager,
            IPictureService pictureService,
            ICustomerService customerService,
            ISpecialDiscountTakeXPayYService specialDiscountTakeXPayYService)
        {
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._categoryService = categoryService;
            this._productService = productService;
            this._webHelper = webHelper;
            this._dateTimeHelper = dateTimeHelper;
            this._customerActivityService = customerActivityService;
            this._currencySettings = currencySettings;
            this._catalogSettings = catalogSettings;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._orderService = orderService;
            this._priceFormatter = priceFormatter;
            this._cacheManager = cacheManager;
            this._exportManager = exportManager;
            this._pictureService = pictureService;
            this._customerService = customerService;
            this._specialDiscountTakeXPayYService = specialDiscountTakeXPayYService;
        }

        #endregion

        #region Utilities

        protected virtual string GetRequirementUrlInternal(IDiscountRequirementRule discountRequirementRule, Discount discount, int? discountRequirementId)
        {
            if (discountRequirementRule == null)
                throw new ArgumentNullException(nameof(discountRequirementRule));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            var url = $"{_webHelper.GetStoreLocation()}{discountRequirementRule.GetConfigurationUrl(discount.Id, discountRequirementId)}";
            return url;
        }

        protected virtual void PrepareDiscountModel(DiscountModel model, Discount discount)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (discount == null)
                return;

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.AvailableDiscountRequirementRules.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.Select"), Value = "" });

            //add item "Add group" in the list
            model.AvailableDiscountRequirementRules.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.AddGroup"), Value = "AddGroup" });

            var discountRules = _discountService.LoadAllDiscountRequirementRules();
            foreach (var discountRule in discountRules)
                model.AvailableDiscountRequirementRules.Add(new SelectListItem { Text = discountRule.PluginDescriptor.FriendlyName, Value = discountRule.PluginDescriptor.SystemName });

            //get available requirement groups
            var requirementGroups = discount.DiscountRequirements.Where(requirement => requirement.IsGroup);
            model.AvailableRequirementGroups = requirementGroups.Select(requirement =>
                new SelectListItem { Value = requirement.Id.ToString(), Text = requirement.DiscountRequirementRuleSystemName }).ToList();

            //customers for limited to emails
            if (!string.IsNullOrEmpty(discount.LimitedToCustomerEmail))
            {
                var emails = discount.LimitedToCustomerEmail.Split(',').ToList();
                var customerIds = _customerService.GetAllCustomersQuery()
                    .Where(x => !x.Deleted && x.Active && emails.Contains(x.Email))
                    .Select(x => x.Id).ToList();
                model.LimitedToCustomerIds = customerIds;
            }
            //discounts for exclusive usage
            /*
            if (!string.IsNullOrEmpty(discount.ExclusiveUsageAgainstIds))
            {
                var discounts = discount.ExclusiveUsageAgainstIds.Split(',').Select(x => int.Parse(x)).ToList();
                if (discounts.FirstOrDefault() == 0)
                    model.ExclusiveUsageIds = new List<int> { 0 };
                else
                {
                    var discountIds = _discountService.GetAllDiscounts(showHidden: true)
                        .Where(x => x.RequiresCouponCode && !string.IsNullOrEmpty(x.CouponCode) && discounts.Contains(x.Id))
                        .Select(x => x.Id).ToList();
                    model.ExclusiveUsageIds = discountIds;
                }
            }
            */
            //special promotions
            var specialPromotion = _specialDiscountTakeXPayYService.GetSpecialDiscountByDiscountId(discount.Id);
            if (specialPromotion != null)
            {
                model.IsAcitve = specialPromotion.IsAcitve;
                model.EntityId = specialPromotion.EntityId;
                model.EntityTypeId = specialPromotion.EntityTypeId;
                model.TakeAmount = specialPromotion.TakeAmount;
                model.PayAmount = specialPromotion.PayAmount;
            }

        }

        protected void SpecialPromotionsCheck(int discountId, DiscountModel model)
        {
            // Take X, pay Y
            var shouldUpdate = false;
            var specialPromotion = _specialDiscountTakeXPayYService.GetSpecialDiscountByDiscountId(discountId);
            if (model.IsAcitve)
            {
                if (specialPromotion == null)
                {
                    specialPromotion = new SpecialDiscountTakeXPayY
                    {
                        DiscountId = discountId,
                        EntityId = model.EntityId,
                        EntityTypeId = model.EntityTypeId,
                        IsAcitve = model.IsAcitve,
                        TakeAmount = model.TakeAmount,
                        PayAmount = model.PayAmount,
                        Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) agregó esta promoción especial para el descuento con id {discountId}.\n"
                    };
                    _specialDiscountTakeXPayYService.InsertDiscount(specialPromotion);
                }
                else
                    shouldUpdate = true;
            }
            else if (specialPromotion != null)
                shouldUpdate = true;

            if (shouldUpdate)
            {
                if (specialPromotion.IsAcitve != model.IsAcitve)
                {
                    specialPromotion.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) " +
                        $"cambió si la promoción esta ativa de \"{(specialPromotion.IsAcitve ? "Si" : "No")}\" a \"{(model.IsAcitve ? "Si" : "No")}\".\n";
                    specialPromotion.IsAcitve = model.IsAcitve;
                }
                if (specialPromotion.EntityId != model.EntityId)
                {
                    specialPromotion.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) " +
                        $"cambió el elemento de \"{specialPromotion.EntityId}\" a \"{model.EntityId}\".\n";
                    specialPromotion.EntityId = model.EntityId;
                }
                if (specialPromotion.EntityTypeId != model.EntityTypeId)
                {
                    specialPromotion.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) " +
                        $"cambió el tipo de elemento de \"{specialPromotion.EntityTypeId}\" a \"{model.EntityTypeId}\".\n";
                    specialPromotion.EntityTypeId = model.EntityTypeId;
                }
                if (specialPromotion.TakeAmount != model.TakeAmount)
                {
                    specialPromotion.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) " +
                        $"cambió la cantidad a dar de {specialPromotion.TakeAmount} a {model.TakeAmount}.\n";
                    specialPromotion.TakeAmount = model.TakeAmount;
                }
                if (specialPromotion.PayAmount != model.PayAmount)
                {
                    specialPromotion.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) " +
                        $"cambió la cantidad a dar de {specialPromotion.PayAmount} a {model.PayAmount}.\n";
                    specialPromotion.PayAmount = model.PayAmount;
                }
                _specialDiscountTakeXPayYService.UpdateDiscount(specialPromotion);
            }
        }

        protected IList<DiscountModel.DiscountRequirementMetaInfo> GetReqirements(IEnumerable<DiscountRequirement> requirements,
            RequirementGroupInteractionType groupInteractionType, Discount discount)
        {
            var lastRequirement = requirements.LastOrDefault();

            return requirements.Select(requirement =>
            {
                //set common properties
                var requirementModel = new DiscountModel.DiscountRequirementMetaInfo
                {
                    DiscountRequirementId = requirement.Id,
                    ParentId = requirement.ParentId,
                    IsGroup = requirement.IsGroup,
                    RuleName = requirement.DiscountRequirementRuleSystemName,
                    IsLastInGroup = lastRequirement == null || lastRequirement.Id == requirement.Id,
                    InteractionTypeId = (int)groupInteractionType,
                };

                var interactionType = requirement.InteractionType.HasValue
                    ? requirement.InteractionType.Value : RequirementGroupInteractionType.And;
                requirementModel.AvailableInteractionTypes = interactionType.ToSelectList(true);

                if (requirement.IsGroup)
                {
                    //get child requirements for the group
                    requirementModel.ChildRequirements = GetReqirements(requirement.ChildRequirements, interactionType, discount);
                    return requirementModel;
                }

                //or try to get name and configuration URL for the requirement
                var requirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(requirement.DiscountRequirementRuleSystemName);
                if (requirementRule == null)
                    return null;

                requirementModel.RuleName = requirementRule.PluginDescriptor.FriendlyName;
                requirementModel.ConfigurationUrl = GetRequirementUrlInternal(requirementRule, discount, requirement.Id);

                return requirementModel;
            }).ToList();
        }

        protected void DeleteRequirement(ICollection<DiscountRequirement> requirements)
        {
            //recursively delete child requirements
            var tmpRequirements = requirements.ToList();
            for (var i = 0; i < tmpRequirements.Count; i++)
            {
                if (tmpRequirements[i].ChildRequirements.Any())
                    DeleteRequirement(tmpRequirements[i].ChildRequirements);
                _discountService.DeleteDiscountRequirement(tmpRequirements[i]);
            }
        }

        [HttpPost]
        public IActionResult GetCustomersFiltering(FilteringModel model)
        {
            if (!string.IsNullOrEmpty(model.Text) || model.ByIds.Any() || model.Texts.Any())
            {
                var customer = _customerService.GetAllCustomersQuery()
                        .Where(x => !x.Deleted && x.Active && !string.IsNullOrEmpty(x.Email));
                if (model.ByIds.Any())
                {
                    var byCustomerIds = model.ByIds.Distinct().ToList();
                    var filtered = customer
                        .Where(x => byCustomerIds.Contains(x.Id))
                        .ToList()
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.Email}",
                        })
                        .ToList();
                    return Json(filtered);
                }
                if (!string.IsNullOrEmpty(model.Text))
                {
                    var text = model.Text.Trim().ToLower();
                    var filtered = customer
                        .Where(x => x.Email.ToLower().Contains(text))
                        .Select(x => new { Name = x.Email, x.Id }).ToList();
                    var customersFilter = filtered.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name
                    }).ToList();

                    return Json(customersFilter);
                }
                else if (model.Texts.Any())
                {
                    var emails = model.Texts.Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => x.Trim())
                        .Distinct().ToList();
                    var filtered = customer
                        .Where(x => emails.Contains(x.Email))
                        .Select(x => new { Name = x.Email, x.Id }).ToList();
                    var foundEmails = filtered.Select(x => x.Name).ToList();
                    var notFoundEmails = emails.Except(foundEmails).ToList();
                    var customersFilter = filtered.Select(x => new
                    {
                        id = x.Id,
                        name = x.Name
                    }).ToList();

                    return Json(new { setAll = true, notFound = notFoundEmails, customers = customersFilter });
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult GetDiscountsFiltering(FilteringModel model)
        {
            if (!string.IsNullOrEmpty(model.Text) || model.ByIds.Any() || model.Texts.Any())
            {
                var discounts = _discountService.GetAllDiscounts(showHidden: true)
                        .Where(x => x.RequiresCouponCode && !string.IsNullOrEmpty(x.CouponCode));
                if (model.ByIds.Any())
                {
                    var byDiscountIds = model.ByIds.Distinct().ToList();
                    var filtered = discounts
                        .Where(x => byDiscountIds.Contains(x.Id))
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.Name} ({x.CouponCode})",
                        })
                        .ToList();
                    return Json(filtered);
                }
                if (!string.IsNullOrEmpty(model.Text))
                {
                    var text = model.Text.Trim().ToLower();
                    var filtered = discounts
                        .Where(x => x.CouponCode.ToLower().Contains(text))
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.Name} ({x.CouponCode})",
                        }).ToList();

                    return Json(filtered);
                }
                else if (model.Texts.Any())
                {
                    var coupons = model.Texts.Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => x.Trim())
                        .Distinct().ToList();
                    var filtered = discounts
                        .Where(x => coupons.Contains(x.CouponCode))
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.Name} ({x.CouponCode})",
                            coupon = $"{x.CouponCode}",
                        })
                        .ToList();
                    var foundDiscounts = filtered.Select(x => x.coupon).ToList();
                    var notFoundDiscounts = coupons.Except(foundDiscounts).ToList();

                    return Json(new { setAll = true, notFound = notFoundDiscounts, discounts = filtered });
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult GetEntitiesFiltering(FilteringModel model)
        {
            if ((!string.IsNullOrEmpty(model.Text) || model.ByIds.Any()) && model.TypeId > 0)
            {
                var entities = new List<EntityObject>();
                if (model.ByIds.Any())
                {
                    var byEntityIds = model.ByIds.Distinct().ToList();
                    if (model.TypeId == (int)TakeXPayYEntityType.Product)
                    {
                        var filtered = _productService.GetAllProductsQuery()
                        .Where(x => byEntityIds.Contains(x.Id))
                        .ToList()
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.Name} ({GetFirstCategoryBreadCrumb(x.ProductCategories.ToList())})",
                        })
                        .ToList();
                        return Json(new { set = true, data = filtered });
                    }
                    else if (model.TypeId == (int)TakeXPayYEntityType.Category)
                    {
                        var filtered = _categoryService.GetAllCategories()
                        .Where(x => byEntityIds.Contains(x.Id))
                        .Select(x => new
                        {
                            id = x.Id.ToString(),
                            name = $"{x.GetFormattedBreadCrumb(_categoryService)}",
                        })
                        .ToList();
                        return Json(new { set = true, data = filtered });
                    }
                }
                if (!string.IsNullOrEmpty(model.Text))
                {
                    var text = model.Text.Trim().ToLower();
                    if (model.TypeId == (int)TakeXPayYEntityType.Product)
                    {
                        var filtered = _productService.GetAllProductsQuery()
                            .Where(x => x.Name.ToLower().Contains(text))
                            .Select(x => new { x.Name, x.Id, x.ProductCategories }).ToList();
                        var productsFilter = filtered.Select(x => new
                        {
                            id = x.Id,
                            name = $"{x.Name} ({GetFirstCategoryBreadCrumb(x.ProductCategories.ToList())})"
                        }).ToList();

                        return Json(productsFilter);
                    }
                    else if (model.TypeId == (int)TakeXPayYEntityType.Category)
                    {
                        var filtered = _categoryService.GetAllCategories()
                            .Where(x => x.Name.ToLower().Contains(text))
                            .ToList()
                            .Select(x => new { Name = $"{x.GetFormattedBreadCrumb(_categoryService)}", x.Id }).ToList();
                        var categoriesFilter = filtered.Select(x => new
                        {
                            id = x.Id,
                            name = x.Name
                        }).ToList();

                        return Json(categoriesFilter);
                    }
                }
            }
            return Ok();
        }

        public string GetFirstCategoryBreadCrumb(List<ProductCategory> productCategories)
        {
            if (productCategories.Any())
            {
                return productCategories.FirstOrDefault().Category.GetFormattedBreadCrumb(_categoryService);
            }
            else
                return "Sin categoría asignada";
        }

        public class FilteringModel
        {
            public FilteringModel()
            {
                Texts = new List<string>();
                ByIds = new List<int>();
            }

            public string Text { get; set; }
            public IList<string> Texts { get; set; }
            public IList<int> ByIds { get; set; }

            public int TypeId { get; set; }
        }

        public class EntityObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Methods

        #region Discounts

        //list
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountListModel
            {
                AvailableDiscountTypes = DiscountType.AssignedToOrderTotal.ToSelectList(false).ToList()
            };
            model.AvailableDiscountTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            if (_catalogSettings.IgnoreDiscounts)
                WarningNotification(_localizationService.GetResource("Admin.Promotions.Discounts.IgnoreDiscounts.Warning"));

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(DiscountListModel model, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            DiscountType? discountType = null;
            if (model.SearchDiscountTypeId > 0)
                discountType = (DiscountType)model.SearchDiscountTypeId;
            var discounts = _discountService.GetAllDiscounts(discountType,
                model.SearchDiscountCouponCode,
                model.SearchDiscountName,
                model.SearchDiscountPublicNot,
                true).Where(x => x.NumSeries == 0).ToList();



            //var filteredDiscounts = discounts.Where(x => x.CustomerOwnerId == 0).Select(x =>
            //{
            //    var discountModel = x.ToModel();
            //    discountModel.DiscountTypeName = x.DiscountType.GetLocalizedEnum(_localizationService, _workContext);
            //    discountModel.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            //    discountModel.TimesUsed = _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1).TotalCount;
            //    discountModel = ChangeDatesToLocal(discountModel);
            //    return discountModel;
            //});

            //if (model.SearchDiscountPublicNot)
            //{
            //    filteredDiscounts = filteredDiscounts.Where(x => (x.LimitationTimes >= x.TimesUsed && x.DiscountLimitationId == 15) || x.DiscountLimitationId != 15);
            //}

            //var gridModel = new DataSourceResult
            //{
            //    Data = filteredDiscounts,
            //    Total = filteredDiscounts.Count()
            //};
            //return Json(gridModel);




            if (model.SearchDiscountPublicNot)
            {
                var gridmodel = new DataSourceResult
                {
                    Data = discounts.Where(x => x.CustomerOwnerId == 0).PagedForCommand(command).Select(x =>
                    {
                        var discountModel = x.ToModel();
                        var discountUsageHistory = _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1);
                        discountModel.DiscountTypeName = x.DiscountType.GetLocalizedEnum(_localizationService, _workContext);
                        discountModel.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
                        discountModel.TimesUsed = discountUsageHistory.TotalCount;
                        discountModel = ChangeDatesToLocal(discountModel);
                        return discountModel;
                    }).Where(x => (x.LimitationTimes >= x.TimesUsed && x.DiscountLimitationId == 15) || x.DiscountLimitationId != 15),
                    Total = discounts.Count
                };
                return Json(gridmodel);
            }
            else
            {
                var gridmodel = new DataSourceResult
                {
                    Data = discounts.Where(x => x.CustomerOwnerId == 0).PagedForCommand(command).Select(x =>
                    {
                        var discountModel = x.ToModel();
                        discountModel.DiscountTypeName = x.DiscountType.GetLocalizedEnum(_localizationService, _workContext);
                        discountModel.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
                        discountModel.TimesUsed = _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1).TotalCount;
                        discountModel = ChangeDatesToLocal(discountModel);
                        return discountModel;
                    }),
                    Total = discounts.Count
                };
                return Json(gridmodel);
            }
        }

        public virtual IActionResult ListMassive(string parentId)
        {
            return View("ListMassive", parentId);
        }


        [HttpPost]
        public virtual IActionResult ListMassiveData(DataSourceRequest command, string parentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var discounts = _discountService.GetAllDiscounts(showHidden: true).Where(x => x.ParentId == parentId).OrderBy(x => x.NumSeries).ToList();

            var gridmodel = new DataSourceResult
            {
                Data = discounts.Where(x => x.CustomerOwnerId == 0).PagedForCommand(command).Select(x =>
                {
                    var discountModel = x.ToModel();
                    discountModel.DiscountTypeName = x.DiscountType.GetLocalizedEnum(_localizationService, _workContext);
                    discountModel.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
                    discountModel.TimesUsed = _discountService.GetAllDiscountUsageHistory(x.Id, pageSize: 1).TotalCount;
                    discountModel = ChangeDatesToLocal(discountModel);
                    return discountModel;
                }),
                Total = discounts.Count
            };
            return Json(gridmodel);
        }

        //create
        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel();
            PrepareDiscountModel(model, null);
            //default values
            model.LimitationTimes = 1;
            // Convert dates to local time
            model = ChangeDatesToLocal(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(DiscountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            // Convert dates to Utc
            model = ChangeDatesToUtc(model);

            Discount discount = null;
            if (ModelState.IsValid)
            {
                if (model.MassiveCreation)
                {
                    var createdCoupons = new List<Discount>();
                    for (int i = 0; i < model.MassiveCreationQty; i++)
                    {
                        Discount massiveDiscount = model.ToEntity();
                        if (massiveDiscount.DiscountTypeId != (int)DiscountType.AssignedToSkus)
                            //massiveDiscount.ShouldAddProducts = false;
                        /*string newCode = string.Empty;

                        do
                        {
                            newCode = massiveDiscount.CouponCode + GenerateRandomCode(model.MassiveCharacterCount);
                        } while (_discountService.GetAllDiscounts(couponCode: newCode).Count > 0);
                        */
                        massiveDiscount.ParentId = model.CouponCode;
                        //massiveDiscount.CouponCode = newCode;
                        massiveDiscount.Name = massiveDiscount.Name + $" ({i + 1})";
                        massiveDiscount.RequiresCouponCode = true;
                        massiveDiscount.ParentId = model.CouponCode;
                        massiveDiscount.NumSeries = i;
                        massiveDiscount.MassiveCreation = model.MassiveCreation;

                        createdCoupons.Add(massiveDiscount);

                        _discountService.InsertDiscount(massiveDiscount);
                        //activity log
                        _customerActivityService.InsertActivity("AddNewDiscount", _localizationService.GetResource("ActivityLog.AddNewDiscount"), massiveDiscount.Name);
                    }

                    byte[] bytes = GetExcelBytes(createdCoupons);
                    return File(bytes, MimeTypes.TextXlsx, $"coupons_{createdCoupons.Count}_{DateTime.Now.ToString("ddMMyyyy")}.xlsx");
                }
                else
                {
                    discount = model.ToEntity();
                    discount.NumSeries = 0;
                    if (discount.DiscountTypeId != (int)DiscountType.AssignedToSkus)
                        //discount.ShouldAddProducts = false;

                    _discountService.InsertDiscount(discount);
                    //activity log
                    _customerActivityService.InsertActivity("AddNewDiscount", _localizationService.GetResource("ActivityLog.AddNewDiscount"), discount.Name);
                }

                //check limited to customers
                if (model.LimitedToCustomerIds.Count > 0)
                {
                    var emails = _customerService.GetAllCustomersQuery()
                        .Where(x => !x.Deleted && x.Active && !string.IsNullOrEmpty(x.Email) && model.LimitedToCustomerIds.Contains(x.Id))
                        .Select(x => x.Email).ToList();
                    discount.LimitedToCustomerEmail = string.Join(",", emails);
                    _discountService.UpdateDiscount(discount);
                }
                //check explusive discount
                if (model.ExclusiveUsageIds.Count > 0 || model.ExclusiveUsageAgainstIds == "0")
                {
                    /*
                    if (model.ExclusiveUsageIds.FirstOrDefault() == 0 || model.ExclusiveUsageAgainstIds == "0")
                        //discount.ExclusiveUsageAgainstIds = "0";
                    else
                    {
                        var discounts = _discountService.GetAllDiscounts(showHidden: true)
                            .Where(x => x.RequiresCouponCode && !string.IsNullOrEmpty(x.CouponCode) && model.ExclusiveUsageIds.Contains(x.Id))
                            .Select(x => x.Id).ToList();
                        //discount.ExclusiveUsageAgainstIds = string.Join(",", discounts);
                    }
                    _discountService.UpdateDiscount(discount);
                    */
                }

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Added"));

                SpecialPromotionsCheck(discount.Id, model);

                if (continueEditing && !model.MassiveCreation)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = discount.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareDiscountModel(model, null);
            return View(model);
        }

        private byte[] GetExcelBytes(List<Discount> coupons)
        {
            return _exportManager.ExportMassiveDiscountsToXlsx(coupons);
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //edit
        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            var model = discount.ToModel();
            PrepareDiscountModel(model, discount);
            // Convert dates to local time
            model = ChangeDatesToLocal(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(DiscountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            // Convert dates to Utc
            model = ChangeDatesToUtc(model);

            var discount = _discountService.GetDiscountById(model.Id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevDiscountType = discount.DiscountType;
                discount = model.ToEntity(discount);
                if (discount.DiscountTypeId != (int)DiscountType.AssignedToSkus)
                    //discount.ShouldAddProducts = false;
                _discountService.UpdateDiscount(discount);

                //clean up old references (if changed) and update "HasDiscountsApplied" properties
                if (prevDiscountType == DiscountType.AssignedToCategories
                    && discount.DiscountType != DiscountType.AssignedToCategories)
                {
                    //applied to categories
                    discount.AppliedToCategories.Clear();
                    _discountService.UpdateDiscount(discount);
                }
                if (prevDiscountType == DiscountType.AssignedToManufacturers
                    && discount.DiscountType != DiscountType.AssignedToManufacturers)
                {
                    //applied to manufacturers
                    discount.AppliedToManufacturers.Clear();
                    _discountService.UpdateDiscount(discount);
                }
                if (prevDiscountType == DiscountType.AssignedToSkus
                    && discount.DiscountType != DiscountType.AssignedToSkus)
                {
                    //applied to products
                    var products = discount.AppliedToProducts.ToList();
                    discount.AppliedToProducts.Clear();
                    _discountService.UpdateDiscount(discount);
                    //update "HasDiscountsApplied" property
                    foreach (var p in products)
                        _productService.UpdateHasDiscountsApplied(p);
                }

                //activity log
                _customerActivityService.InsertActivity("EditDiscount", _localizationService.GetResource("ActivityLog.EditDiscount"), discount.Name);

                //check limited to customers
                if (model.LimitedToCustomerIds.Count > 0)
                {
                    var emails = _customerService.GetAllCustomersQuery()
                        .Where(x => !x.Deleted && x.Active && !string.IsNullOrEmpty(x.Email) && model.LimitedToCustomerIds.Contains(x.Id))
                        .Select(x => x.Email).ToList();
                    discount.LimitedToCustomerEmail = string.Join(",", emails);
                    _discountService.UpdateDiscount(discount);
                }
                else
                {
                    discount.LimitedToCustomerEmail = null;
                    _discountService.UpdateDiscount(discount);
                }

                //check explusive discount
                if (model.ExclusiveUsageIds.Count > 0 || model.ExclusiveUsageAgainstIds == "0")
                {
                    if (model.ExclusiveUsageIds.FirstOrDefault() == 0 || model.ExclusiveUsageAgainstIds == "0") { }
                        //discount.ExclusiveUsageAgainstIds = "0";
                    else
                    {
                        var discounts = _discountService.GetAllDiscounts(showHidden: true)
                        .Where(x => x.RequiresCouponCode && !string.IsNullOrEmpty(x.CouponCode) && model.ExclusiveUsageIds.Contains(x.Id))
                        .Select(x => x.Id).ToList();
                        //discount.ExclusiveUsageAgainstIds = string.Join(",", discounts);
                    }
                }
                else
                    //discount.ExclusiveUsageAgainstIds = null;
                _discountService.UpdateDiscount(discount);

                SpecialPromotionsCheck(discount.Id, model);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = discount.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareDiscountModel(model, discount);
            return View(model);
        }

        //delete
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(id);
            if (discount == null)
                //No discount found with the specified id
                return RedirectToAction("List");

            //applied to products
            var products = discount.AppliedToProducts.ToList();

            _discountService.DeleteDiscount(discount);

            //update "HasDiscountsApplied" properties
            foreach (var p in products)
                _productService.UpdateHasDiscountsApplied(p);

            //activity log
            _customerActivityService.InsertActivity("DeleteDiscount", _localizationService.GetResource("ActivityLog.DeleteDiscount"), discount.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Discount requirements

        public virtual IActionResult GetDiscountRequirementConfigurationUrl(string systemName, int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var discountRequirementRule = _discountService.LoadDiscountRequirementRuleBySystemName(systemName);
            if (discountRequirementRule == null)
                throw new ArgumentException("Discount requirement rule could not be loaded");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var url = GetRequirementUrlInternal(discountRequirementRule, discount, discountRequirementId);
            return Json(new { url = url });
        }

        public virtual IActionResult GetDiscountRequirements(int discountId, int discountRequirementId,
            int? parentId, int? interactionTypeId, bool deleteRequirement)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var requirements = new List<DiscountModel.DiscountRequirementMetaInfo>();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                return Json(requirements);

            var discountRequirement = discount.DiscountRequirements.FirstOrDefault(requirement => requirement.Id == discountRequirementId);
            if (discountRequirement != null)
            {
                //delete
                if (deleteRequirement)
                {
                    DeleteRequirement(new List<DiscountRequirement> { discountRequirement });

                    //delete default group if there are no any requirements
                    if (!discount.DiscountRequirements.Any(requirement => requirement.ParentId.HasValue))
                        DeleteRequirement(discount.DiscountRequirements);
                }
                //or update the requirement
                else
                {
                    var defaultGroupId = discount.DiscountRequirements.FirstOrDefault(requirement =>
                        !requirement.ParentId.HasValue && requirement.IsGroup)?.Id ?? 0;
                    if (defaultGroupId == 0)
                    {
                        //add default requirement group
                        var defaultGroup = new DiscountRequirement
                        {
                            IsGroup = true,
                            InteractionType = RequirementGroupInteractionType.And,
                            DiscountRequirementRuleSystemName = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DefaultRequirementGroup")
                        };
                        discount.DiscountRequirements.Add(defaultGroup);
                        _discountService.UpdateDiscount(discount);
                        defaultGroupId = defaultGroup.Id;
                    }

                    //set parent identifier if specified
                    if (parentId.HasValue)
                        discountRequirement.ParentId = parentId.Value;
                    else
                    {
                        //or default group identifier
                        if (defaultGroupId != discountRequirement.Id)
                            discountRequirement.ParentId = defaultGroupId;
                    }

                    //set interaction type
                    if (interactionTypeId.HasValue)
                        discountRequirement.InteractionTypeId = interactionTypeId;

                    _discountService.UpdateDiscount(discount);
                }
            }

            //get current requirements
            var topLevelRequirements = discount.DiscountRequirements.Where(requirement => !requirement.ParentId.HasValue && requirement.IsGroup).ToList();

            //get interaction type of top-level group
            var interactionType = topLevelRequirements.FirstOrDefault()?.InteractionType;

            if (interactionType.HasValue)
                requirements = GetReqirements(topLevelRequirements, interactionType.Value, discount).ToList();

            //get available groups
            var requirementGroups = discount.DiscountRequirements.Where(requirement => requirement.IsGroup);
            var availableRequirementGroups = requirementGroups.Select(requirement =>
                new SelectListItem { Value = requirement.Id.ToString(), Text = requirement.DiscountRequirementRuleSystemName }).ToList();

            return Json(new { Requirements = requirements, AvailableGroups = availableRequirementGroups });
        }

        public virtual IActionResult AddNewGroup(int discountId, string name)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var defaultGroup = discount.DiscountRequirements.FirstOrDefault(requirement => !requirement.ParentId.HasValue && requirement.IsGroup);
            if (defaultGroup == null)
            {
                //add default requirement group
                discount.DiscountRequirements.Add(new DiscountRequirement
                {
                    IsGroup = true,
                    InteractionType = RequirementGroupInteractionType.And,
                    DiscountRequirementRuleSystemName = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DefaultRequirementGroup")
                });
            }

            //save new requirement group
            var discountRequirementGroup = new DiscountRequirement
            {
                IsGroup = true,
                DiscountRequirementRuleSystemName = name,
                InteractionType = RequirementGroupInteractionType.And
            };
            discount.DiscountRequirements.Add(discountRequirementGroup);
            _discountService.UpdateDiscount(discount);

            //set identifier as group name (if not specified)
            if (string.IsNullOrEmpty(name))
            {
                discountRequirementGroup.DiscountRequirementRuleSystemName = $"#{discountRequirementGroup.Id}";
                _discountService.UpdateDiscount(discount);
            }

            return Json(new { Result = true, NewRequirementId = discountRequirementGroup.Id });
        }

        #endregion

        #region Applied to products

        [HttpPost]
        public virtual IActionResult ProductList(DataSourceRequest command, int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var products = discount
                .AppliedToProducts
                .Where(x => !x.Deleted)
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new DiscountModel.AppliedToProductModel
                {
                    ProductId = x.Id,
                    ProductName = x.Name
                }),
                Total = products.Count
            };

            return Json(gridModel);
        }

        public virtual IActionResult ProductDelete(int discountId, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new Exception("No product found with the specified id");

            //remove discount
            if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                product.AppliedDiscounts.Remove(discount);

            _productService.UpdateProduct(product);
            _productService.UpdateHasDiscountsApplied(product);

            return new NullJsonResult();
        }

        public virtual IActionResult ProductAddPopup(int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddProductToDiscountModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var manufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);
            foreach (var m in manufacturers)
                model.AvailableManufacturers.Add(m);

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ProductAddPopupList(DataSourceRequest command, DiscountModel.AddProductToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
            );
            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };


            return Json(gridModel);
        }



        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult ProductAddPopup(DiscountModel.AddProductToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedProductIds != null)
            {
                foreach (var id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            product.AppliedDiscounts.Add(discount);

                        _productService.UpdateProduct(product);
                        _productService.UpdateHasDiscountsApplied(product);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to categories

        [HttpPost]
        public virtual IActionResult CategoryList(DataSourceRequest command, int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var categories = discount
                .AppliedToCategories
                .Where(x => !x.Deleted)
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x => new DiscountModel.AppliedToCategoryModel
                {
                    CategoryId = x.Id,
                    CategoryName = x.GetFormattedBreadCrumb(_categoryService)
                }),
                Total = categories.Count
            };

            return Json(gridModel);
        }

        public virtual IActionResult CategoryDelete(int discountId, int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                throw new Exception("No category found with the specified id");

            //remove discount
            if (category.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                category.AppliedDiscounts.Remove(discount);

            _categoryService.UpdateCategory(category);

            return new NullJsonResult();
        }

        public virtual IActionResult CategoryAddPopup(int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddCategoryToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CategoryAddPopupList(DataSourceRequest command, DiscountModel.AddCategoryToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName,
                0, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult CategoryAddPopup(DiscountModel.AddCategoryToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedCategoryIds != null)
            {
                foreach (var id in model.SelectedCategoryIds)
                {
                    var category = _categoryService.GetCategoryById(id);
                    if (category != null)
                    {
                        if (category.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            category.AppliedDiscounts.Add(discount);

                        _categoryService.UpdateCategory(category);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Applied to manufacturers

        [HttpPost]
        public virtual IActionResult ManufacturerList(DataSourceRequest command, int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturers = discount
                .AppliedToManufacturers
                .Where(x => !x.Deleted)
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => new DiscountModel.AppliedToManufacturerModel
                {
                    ManufacturerId = x.Id,
                    ManufacturerName = x.Name
                }),
                Total = manufacturers.Count
            };

            return Json(gridModel);
        }

        public virtual IActionResult ManufacturerDelete(int discountId, int manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                throw new Exception("No manufacturer found with the specified id");

            //remove discount
            if (manufacturer.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                manufacturer.AppliedDiscounts.Remove(discount);

            _manufacturerService.UpdateManufacturer(manufacturer);

            return new NullJsonResult();
        }

        public virtual IActionResult ManufacturerAddPopup(int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new DiscountModel.AddManufacturerToDiscountModel();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ManufacturerAddPopupList(DataSourceRequest command, DiscountModel.AddManufacturerToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName,
                0, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult ManufacturerAddPopup(DiscountModel.AddManufacturerToDiscountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                throw new Exception("No discount found with the specified id");

            if (model.SelectedManufacturerIds != null)
            {
                foreach (var id in model.SelectedManufacturerIds)
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(id);
                    if (manufacturer != null)
                    {
                        if (manufacturer.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            manufacturer.AppliedDiscounts.Add(discount);

                        _manufacturerService.UpdateManufacturer(manufacturer);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Discount usage history

        [HttpPost]
        public virtual IActionResult UsageHistoryList(int discountId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var duh = _discountService.GetAllDiscountUsageHistory(discount.Id, null, null, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = duh.Select(x =>
                {
                    var order = _orderService.GetOrderById(x.OrderId);
                    var duhModel = new DiscountModel.DiscountUsageHistoryModel
                    {
                        Id = x.Id,
                        DiscountId = x.DiscountId,
                        OrderId = x.OrderId,
                        OrderTotal = order != null ? _priceFormatter.FormatPrice(order.OrderTotal, true, false) : "",
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                        CustomOrderNumber = x.Order.CustomOrderNumber
                    };
                    return duhModel;
                }),
                Total = duh.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public virtual IActionResult UsageHistoryDelete(int discountId, int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("No discount found with the specified id");

            var duh = _discountService.GetDiscountUsageHistoryById(id);
            if (duh != null)
                _discountService.DeleteDiscountUsageHistory(duh);

            return new NullJsonResult();
        }

        #endregion

        #region Helpers

        public DiscountModel ChangeDatesToUtc(DiscountModel model)
        {
            if (!model.StartDateUtc.Equals(null))
                model.StartDateUtc = TimeZoneInfo.ConvertTimeToUtc(model.StartDateUtc.Value);
            if (!model.EndDateUtc.Equals(null))
                model.EndDateUtc = TimeZoneInfo.ConvertTimeToUtc(model.EndDateUtc.Value);
            return model;
        }

        public DiscountModel ChangeDatesToLocal(DiscountModel model)
        {
            if (!model.StartDateUtc.Equals(null))
                model.StartDateUtc = TimeZoneInfo.ConvertTimeFromUtc(model.StartDateUtc.Value, TimeZoneInfo.Local);
            if (!model.EndDateUtc.Equals(null))
                model.EndDateUtc = TimeZoneInfo.ConvertTimeFromUtc(model.EndDateUtc.Value, TimeZoneInfo.Local);
            return model;
        }

        public IActionResult DuplicateDiscount(int id, string text, int quantity = 1, int startFrom = 0)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var final = "";
            if (id > 0)
            {
                quantity = quantity < 1 ? 1 : quantity;
                startFrom = startFrom < 1 ? 1 : startFrom;
                try
                {
                    var discount = _discountService.GetDiscountById(id);
                    if (discount != null)
                    {
                        for (int i = startFrom; i < quantity; i++)
                        {
                            discount.CouponCode = text + (i + 1);

                            var products = discount.AppliedToProducts.ToList();
                            var manufacturers = discount.AppliedToManufacturers.ToList();
                            var categories = discount.AppliedToCategories.ToList();

                            discount.AppliedToCategories.Clear();
                            discount.AppliedToManufacturers.Clear();
                            discount.AppliedToProducts.Clear();

                            _discountService.InsertDiscount(discount);
                            if (products.Any())
                            {
                                foreach (var elementId in products.Select(x => x.Id))
                                {
                                    var product = _productService.GetProductById(elementId);
                                    if (product != null)
                                    {
                                        if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                                            product.AppliedDiscounts.Add(discount);

                                        _productService.UpdateProduct(product);
                                        _productService.UpdateHasDiscountsApplied(product);
                                    }
                                }
                            }
                        }
                    }
                    else
                        final = "ERROR: Discount with given Id returns null.";
                }
                catch (Exception e)
                {
                    final = e.Message;
                }
            }
            else
                final = "ERROR: No Id or text specified.";
            return Ok(final);
        }


        public IActionResult ChangeDiscountDuplicationNumber(int primalDiscountId, int characterCount = 5)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var final = "";
            try
            {
                var primalDiscount = _discountService.GetDiscountById(primalDiscountId);

                if (primalDiscount == null)
                    return BadRequest("coupon doesnt exist with given Id");

                var discounts = _discountService.GetAllDiscounts()
                    .Where(x => x.Name.ToLower() == primalDiscount.Name.ToLower())
                    .ToList();
                if (discounts.Any())
                {
                    foreach (var discount in discounts)
                    {
                        discount.CouponCode = Regex.Replace(discount.CouponCode, "[0-9]", string.Empty);
                        var newCode = string.Empty;
                        do
                        {
                            newCode = GenerateRandomCode(characterCount);
                        } while (_discountService.GetAllDiscounts(couponCode: discount.CouponCode + newCode).Count > 0);
                        discount.CouponCode += newCode;
                        _discountService.UpdateDiscount(discount);
                    }
                    final = $"Changed {discounts.Count} discounts coupon codes.";
                }
                else
                    final = "ERROR: Discount with given prefix returns empty.";
            }
            catch (Exception e)
            {
                final = e.Message;
            }
            return Ok(final);
        }

        #endregion

        #endregion
    }
}