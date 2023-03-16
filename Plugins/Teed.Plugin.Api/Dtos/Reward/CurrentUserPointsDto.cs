using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Api.Dtos.Reward
{
    public class CurrentUserPointsDto
    {
        public decimal Points { get; set; }
        public bool IsActive { get; set; }
    }
}
