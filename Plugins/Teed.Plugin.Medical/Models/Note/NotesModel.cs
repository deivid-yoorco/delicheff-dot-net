using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.Note
{
    public class NotesModel : BaseNopModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Text { get; set; }
    }
}
