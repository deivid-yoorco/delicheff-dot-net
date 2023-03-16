using System;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents an sms verification 
    /// </summary>
    public partial class SmsVerification : BaseEntity
    {

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance last update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets if the entity is deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer's email
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the phone number for which the verification is made for
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the verification code for this entry
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// Gets or sets if the code has been verified succesfully
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the log
        /// </summary>
        public string Log { get; set; }
    }
}
