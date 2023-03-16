using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Medical.Models.File
{
    public class FilesListModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }
        public string UploadedDate { get; set; }
    }
}
