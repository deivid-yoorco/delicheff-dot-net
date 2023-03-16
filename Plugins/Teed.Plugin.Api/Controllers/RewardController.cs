using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Rewards;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Reward;

namespace Teed.Plugin.Api.Controllers
{
    public class RewardController : ApiBaseController
    {
        #region Properties

        private readonly ICustomerService _customerService;
        private readonly ICustomerPointService _customerPointService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public RewardController(ICustomerService customerService, 
            ICustomerPointService customerPointService,
            ISettingService settingService)
        {
            _customerPointService = customerPointService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult GetCurrentUserPoints()
        {
            RewardSettings settings = _settingService.LoadSetting<RewardSettings>();
            var customerPoints = _customerPointService.GetCustomerPointsBalance(int.Parse(UserId));

            var dto = new CurrentUserPointsDto()
            {
                IsActive = settings.IsActive,
                Points = customerPoints
            };

            return Ok(dto);
        }

        #endregion
    }
}
