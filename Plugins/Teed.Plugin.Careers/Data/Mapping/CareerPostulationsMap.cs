using Nop.Data.Mapping;
using Teed.Plugin.Careers.Domain;

namespace Teed.Plugin.Careers.Data.Mapping
{
    public class CareerPostulationsMap : NopEntityTypeConfiguration<CareerPostulations>
    {
        public CareerPostulationsMap()
        {
            ToTable(nameof(CareerPostulations));
            HasKey(x => x.Id);
        }
    }
}