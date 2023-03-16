using Nop.Core.Domain.Media;

namespace Nop.Data.Mapping.Media
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class PictureMap : NopEntityTypeConfiguration<Picture>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public PictureMap()
        {
            this.ToTable("Picture");
            this.HasKey(p => p.Id);
            this.Property(p => p.PictureBinary).IsMaxLength();
            this.Property(p => p.MimeType).IsRequired().HasMaxLength(40);
            this.Property(p => p.SeoFilename).HasMaxLength(300);
            this.Property(p => p.Is360);
            this.Property(p => p.CustomEnable);
            this.Property(p => p.BoundingX);
            this.Property(p => p.BoundingY);
            this.Property(p => p.BoundingWidth);
            this.Property(p => p.BoundingHeight);
        }
    }
}