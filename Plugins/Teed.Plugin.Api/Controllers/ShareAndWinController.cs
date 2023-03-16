using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.ShareAndWin;

namespace Teed.Plugin.Api.Controllers
{
    public class ShareAndWinController : ApiBaseController
    {
        #region Properties

        private readonly ISettingService _settingService;
        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public ShareAndWinController(ISettingService settingService,
            IDiscountService discountService)
        {
            _settingService = settingService;
            _discountService = discountService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetShareAndWinConfig()
        {
            int userId = int.Parse(UserId);

            var growthHackingSettings = _settingService.LoadSetting<GrowthHackingSettings>();
            var dto = new ShareAndWinDto()
            {
                MinimumAmountToCreateFriendCode = growthHackingSettings.MinimumAmountToCreateFriendCode,
                CustomerCode = _discountService.GetAllDiscounts().Where(x => x.CustomerOwnerId == userId).Select(x => x.CouponCode).FirstOrDefault(),
                RewardAmount = growthHackingSettings.RewardAmount,
                CouponValue = growthHackingSettings.UserCodeCouponAmount,
                MinimumAmountToUseFriendCode = growthHackingSettings.UserCodeCouponOrderMinimumAmount
            };

            return Ok(dto);
        }

        [HttpPost]
        public IActionResult UpdateCode(string newCode)
        {
            int userId = int.Parse(UserId);

            if (string.IsNullOrEmpty(newCode))
                return BadRequest("El código no debe estar vacío.");

            newCode = newCode.Trim().ToUpper();

            if (newCode.Count() > 12)
                return BadRequest("El código no puede tener más de 12 caracteres.");

            if (newCode.Count() < 5)
                return BadRequest("El código debe tener al menos 5 caracteres.");

            if (!Regex.IsMatch(newCode, @"^[a-zA-Z0-9]+$"))
                return BadRequest("El código solo puede tener caracteres alfanuméricos.");

            var discountWithSameCode = _discountService.GetAllDiscounts(couponCode: newCode).Any();
            if (discountWithSameCode)
                return BadRequest("El código ya está siendo utilizado, por favor, ingresa otro.");

            var discount = _discountService.GetAllDiscounts().Where(x => x.CustomerOwnerId == userId).FirstOrDefault();
            if (discount == null) return BadRequest("Ocurrió un problema al intentar actualizar tu código. Por favor, inténtalo de nuevo más tarde.");

            discount.CouponCode = newCode;
            _discountService.UpdateDiscount(discount);

            return NoContent();
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
