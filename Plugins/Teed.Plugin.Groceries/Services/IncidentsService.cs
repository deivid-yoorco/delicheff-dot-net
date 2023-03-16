using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.Franchise;
using Teed.Plugin.Groceries.Domain.OrderReports;

namespace Teed.Plugin.Groceries.Services
{
    public class IncidentsService
    {
        private readonly IRepository<Incidents> _db;
        private readonly IEventPublisher _eventPublisher;
        private readonly FranchiseBonusService _franchiseBonusService;

        public IncidentsService(IRepository<Incidents> db,
            IEventPublisher eventPublisher, FranchiseBonusService franchiseBonusService)
        {
            _db = db;
            _eventPublisher = eventPublisher;
            _franchiseBonusService = franchiseBonusService;
        }

        public IQueryable<Incidents> GetAll(bool deleted = false)
        {
            if (deleted)
                return _db.Table;
            else
                return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(Incidents entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);

            var inTimeBonus = new List<FranchiseBonus>();
            if (entity.IncidentCode == "R04" || entity.IncidentCode == "R05")
            {
                inTimeBonus = _franchiseBonusService.GetAll()
                                .Where(x => x.VehicleId == entity.VehicleId && x.Date == entity.Date && x.BonusCode == "B02")
                                .ToList();
            }

            var noIncidentBonus = _franchiseBonusService.GetAll()
                                .Where(x => x.VehicleId == entity.VehicleId && x.Date == entity.Date && x.BonusCode == "B01")
                                .ToList();

            foreach (var bonus in noIncidentBonus)
            {
                bonus.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - El bono fue eliminado ya que se registró una incidencia (ID: {entity.Id}) para ese día de entrega. \n";
                _franchiseBonusService.Delete(bonus);
            }

            foreach (var bonus in inTimeBonus)
            {
                bonus.Log += DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt") + $" - El bono fue eliminado ya que se registró una incidencia por entrega tarde (ID: {entity.Id}) para ese día de entrega. \n";
                _franchiseBonusService.Delete(bonus);
            }
        }

        public void Update(Incidents entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(Incidents entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Deleted = true;
            Update(entity);

            //event notification
            _eventPublisher.EntityDeleted(entity);
        }
    }
}