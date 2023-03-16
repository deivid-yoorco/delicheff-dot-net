using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain.Appointment;

namespace Teed.Plugin.Medical.Services
{
    public class AppointmentItemService
    {
        private readonly IRepository<AppointmentItem> _appointmentItemRepository;
        private readonly IProductService _productService;
        private readonly IEventPublisher _eventPublisher;

        public AppointmentItemService(
            IRepository<AppointmentItem> appointmentItemRepository,
            IEventPublisher eventPublisher,
            IProductService productService)
        {
            _appointmentItemRepository = appointmentItemRepository;
            _eventPublisher = eventPublisher;
            _productService = productService;
        }

        public void Insert(AppointmentItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.GuidId = Guid.NewGuid();
            entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;

            _appointmentItemRepository.Insert(entity);
            _eventPublisher.EntityInserted(entity);
        }

        public List<Product> GetProductsByAppointmentId(int id)
        {
            var productIds = _appointmentItemRepository.TableNoTracking.Where(x => x.AppointmentId == id && !x.Deleted).Select(x => x.ProductId);
            return _productService.GetProductsByIds(productIds.ToArray()).ToList();
        }

        public IQueryable<int> GetProductsIdsByAppointmentId(int id)
        {
            return _appointmentItemRepository.TableNoTracking.Where(x => x.AppointmentId == id && !x.Deleted).Select(x => x.ProductId);
        }

        public IPagedList<AppointmentItem> ListAsNoTracking(int pageIndex = 0, int pageSize = 20)
        {
            var query = _appointmentItemRepository.TableNoTracking;
            query = query.OrderByDescending(m => m.CreatedOnUtc).ThenBy(m => m.Id).Where(x => !x.Deleted);
            return new PagedList<AppointmentItem>(query, pageIndex, pageSize);
        }

        public IQueryable<AppointmentItem> GetByProductAndAppointmentId(int productId, int appointmentId)
        {
            return _appointmentItemRepository.Table.Where(x => x.ProductId == productId && x.AppointmentId == appointmentId && !x.Deleted);
        }

        public IQueryable<AppointmentItem> GetByAppointmentId(int id)
        {
            return _appointmentItemRepository.Table.Where(x => x.AppointmentId == id && !x.Deleted);
        }

        public void Delete(AppointmentItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DateTime now = DateTime.UtcNow;
            entity.UpdatedOnUtc = now;
            entity.Deleted = true;

            _appointmentItemRepository.Update(entity);
            _eventPublisher.EntityUpdated(entity);
        }
    }
}