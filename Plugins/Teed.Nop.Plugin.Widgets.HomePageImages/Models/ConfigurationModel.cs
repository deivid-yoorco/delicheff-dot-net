using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Teed.Nop.Plugin.Widgets.NivoSlider2.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture1Id { get; set; }
        public bool BannerPicture1Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string BannerText1 { get; set; }
        public bool BannerText1_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string BannerLink1 { get; set; }
        public bool BannerLink1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture2Id { get; set; }
        public bool BannerPicture2Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string BannerText2 { get; set; }
        public bool BannerText2_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string BannerLink2 { get; set; }
        public bool BannerLink2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture3Id { get; set; }
        public bool BannerPicture3Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string BannerText3 { get; set; }
        public bool BannerText3_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string BannerLink3 { get; set; }
        public bool BannerLink3_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture4Id { get; set; }
        public bool BannerPicture4Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string BannerText4 { get; set; }
        public bool BannerText4_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string BannerLink4 { get; set; }
        public bool BannerLink4_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int NewBannerPicture5Id { get; set; }
        public bool NewBannerPicture5Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string NewBannerText5 { get; set; }
        public bool NewBannerText5_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string NewBannerLink5 { get; set; }
        public bool NewBannerLink5_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int NewBannerPicture6Id { get; set; }
        public bool NewBannerPicture6Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string NewBannerText6 { get; set; }
        public bool NewBannerText6_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string NewBannerLink6 { get; set; }
        public bool NewBannerLink6_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPictureArrowId { get; set; }
        public bool BannerPictureArrowId_OverrideForStore { get; set; }


        public string VideoId{ get; set; }
        public bool VideoId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture5Id { get; set; }
        public bool BannerPicture5Id_OverrideForStore { get; set; }
        public string BannerTitle5 { get; set; }
        public bool BannerTitle5_OverrideForStore { get; set; }
        public string BannerSubTitle5 { get; set; }
        public bool BannerSubTitle5_OverrideForStore { get; set; }
        public string BannerLink5 { get; set; }
        public bool BannerLink5_OverrideForStore { get; set; }
        public string BannerTitleColor5 { get; set; }
        public bool BannerTitleColor5_OverrideForStore { get; set; }
        public string BannerSubTitleColor5 { get; set; }
        public bool BannerSubTitleColor5_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int BannerPicture6Id { get; set; }
        public bool BannerPicture6Id_OverrideForStore { get; set; }
        public string BannerTitle6 { get; set; }
        public bool BannerTitle6_OverrideForStore { get; set; }
        public string BannerSubTitle6 { get; set; }
        public bool BannerSubTitle6_OverrideForStore { get; set; }
        public string BannerLink6 { get; set; }
        public bool BannerLink6_OverrideForStore { get; set; }
        public string BannerTitleColor6 { get; set; }
        public bool BannerTitleColor6_OverrideForStore { get; set; }
        public string BannerSubTitleColor6 { get; set; }
        public bool BannerSubTitleColor6_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture1Id { get; set; }
        public bool Picture1Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string Text1 { get; set; }
        public bool Text1_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string Link1 { get; set; }
        public bool Link1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture2Id { get; set; }
        public bool Picture2Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string Text2 { get; set; }
        public bool Text2_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string Link2 { get; set; }
        public bool Link2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture3Id { get; set; }
        public bool Picture3Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string Text3 { get; set; }
        public bool Text3_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string Link3 { get; set; }
        public bool Link3_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture4Id { get; set; }
        public bool Picture4Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string Text4 { get; set; }
        public bool Text4_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string Link4 { get; set; }
        public bool Link4_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture5Id { get; set; }
        public bool Picture5Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        public string Text5 { get; set; }
        public bool Text5_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        public string Link5 { get; set; }
        public bool Link5_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture6Id { get; set; }
        public bool Picture6Id_OverrideForStore { get; set; }
        public string Text6 { get; set; }
        public bool Text6_OverrideForStore { get; set; }
        public string Link6 { get; set; }
        public bool Link6_OverrideForStore { get; set; }
        public string TextColor6 { get; set; }
        public bool TextColor6_OverrideForStore { get; set; }

        public string TextDropdown1 { get; set; }
        public bool TextDropdown1_OverrideForStore { get; set; }
        public string LinkDropdown1 { get; set; }
        public bool LinkDropdown1_OverrideForStore { get; set; }
        public string TextDropdown2 { get; set; }
        public bool TextDropdown2_OverrideForStore { get; set; }
        public string LinkDropdown2 { get; set; }
        public bool LinkDropdown2_OverrideForStore { get; set; }
        public string TextDropdown3 { get; set; }
        public bool TextDropdown3_OverrideForStore { get; set; }
        public string LinkDropdown3 { get; set; }
        public bool LinkDropdown3_OverrideForStore { get; set; }
        public string TextDropdown4 { get; set; }
        public bool TextDropdown4_OverrideForStore { get; set; }
        public string LinkDropdown4 { get; set; }
        public bool LinkDropdown4_OverrideForStore { get; set; }
        public string TextDropdown5 { get; set; }
        public bool TextDropdown5_OverrideForStore { get; set; }
        public string LinkDropdown5 { get; set; }
        public bool LinkDropdown5_OverrideForStore { get; set; }
        public string TextDropdown6 { get; set; }
        public bool TextDropdown6_OverrideForStore { get; set; }
        public string LinkDropdown6 { get; set; }
        public bool LinkDropdown6_OverrideForStore { get; set; }
        public string TextDropdown7 { get; set; }
        public bool TextDropdown7_OverrideForStore { get; set; }
        public string LinkDropdown7 { get; set; }
        public bool LinkDropdown7_OverrideForStore { get; set; }
        public string TextDropdown8 { get; set; }
        public bool TextDropdown8_OverrideForStore { get; set; }
        public string LinkDropdown8 { get; set; }
        public bool LinkDropdown8_OverrideForStore { get; set; }
        public string TextDropdown9 { get; set; }
        public bool TextDropdown9_OverrideForStore { get; set; }
        public string LinkDropdown9 { get; set; }
        public bool LinkDropdown9_OverrideForStore { get; set; }
        public string TextDropdown10 { get; set; }
        public bool TextDropdown10_OverrideForStore { get; set; }
        public string LinkDropdown10 { get; set; }
        public bool LinkDropdown10_OverrideForStore { get; set; }

        public string TextCarousel { get; set; }
        public bool TextCarousel_OverrideForStore { get; set; }
        public string TextColorCarousel { get; set; }
        public bool TextColorCarousel_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture7Id { get; set; }
        public bool Picture7Id_OverrideForStore { get; set; }
        public string Link7 { get; set; }
        public bool Link7_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture8Id { get; set; }
        public bool Picture8Id_OverrideForStore { get; set; }
        public string Link8 { get; set; }
        public bool Link8_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture9Id { get; set; }
        public bool Picture9Id_OverrideForStore { get; set; }
        public string Link9 { get; set; }
        public bool Link9_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture10Id { get; set; }
        public bool Picture10Id_OverrideForStore { get; set; }
        public string Link10 { get; set; }
        public bool Link10_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture11Id { get; set; }
        public bool Picture11Id_OverrideForStore { get; set; }
        public string Link11 { get; set; }
        public bool Link11_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture12Id { get; set; }
        public bool Picture12Id_OverrideForStore { get; set; }
        public string Link12 { get; set; }
        public bool Link12_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture13Id { get; set; }
        public bool Picture13Id_OverrideForStore { get; set; }
        public string Link13 { get; set; }
        public bool Link13_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture14Id { get; set; }
        public bool Picture14Id_OverrideForStore { get; set; }
        public string Link14 { get; set; }
        public bool Link14_OverrideForStore { get; set; }

        public int TagsQty { get; set; }
        public bool TagsQty_OverrideForStore { get; set; }
        public bool TagsEnable { get; set; }
        public bool TagsEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture15Id { get; set; }
        public bool Picture15Id_OverrideForStore { get; set; }
        public string Link15 { get; set; }
        public bool Link15_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture16Id { get; set; }
        public bool Picture16Id_OverrideForStore { get; set; }
        public string Link16 { get; set; }
        public bool Link16_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture17Id { get; set; }
        public bool Picture17Id_OverrideForStore { get; set; }
        public string Link17 { get; set; }
        public bool Link17_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture18Id { get; set; }
        public bool Picture18Id_OverrideForStore { get; set; }
        public string Link18 { get; set; }
        public bool Link18_OverrideForStore { get; set; }

        public bool ManufacturerEnable { get; set; }
        public bool ManufacturerEnable_OverrideForStore { get; set; }
        public string TitleManufacturer { get; set; }
        public bool TitleManufacturer_OverrideForStore { get; set; }

        public bool CollageEnable { get; set; }
        public bool CollageEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture19Id { get; set; }
        public bool Picture19Id_OverrideForStore { get; set; }
        public string Link19 { get; set; }
        public bool Link19_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture20Id { get; set; }
        public bool Picture20Id_OverrideForStore { get; set; }
        public string Link20 { get; set; }
        public bool Link20_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture21Id { get; set; }
        public bool Picture21Id_OverrideForStore { get; set; }
        public string Link21 { get; set; }
        public bool Link21_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture22Id { get; set; }
        public bool Picture22Id_OverrideForStore { get; set; }
        public string Link22 { get; set; }
        public bool Link22_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture23Id { get; set; }
        public bool Picture23Id_OverrideForStore { get; set; }
        public string Link23 { get; set; }
        public bool Link23_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture24Id { get; set; }
        public bool Picture24Id_OverrideForStore { get; set; }
        public string Link24 { get; set; }
        public bool Link24_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture25Id { get; set; }
        public bool Picture25Id_OverrideForStore { get; set; }
        public string Link25 { get; set; }
        public bool Link25_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture26Id { get; set; }
        public bool Picture26Id_OverrideForStore { get; set; }
        public string Link26 { get; set; }
        public bool Link26_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture27Id { get; set; }
        public bool Picture27Id_OverrideForStore { get; set; }
        public string Link27 { get; set; }
        public bool Link27_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture28Id { get; set; }
        public bool Picture28Id_OverrideForStore { get; set; }
        public string Link28 { get; set; }
        public bool Link28_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture29Id { get; set; }
        public bool Picture29Id_OverrideForStore { get; set; }
        public string Link29 { get; set; }
        public bool Link29_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture30Id { get; set; }
        public bool Picture30Id_OverrideForStore { get; set; }
        public string Link30 { get; set; }
        public bool Link30_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture31Id { get; set; }
        public bool Picture31Id_OverrideForStore { get; set; }
        public string Link31 { get; set; }
        public bool Link31_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture32Id { get; set; }
        public bool Picture32Id_OverrideForStore { get; set; }
        public string Link32 { get; set; }
        public bool Link32_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture33Id { get; set; }
        public bool Picture33Id_OverrideForStore { get; set; }
        public string Link33 { get; set; }
        public bool Link33_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture34Id { get; set; }
        public bool Picture34Id_OverrideForStore { get; set; }
        public string Link34 { get; set; }
        public bool Link34_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture35Id { get; set; }
        public bool Picture35Id_OverrideForStore { get; set; }
        public string Link35 { get; set; }
        public bool Link35_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture36Id { get; set; }
        public bool Picture36Id_OverrideForStore { get; set; }
        public string Link36 { get; set; }
        public bool Link36_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture37Id { get; set; }
        public bool Picture37Id_OverrideForStore { get; set; }
        public string Link37 { get; set; }
        public bool Link37_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture38Id { get; set; }
        public bool Picture38Id_OverrideForStore { get; set; }
        public string Link38 { get; set; }
        public bool Link38_OverrideForStore { get; set; }

        public bool PopUpEnable { get; set; }
        public bool PopUpEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture39Id { get; set; }
        public bool Picture39Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture39ResponsiveId { get; set; }
        public bool Picture39ResponsiveId_OverrideForStore { get; set; }

        public bool VendorEnable { get; set; }
        public bool VendorEnable_OverrideForStore { get; set; }
        public string TitleVendor { get; set; }
        public bool TitleVendor_OverrideForStore { get; set; }
    }
}