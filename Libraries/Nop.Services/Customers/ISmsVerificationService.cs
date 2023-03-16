using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Sms verification service
    /// </summary>
    public partial interface ISmsVerificationService
    {
        /// <summary>
        /// Gets a smsVerification
        /// </summary>
        /// <param name="smsVerificationId">The smsVerification identifier</param>
        /// <returns>SmsVerification</returns>
        SmsVerification GetSmsVerificationById(int smsVerificationId);

        /// <summary>
        /// Gets a verified smsVerification by customer email
        /// </summary>
        /// <param name="customerEmail">customer email</param>
        /// <returns>SmsVerification</returns>
        SmsVerification GetSmsVerificationByCustomerEmail(string customerEmail);

        /// <summary>
        /// Get smsVerifications by customer identifier
        /// </summary>
        /// <param name="customerId">customer identifiers</param>
        /// <returns>List<SmsVerification></returns>
        IList<SmsVerification> GetSmsVerificationsByCustomerId(int customerId);

        /// <summary>
        /// Gets smsVerifications by phone number and code, can be searched only by phone number too
        /// </summary>
        /// <param name="currentPhoneNumber">customers current phone number</param>
        /// <param name="code">entered verification code</param>
        /// <returns>List<SmsVerification></returns>
        IList<SmsVerification> GetSmsVerificationsByPhoneNumberAndCode(string currentPhoneNumber, string code = null);

        /// <summary>
        /// Gets if customer is verified
        /// </summary>
        /// <param name="customerEmail">customer's email</param>
        /// <param name="currentPhoneNumber">customers current phone number</param>
        /// <returns>bool</returns>
        bool GetIsCustomerVerified(string customerEmail, string currentPhoneNumber);

        IList<SmsVerification> GetSmsVerifications();

        IQueryable<SmsVerification> GetSmsVerificationsQuery();

        /// <summary>
        /// Deletes a smsVerification
        /// </summary>
        /// <param name="smsVerification">The smsVerification</param>
        void DeleteSmsVerification(SmsVerification smsVerification);

        /// <summary>
        /// Inserts a smsVerification
        /// </summary>
        /// <param name="smsVerification">SmsVerification</param>
        void InsertSmsVerification(SmsVerification smsVerification);

        /// <summary>
        /// Updates the smsVerification
        /// </summary>
        /// <param name="smsVerification">The smsVerification</param>
        void UpdateSmsVerification(SmsVerification smsVerification);
    }
}
