using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.PenaltiesCatalog;

namespace Teed.Plugin.Groceries.Services
{
    public class FranchiseBonusService
    {
        private readonly IRepository<FranchiseBonus> _db;
        private readonly IEventPublisher _eventPublisher;

        public FranchiseBonusService(
            IRepository<FranchiseBonus> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<FranchiseBonus> GetAll(bool deleted = false)
        {
            if (deleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(FranchiseBonus entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(FranchiseBonus entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(FranchiseBonus entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }

        public void RecalculateBonus(List<PenaltiesCatalog> penaltiesCatalogs, DateTime date)
        {
            var bonuses = GetAll().Where(x => x.BonusCode == "B02" || x.BonusCode == "B03").ToList();
            foreach (var bonus in bonuses)
            {
                var newAmount = GetBonusAmount(bonus.BonusCode, null, 0, penaltiesCatalogs, date);
                if (newAmount != bonus.Amount && newAmount > 0)
                {
                    bonus.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + " - " + $"Se actualizó de forma autmática el monto del bono de {bonus.Amount} a {newAmount}.\n";
                    bonus.Amount = newAmount;
                    Update(bonus);
                }
            }
        }

        /// <summary>
        /// Get the bonus amount
        /// </summary>
        /// <param name="bonusCode"></param>
        /// <param name="settings"></param>
        /// <param name="orderTotalAmount">Required for B01</param>
        /// <param name="penaltiesCatalogs">Required for bonus B02 and B03</param>
        /// <param name="date">Selected shipping date. Required for bonus B02 and B03</param>
        /// <returns></returns>
        public decimal GetBonusAmount(string bonusCode, 
            RatesAndBonusesSettings settings, 
            decimal orderTotalAmount = 0, 
            List<PenaltiesCatalog> penaltiesCatalogs = null, 
            DateTime? date = null)
        {
            switch (bonusCode)
            {
                case "B01":
                    return orderTotalAmount * settings.VariableWeeklyBonusCeroIncidents;
                case "B02":
                    if (penaltiesCatalogs == null || !date.HasValue) return 0;
                    var amounti01 = penaltiesCatalogs.Where(x => x.PenaltyCustomId == "I01")
                        .Where(x => date >= x.ApplyDate)
                        .OrderByDescending(x => x.ApplyDate).Select(x => x.Amount)
                        .FirstOrDefault();
                    return amounti01 / 3;
                case "B03":
                    if (penaltiesCatalogs == null || !date.HasValue) return 0;
                    var amounti04 = penaltiesCatalogs.Where(x => x.PenaltyCustomId == "I04")
                        .Where(x => date >= x.ApplyDate)
                        .OrderByDescending(x => x.ApplyDate).Select(x => x.Amount)
                        .FirstOrDefault();
                    return amounti04 / 4;
                default:
                    return 0;
            }
        }
    }
}