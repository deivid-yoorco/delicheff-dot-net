using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Groceries.Models.PrintedCouponBook
{
    public class PrintedCouponBookModel
    {
        public PrintedCouponBookModel()
        {
            AvailableZipCodes = new List<SelectListItem>();
            AvailableCustomers = new List<SelectListItem>();
            SelectedZipCodes = new List<int>();
            SelectedCustomerIds = new List<int>();
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        [UIHint("Picture")]
        public virtual int ReferencePictureId { get; set; }
        public virtual int Inventory { get; set; }
        public virtual int InventoryAvailable { get; set; }
        public virtual bool Active { get; set; }
        public virtual int BookTypeId { get; set; }

        public IList<SelectListItem> AvailableZipCodes { get; set; }
        public IList<SelectListItem> AvailableCustomers { get; set; }
        public IList<int> SelectedZipCodes { get; set; }
        public IList<int> SelectedCustomerIds { get; set; }
        public virtual decimal Subtotal { get; set; }
        public virtual string BookTypeValue { get; set; }

        [UIHint("DateTimeNullable")]
        public virtual DateTime StartDate { get; set; }

        [UIHint("DateTimeNullable")]
        public virtual DateTime EndDate { get; set; }
        public virtual int ConnectedProductId { get; set; }
        public virtual string Log { get; set; }
    }
}
