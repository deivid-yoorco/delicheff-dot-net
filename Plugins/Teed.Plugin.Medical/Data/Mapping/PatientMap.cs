using Nop.Data.Mapping;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Data.Mapping
{
    public class PatientMap : NopEntityTypeConfiguration<Patient>
    {
        public PatientMap()
        {
            ToTable("Patients");

            HasKey(m => m.Id); // Primary Key
            Property(m => m.CreatedOnUtc);
            Property(m => m.UpdatedOnUtc);
            Property(m => m.Deleted);

            Property(m => m.FirstName).HasMaxLength(256);
            Property(m => m.LastName).HasMaxLength(256);
            Property(m => m.Email).HasMaxLength(256).IsOptional();
            Property(m => m.DateOfBirth).IsOptional();
            Property(m => m.Gender).HasMaxLength(10).IsOptional();
            Property(m => m.Phone1).HasMaxLength(256).IsRequired();
            Property(m => m.Phone2).HasMaxLength(256);
            Property(m => m.Phone3).HasMaxLength(256);
            Property(m => m.StreetAddress).HasMaxLength(500).IsOptional(); 
            Property(m => m.StreetAddress2).HasMaxLength(500).IsOptional(); 
            Property(m => m.City).HasMaxLength(256).IsOptional(); 
            Property(m => m.StateProvinceId).IsOptional(); 
            Property(m => m.CountryId).IsOptional(); 
            Property(m => m.ZipPostalCode).HasMaxLength(256).IsOptional(); 

            Property(m => m.FamilyBackground).HasMaxLength(1024).IsOptional();
            Property(m => m.PersonalPathologicalHistory).HasMaxLength(1024).IsOptional();
            Property(m => m.PersonalNonPathologicalHistory).HasMaxLength(1024).IsOptional();
            Property(m => m.CurrentConditions).HasMaxLength(1024).IsOptional();
            Property(m => m.Allergies).HasMaxLength(1024).IsOptional();
            Property(m => m.ReferedBy).IsOptional();
            Property(m => m.Commentary).HasMaxLength(1024).IsOptional();
            Property(m => m.UpdatePageActivationDateOnUtc).IsOptional();
            Property(m => m.UpdatePageActive).IsOptional();

            Property(m => m.DL_UUID).HasMaxLength(256).IsOptional();
            Property(m => m.Occupation).HasMaxLength(256).IsOptional();
            Property(m => m.BillingFullName).HasMaxLength(256).IsOptional();
            Property(m => m.BillingEmail).HasMaxLength(256).IsOptional();
            Property(m => m.RFC).HasMaxLength(256).IsOptional();
            Property(m => m.BillingStreetAddress).HasMaxLength(256).IsOptional();
            Property(m => m.BillingStreetAddress2).HasMaxLength(256).IsOptional();
            Property(m => m.BillingCity).HasMaxLength(256).IsOptional();
            Property(m => m.BillingCountryId).IsOptional();
            Property(m => m.BillingStateProvinceId).IsOptional();
            Property(m => m.BillingZipPostalCode).HasMaxLength(256).IsOptional();
        }
    }
}