using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teed.Plugin.Manager.Domain.CashExpenses;

namespace Teed.Plugin.Manager.Models.CashExpenses
{
    public class CreateViewModel
    {
        public CreateViewModel()
        {
            Files = new List<IFormFile>();
        }

        public string SelectedDate { get; set; }

        public TransactionType TransactionType { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Concept { get; set; }
        public string Comments { get; set; }
        public int ReceptorUserId { get; set; }
        public IList<IFormFile> Files { get; set; }
    }
}
