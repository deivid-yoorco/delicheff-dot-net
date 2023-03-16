using Nop.Core;
using System;

namespace Teed.Plugin.Medical.Domain
{
    public class Patient : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Phone1 { get; set; }
        public virtual string Phone2 { get; set; }
        public virtual string Phone3 { get; set; }
        public virtual string StreetAddress { get; set; }
        public virtual string StreetAddress2 { get; set; }
        public virtual string City { get; set; }
        public virtual int CountryId { get; set; }
        public virtual int StateProvinceId { get; set; }
        public virtual string ZipPostalCode { get; set; }

        public virtual int ReferedBy { get; set; }
        public virtual string ReferedByExternal { get; set; }
        public virtual string FamilyBackground { get; set; }
        public virtual string PersonalPathologicalHistory { get; set; }
        public virtual string PersonalNonPathologicalHistory { get; set; }
        public virtual string CurrentConditions { get; set; }
        public virtual string Allergies { get; set; }
        public virtual string Commentary { get; set; }
        public virtual bool UpdatePageActive { get; set; }
        public virtual DateTime? UpdatePageActivationDateOnUtc { get; set; }

        public virtual string DL_UUID { get; set; }
        public virtual string Occupation { get; set; }
        public virtual string BillingFullName { get; set; }
        public virtual string BillingEmail { get; set; }
        public virtual string RFC { get; set; }
        public virtual string BillingStreetAddress { get; set; }
        public virtual string BillingStreetAddress2 { get; set; }
        public virtual string BillingCity { get; set; }
        public virtual int BillingCountryId { get; set; }
        public virtual int BillingStateProvinceId { get; set; }
        public virtual string BillingZipPostalCode { get; set; }
    }
}
