using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.TaggableBoxes;
using Teed.Plugin.Api.Dtos.Address;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Models;
using Teed.Plugin.Api.Services;
using Teed.Plugin.Api.Security;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Teed.Plugin.Api.Controllers
{
    [Area(AreaNames.Admin)]
    public class TaggableBoxConfigController : BasePluginController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly TaggableBoxService _taggableBoxService;
        private readonly IWorkContext _workContext;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public TaggableBoxConfigController(ICustomerService customerService,
            IPermissionService permissionService,
            TaggableBoxService taggableBoxService,
            IWorkContext workContext,
            IProductTagService productTagService,
            ICategoryService categoryService,
            IProductService productService)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _taggableBoxService = taggableBoxService;
            _workContext = workContext;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _productService = productService;
        }

        #endregion

        #region Methods

        public IActionResult List()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            return View("~/Plugins/Teed.Plugin.Api/Views/TaggableBoxConfig/List.cshtml");
        }

        [HttpPost]
        public IActionResult ListData(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            var queryList = _taggableBoxService.GetAll().ToList();
            var pagedList = new PagedList<TaggableBox>(queryList, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(x => new
                {
                    x.Id,
                    x.Name,
                    Element = GetElementTypeName(x),
                    Type = ((ElementType)x.Type).GetDisplayName(),
                    Position = ((BoxPosition)x.Position).GetDisplayName(),
                }).ToList(),
                Total = queryList.Count()
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            var model = new TaggableBoxModel();
            PrepareModel(model);

            return View("~/Plugins/Teed.Plugin.Api/Views/TaggableBoxConfig/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(TaggableBoxModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            var taggableBox = new TaggableBox
            {
                Id = model.Id,
                Name = model.Name,
                Position = model.Position,
                Type = model.Type,
                ElementId = model.ElementId,
                PictureId = model.PictureId,
                Log = $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) creó la nueva caja etiquetable con nombre {model.Name}\n"
            };

            _taggableBoxService.Insert(taggableBox);

            if (continueEditing)
            {
                PrepareModel(model);
                return RedirectToAction("Edit", new { id = taggableBox.Id });
            }
            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            var taggableBox = _taggableBoxService.GetById(id);
            if (taggableBox == null)
                return RedirectToAction("List");

            var model = new TaggableBoxModel
            {
                Id = taggableBox.Id,
                Name = taggableBox.Name,
                Position = taggableBox.Position,
                Type = taggableBox.Type,
                ElementId = taggableBox.ElementId,
                PictureId = taggableBox.PictureId,
                Log = taggableBox.Log
            };

            PrepareModel(model);
            return View("~/Plugins/Teed.Plugin.Api/Views/TaggableBoxConfig/Edit.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(TaggableBoxModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(model.Name))
                return View("~/Plugins/Teed.Plugin.Api/Views/TaggableBoxConfig/Edit.cshtml", model);

            var taggableBox = _taggableBoxService.GetById(model.Id);
            if (taggableBox == null)
                return RedirectToAction("List");

            if (taggableBox.Name != model.Name)
            {
                taggableBox.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el nombre de {taggableBox.Name} a {model.Name}\n";
                taggableBox.Name = model.Name;
            }

            if (taggableBox.Position != model.Position)
            {
                taggableBox.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la posición de {((BoxPosition)taggableBox.Position).GetDisplayName()} a {((BoxPosition)model.Position).GetDisplayName()}\n";
                taggableBox.Position = model.Position;
            }

            if (taggableBox.Type != model.Type)
            {
                taggableBox.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el tipo de elemento de {((ElementType)taggableBox.Type).GetDisplayName()} a {((ElementType)model.Type).GetDisplayName()}\n";
                taggableBox.Type = model.Type;
            }

            if (taggableBox.ElementId != model.ElementId)
            {
                taggableBox.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió el elemento de {GetElementTypeName(null, taggableBox.Type, taggableBox.ElementId)} a {GetElementTypeName(null, model.Type, model.ElementId)}\n";
                taggableBox.ElementId = model.ElementId;
            }

            if (taggableBox.PictureId != model.PictureId)
            {
                taggableBox.Log += $"{DateTime.Now:dd-MM-yyyy hh:mm:ss tt} - El usuario {_workContext.CurrentCustomer.Email} ({_workContext.CurrentCustomer.Id}) cambió la imagen de {GetElementTypeName(null, taggableBox.Type, taggableBox.PictureId)} a {GetElementTypeName(null, model.Type, model.PictureId)}\n";
                taggableBox.PictureId = model.PictureId;
            }

            _taggableBoxService.Update(taggableBox);

            if (continueEditing)
            {
                PrepareModel(model);
                return RedirectToAction("Edit", new { id = taggableBox.Id });
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(TeedApiPermissionProvider.TaggableBoxConfig))
                return AccessDeniedView();

            var taggableBox = _taggableBoxService.GetById(id);
            if (taggableBox == null)
                return RedirectToAction("List");

            taggableBox.Deleted = true;
            _taggableBoxService.Update(taggableBox);

            return RedirectToAction("List");
        }

        public void PrepareModel(TaggableBoxModel model)
        {
            model.Positions = Enum.GetValues(typeof(BoxPosition)).Cast<BoxPosition>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = ((int)v).ToString()
            }).OrderBy(x => x.Text).ToList();

            model.Types.Add(new SelectListItem
            {
                Selected = true,
                Disabled = true,
                Text = "Selecciona el tipo de elemento...",
                Value = "-1"
            });

            model.Types.AddRange(Enum.GetValues(typeof(ElementType)).Cast<ElementType>().Select(v => new SelectListItem
            {
                Text = v.GetDisplayName(),
                Value = ((int)v).ToString()
            }).OrderBy(x => x.Text).ToList());
        }

        public string GetElementTypeName(TaggableBox box, int typeId = 0, int elementId = 0)
        {
            var final = "Sin elemento especificado";
            if (box != null)
                switch (box.Type)
                {
                    case (int)ElementType.Tag:
                        var tag = _productTagService.GetProductTagById(box.ElementId);
                        if (tag != null)
                            final = tag.Name;
                        break;
                    case (int)ElementType.Product:
                        var product = _productService.GetProductById(box.ElementId);
                        if (product != null)
                            final = product.Name;
                        break;
                    case (int)ElementType.Category:
                        var category = _categoryService.GetCategoryById(box.ElementId);
                        if (category != null)
                            final = category.GetFormattedBreadCrumb(_categoryService);
                        break;
                    default:
                        break;
                }
            else
                switch (typeId)
                {
                    case (int)ElementType.Tag:
                        var tag = _productTagService.GetProductTagById(elementId);
                        if (tag != null)
                            final = tag.Name;
                        break;
                    case (int)ElementType.Product:
                        var product = _productService.GetProductById(elementId);
                        if (product != null)
                            final = product.Name;
                        break;
                    case (int)ElementType.Category:
                        var category = _categoryService.GetCategoryById(elementId);
                        if (category != null)
                            final = category.GetFormattedBreadCrumb(_categoryService);
                        break;
                    default:
                        break;
                }
            return final;
        }

        [HttpGet]
        public IActionResult GetElementsByType(int type = -1)
        {
            var elements = new List<SelectListItem>();
            switch (type)
            {
                case (int)ElementType.Tag:
                    elements = _productTagService.GetAllProductTags()
                        .Select(x => new SelectListItem {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                    break;
                case (int)ElementType.Product:
                    elements = _productService.GetAllProductsQuery()
                        .Where(x => x.Published && !x.Deleted)
                        .Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                    break;
                case (int)ElementType.Category:
                    elements = _categoryService.GetAllCategories()
                        .Where(x => x.Published && !x.Deleted)
                        .Select(x => new SelectListItem
                        {
                            Text = x.GetFormattedBreadCrumb(_categoryService),
                            Value = x.Id.ToString()
                        }).ToList();
                    break;
                default:
                    break;
            }
            elements = elements.OrderBy(x => x.Text).ToList();
            return Json(elements);
        }

        #endregion
    }
}
