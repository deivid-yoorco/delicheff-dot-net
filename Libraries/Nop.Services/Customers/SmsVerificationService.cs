using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Sms verification service
    /// </summary>
    public partial class SmsVerificationService : ISmsVerificationService
    {
        #region Fields

        private readonly IRepository<SmsVerification> _smsVerificationRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="smsVerificationRepository">SmsVerification repository</param>
        /// <param name="eventPublisher">Event publisher service</param>
        public SmsVerificationService(IRepository<SmsVerification> smsVerificationRepository,
            IEventPublisher eventPublisher)
        {
            this._smsVerificationRepository = smsVerificationRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a smsVerification
        /// </summary>
        /// <param name="smsVerificationId">The smsVerification identifier</param>
        /// <returns>SmsVerification</returns>
        public virtual SmsVerification GetSmsVerificationById(int smsVerificationId)
        {
            if (smsVerificationId == 0)
                return null;

            return _smsVerificationRepository.GetById(smsVerificationId);
        }

        /// <summary>
        /// Gets a verified smsVerification by customer email
        /// </summary>
        /// <param name="customerEmail">customer email</param>
        /// <returns>SmsVerification</returns>
        public virtual SmsVerification GetSmsVerificationByCustomerEmail(string customerEmail)
        {
            if (string.IsNullOrEmpty(customerEmail))
                return null;

            return _smsVerificationRepository.Table.Where(x => !x.Deleted && x.CustomerEmail == customerEmail && x.IsVerified).FirstOrDefault();
        }

        /// <summary>
        /// Get smsVerifications by customer identifier
        /// </summary>
        /// <param name="customerId">customer identifiers</param>
        /// <returns>List<SmsVerification></returns>
        public virtual IList<SmsVerification> GetSmsVerificationsByCustomerId(int customerId)
        {
            if (customerId == 0)
                return null;

            return _smsVerificationRepository.Table.Where(x =>!x.Deleted && x.CustomerId == customerId).ToList();
        }

        /// <summary>
        /// Gets smsVerifications by phone number and code, can be searched only by phone number too
        /// </summary>
        /// <param name="currentPhoneNumber">customers current phone number</param>
        /// <param name="code">entered verification code</param>
        /// <returns>List<SmsVerification></returns>
        public virtual IList<SmsVerification> GetSmsVerificationsByPhoneNumberAndCode(string currentPhoneNumber, string code = null)
        {
            if (string.IsNullOrEmpty(currentPhoneNumber))
                return null;

            if (!string.IsNullOrEmpty(code))
                return _smsVerificationRepository.Table.Where(x => !x.Deleted && x.PhoneNumber == currentPhoneNumber.Trim() && x.VerificationCode == code.Trim()).ToList();
            else
                return _smsVerificationRepository.Table.Where(x => !x.Deleted && x.PhoneNumber == currentPhoneNumber.Trim()).ToList();
        }

        /// <summary>
        /// Gets if customer is verified
        /// </summary>
        /// <param name="customerEmail">customer's email</param>
        /// <param name="currentPhoneNumber">customers current phone number</param>
        /// <returns>bool</returns>
        public virtual bool GetIsCustomerVerified(string customerEmail, string currentPhoneNumber)
        {
            if (!string.IsNullOrEmpty(customerEmail) || !string.IsNullOrEmpty(currentPhoneNumber))
                return false;

            return _smsVerificationRepository.Table.Where(x =>!x.Deleted &&
            x.CustomerEmail == customerEmail.Trim() && x.PhoneNumber == currentPhoneNumber.Trim() &&
            x.IsVerified).Any();
        }

        public virtual IList<SmsVerification> GetSmsVerifications()
        {
            return _smsVerificationRepository.Table.Where(x =>!x.Deleted).ToList();
        }

        public virtual IQueryable<SmsVerification> GetSmsVerificationsQuery()
        {
            return _smsVerificationRepository.Table;
        }

        /// <summary>
        /// Deletes a smsVerification
        /// </summary>
        /// <param name="smsVerification">The smsVerification</param>
        public virtual void DeleteSmsVerification(SmsVerification smsVerification)
        {
            if (smsVerification == null)
                throw new ArgumentNullException(nameof(smsVerification));

            smsVerification.Deleted = true;
            UpdateSmsVerification(smsVerification);

            //event notification
            _eventPublisher.EntityDeleted(smsVerification);
        }

        /// <summary>
        /// Inserts a smsVerification
        /// </summary>
        /// <param name="smsVerification">SmsVerification</param>
        public virtual void InsertSmsVerification(SmsVerification smsVerification)
        {
            if (smsVerification == null)
                throw new ArgumentNullException(nameof(smsVerification));
            smsVerification.CreatedOnUtc = DateTime.UtcNow;
            smsVerification.UpdatedOnUtc = DateTime.UtcNow;

            _smsVerificationRepository.Insert(smsVerification);

            //event notification
            _eventPublisher.EntityInserted(smsVerification);
        }

        /// <summary>
        /// Updates the smsVerification
        /// </summary>
        /// <param name="smsVerification">The smsVerification</param>
        public virtual void UpdateSmsVerification(SmsVerification smsVerification)
        {
            if (smsVerification == null)
                throw new ArgumentNullException(nameof(smsVerification));
            smsVerification.UpdatedOnUtc = DateTime.UtcNow;

            _smsVerificationRepository.Update(smsVerification);

            //event notification
            _eventPublisher.EntityUpdated(smsVerification);
        }
        
        #endregion
    }
}
