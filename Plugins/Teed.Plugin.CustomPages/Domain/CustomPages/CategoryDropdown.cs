using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class CategoryDropdown : BaseEntity
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }
        
        public virtual int CustomPageId { get; set; }

        [ForeignKey("CustomPageId")]
        public virtual CustomPage CustomPage { get; set; }

        public int CatPicture6Id { get; set; }
        public string CatText6 { get; set; }
        public string CatLink6 { get; set; }
        public string CatTextColor6 { get; set; }

        public string CatTextDropdown1 { get; set; }
        public string CatLinkDropdown1 { get; set; }
        public string CatTextDropdown2 { get; set; }
        public string CatLinkDropdown2 { get; set; }
        public string CatTextDropdown3 { get; set; }
        public string CatLinkDropdown3 { get; set; }
        public string CatTextDropdown4 { get; set; }
        public string CatLinkDropdown4 { get; set; }
        public string CatTextDropdown5 { get; set; }
        public string CatLinkDropdown5 { get; set; }
        public string CatTextDropdown6 { get; set; }
        public string CatLinkDropdown6 { get; set; }
        public string CatTextDropdown7 { get; set; }
        public string CatLinkDropdown7 { get; set; }
        public string CatTextDropdown8 { get; set; }
        public string CatLinkDropdown8 { get; set; }
        public string CatTextDropdown9 { get; set; }
        public string CatLinkDropdown9 { get; set; }
        public string CatTextDropdown10 { get; set; }
        public string CatLinkDropdown10 { get; set; }
    }
}
