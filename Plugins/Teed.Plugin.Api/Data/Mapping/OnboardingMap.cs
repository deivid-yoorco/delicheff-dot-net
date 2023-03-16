using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Onboardings;

namespace Teed.Plugin.Api.Data.Mapping
{
    public class OnboardingMap : NopEntityTypeConfiguration<Onboarding>
    {
        public OnboardingMap()
        {
            ToTable("Onboardings");
            HasKey(u => u.Id);
        }
    }
}
