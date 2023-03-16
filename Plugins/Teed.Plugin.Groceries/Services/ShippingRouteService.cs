using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Groceries.Domain.OrderReports;
using Teed.Plugin.Groceries.Domain.ShippingRoutes;

namespace Teed.Plugin.Groceries.Services
{
    public class ShippingRouteService
    {
        private readonly IRepository<ShippingRoute> _db;
        private readonly IEventPublisher _eventPublisher;

        public ShippingRouteService(
            IRepository<ShippingRoute> db,
            IEventPublisher eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<ShippingRoute> GetAll()
        {
            return _db.Table.Where(x => !x.Deleted);
        }

        public void Insert(ShippingRoute entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _db.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(ShippingRoute entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public void Delete(ShippingRoute entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _db.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }

        public string GetDeliveryTimeColor(DateTime deliveryDate, DateTime selectedShippingDate, string selectedShippingTime)
        {
            var deliveryStatus = 0;
            if (deliveryDate != DateTime.MinValue && selectedShippingDate != DateTime.MinValue)
            {
                // Create times for comparing
                var dateCompare1 = DateTime.MinValue;
                var dateCompare2 = DateTime.MinValue;
                if (selectedShippingTime == "1:00 PM - 3:00 PM")
                {
                    dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 13, 0, 0);
                    dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 15, 0, 0);
                }
                else if (selectedShippingTime == "3:00 PM - 5:00 PM")
                {
                    dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 15, 0, 0);
                    dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 17, 0, 0);
                }
                else if (selectedShippingTime == "5:00 PM - 7:00 PM")
                {
                    dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 17, 0, 0);
                    dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 19, 0, 0);
                }
                else if (selectedShippingTime == "7:00 PM - 9:00 PM")
                {
                    dateCompare1 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 19, 0, 0);
                    dateCompare2 = new DateTime(selectedShippingDate.Year, selectedShippingDate.Month, selectedShippingDate.Day, 21, 0, 0);
                }
                var dateCompare16Earlier = dateCompare1.AddMinutes(-16);
                var dateCompare15After = dateCompare2.AddMinutes(15);
                var dateCompare30Late = dateCompare2.AddMinutes(30);
                // Check type of deliveryStatus
                if (deliveryDate <= dateCompare16Earlier)
                {
                    deliveryStatus = 1;
                }
                else if (dateCompare16Earlier <= deliveryDate && deliveryDate <= dateCompare15After)
                {
                    deliveryStatus = 2;
                }
                else if (dateCompare15After <= deliveryDate && deliveryDate <= dateCompare30Late)
                {
                    deliveryStatus = 3;
                }
                else if (deliveryDate > dateCompare30Late)
                {
                    deliveryStatus = 4;
                }
            }
            
            return (deliveryStatus == 1 ? "blue" : deliveryStatus == 2 ? "green" : deliveryStatus == 3 ? "yellow" : deliveryStatus == 4 ? "orange" : "grey");
        }
    }
}