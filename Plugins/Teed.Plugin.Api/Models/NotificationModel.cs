using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Domain.Notifications;

namespace Teed.Plugin.Api.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Body { get; set; }

        public string Log { get; set; }
        public DateTime DontSendBeforeDate { get; set; }
        public bool IsCancelled { get; set; }
        public bool SendImmediately { get; set; }
        public IList<int> CustomersIds { get; set; }
        public bool BlockChanges { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int CustomersSent { get; set; }
        public int ErrorsCount { get; set; }
        public int CustomersToSend { get; set; }

        public List<SelectListItem> ActionTypes { get; set; }
        public int ActionTypeId { get; set; }
        public string AdditionalData { get; set; }
    }

    public class NotificationData
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
    }

    public class QueuedNotificationListModel
    {
        public DateTime? SentAtUtc { get; set; }
        public int NotificationId { get; set; }
    }
}
