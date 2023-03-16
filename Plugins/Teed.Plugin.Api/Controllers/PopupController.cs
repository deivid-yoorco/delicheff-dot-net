using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Popups;
using Teed.Plugin.Api.Dtos.Address;
using Teed.Plugin.Api.Dtos.Customer;
using Teed.Plugin.Api.Models;
using Teed.Plugin.Api.Services;

namespace Teed.Plugin.Api.Controllers
{
    public class PopupController : ApiBaseController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly PopupService _popupService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public PopupController(ICustomerService customerService,
            IPermissionService permissionService,
            PopupService popupService,
            IWorkContext workContext,
            IPictureService pictureService)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _popupService = popupService;
            _workContext = workContext;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPopupData(bool isFirstTime, bool mobileOnly = true)
        {
            DateTime now = DateTime.Now.Date;
            DayOfWeek today = now.DayOfWeek;

            var popupQuery = _popupService.GetAll()
                .Where(x => x.Active && (x.ViewableDeadlineDate == null || x.ViewableDeadlineDate >= now) && x.ImageId.HasValue);
            if (isFirstTime)
            {
                popupQuery = popupQuery.Where(x => x.FirstTimeOnly);
            }
            else if (today == DayOfWeek.Monday)
            {
                popupQuery = popupQuery.Where(x => x.Mondays);
            }
            else if (today == DayOfWeek.Tuesday)
            {
                popupQuery = popupQuery.Where(x => x.Tuesdays);
            }
            else if (today == DayOfWeek.Wednesday)
            {
                popupQuery = popupQuery.Where(x => x.Wednesdays);
            }
            else if (today == DayOfWeek.Thursday)
            {
                popupQuery = popupQuery.Where(x => x.Thursdays);
            }
            else if (today == DayOfWeek.Friday)
            {
                popupQuery = popupQuery.Where(x => x.Fridays);
            }
            else if (today == DayOfWeek.Saturday)
            {
                popupQuery = popupQuery.Where(x => x.Saturdays);
            }
            else if (today == DayOfWeek.Sunday)
            {
                popupQuery = popupQuery.Where(x => x.Sundays);
            }

            if (mobileOnly)
            {
                var imageUrls = popupQuery.Select(x => "/Popup/PopupImage?id=" + x.ImageId.Value).ToList();
                return Ok(imageUrls);
            }
            else
            {
                var imageUrls = popupQuery.Select(x => new
                {
                    mobile = "/Popup/PopupImage?id=" + x.ImageId.Value,
                    desktop = "/Popup/PopupImage?id=" + x.ImageForDesktopId.Value,
                }).ToList();
                return Ok(imageUrls);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult PopupImage(int id)
        {
            var picture = _pictureService.GetPictureById(id);
            byte[] bytes = picture?.PictureBinary;
            string mimeType = picture?.MimeType;

            if (picture == null)
            {
                using (WebClient client = new WebClient())
                {
                    bytes = client.DownloadData(_pictureService.GetDefaultPictureUrl());
                    mimeType = "image/png";
                }
            }

            return File(bytes, mimeType);
        }

        #endregion
    }
}
