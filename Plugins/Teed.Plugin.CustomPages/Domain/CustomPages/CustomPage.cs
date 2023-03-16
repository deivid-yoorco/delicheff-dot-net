using Nop.Core;
using Nop.Core.Domain.Seo;
using System;

namespace Teed.Plugin.CustomPages.Domain.CustomPages
{
    public class CustomPage : BaseEntity, ISlugSupported
    {
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public virtual bool Deleted { get; set; }

        public virtual string Name { get; set; }
        public virtual string ProductSectionTitle { get; set; }
        public virtual string PrimaryColor { get; set; }
        public virtual bool Published { get; set; }
        public virtual bool HideNavBar { get; set; }
        public virtual string Log { get; set; }
        public virtual string TabColor { get; set; }
        public virtual int DisplayOrder { get; set; }
    }
}