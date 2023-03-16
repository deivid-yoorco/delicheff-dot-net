using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Models.Note
{
    public class NotesListModel
    {
        public int Id { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
        public string Text { get; set; }
        public Patient Patient { get; set; }
    }
}
