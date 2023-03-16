using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Note
{
    public class NotesDetailsModel: BaseNopModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string Text { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
    }
}
