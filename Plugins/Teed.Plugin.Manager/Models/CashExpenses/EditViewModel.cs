using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teed.Plugin.Manager.Domain.CashExpenses;

namespace Teed.Plugin.Manager.Models.CashExpenses
{
    public class EditViewModel
    {
        public EditViewModel()
        {
            Files = new List<IFormFile>();
        }

        public int Id { get; set; }

        public TransactionType TransactionType { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Concept { get; set; }

        public string Comments { get; set; }
        public string SelectedDate { get; set; }
        public int ReceptorUserId { get; set; }
        public List<CashExpenseFile> UploadedFiles { get; set; }
        public IList<IFormFile> Files { get; set; }
    }
}
