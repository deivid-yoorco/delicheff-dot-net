using Nop.Web.Framework.Mvc.Models;using Nop.Web.Models.Catalog;
using System;using System.Collections.Generic;using System.Linq;using System.Threading.Tasks;namespace Nop.Web.Models.CustomPages{    public class CustomPagesModel : BaseNopEntityModel    {        public string Name { get; set; }        public string PrimaryColor { get; set; }        public bool Published { get; set; }

        public bool HideNavBar { get; set; }

        public List<dynamic> ActiveCustomPages { get; set; }

        // SLIDER
        public string SliderVideoId { get; set; }        public int SliderBannerPicture1Id { get; set; }        public string SliderBannerText1 { get; set; }        public string SliderBannerLink1 { get; set; }
        public int SliderBannerPicture2Id { get; set; }
        public string SliderBannerText2 { get; set; }
        public string SliderBannerLink2 { get; set; }
        public int SliderBannerPicture3Id { get; set; }
        public string SliderBannerText3 { get; set; }
        public string SliderBannerLink3 { get; set; }
        public int SliderBannerPicture4Id { get; set; }
        public string SliderBannerText4 { get; set; }
        public string SliderBannerLink4 { get; set; }
        public int SliderArrowId { get; set; }

        // PARALLAX
        public int ParallaxPicture1Id { get; set; }
        public string ParallaxText1 { get; set; }
        public string ParallaxLink1 { get; set; }
        public int ParallaxPicture2Id { get; set; }
        public string ParallaxText2 { get; set; }
        public string ParallaxLink2 { get; set; }

        // TOP THREE


        public int Top3Picture3Id { get; set; }

        public string Top3Text3 { get; set; }

        public string Top3Link3 { get; set; }



        public int Top3Picture4Id { get; set; }

        public string Top3Text4 { get; set; }

        public string Top3Link4 { get; set; }



        public int Top3Picture5Id { get; set; }

        public string Top3Text5 { get; set; }

        public string Top3Link5 { get; set; }

        // CATEGORY DROPDOWN


        public int CatPicture6Id { get; set; }        public string CatText6 { get; set; }        public string CatLink6 { get; set; }        public string CatTextColor6 { get; set; }        public string CatTextDropdown1 { get; set; }        public string CatLinkDropdown1 { get; set; }        public string CatTextDropdown2 { get; set; }        public string CatLinkDropdown2 { get; set; }        public string CatTextDropdown3 { get; set; }        public string CatLinkDropdown3 { get; set; }        public string CatTextDropdown4 { get; set; }        public string CatLinkDropdown4 { get; set; }        public string CatTextDropdown5 { get; set; }        public string CatLinkDropdown5 { get; set; }        public string CatTextDropdown6 { get; set; }        public string CatLinkDropdown6 { get; set; }        public string CatTextDropdown7 { get; set; }        public string CatLinkDropdown7 { get; set; }        public string CatTextDropdown8 { get; set; }        public string CatLinkDropdown8 { get; set; }        public string CatTextDropdown9 { get; set; }        public string CatLinkDropdown9 { get; set; }        public string CatTextDropdown10 { get; set; }        public string CatLinkDropdown10 { get; set; }

        // BANNERS


        public int BannerPicture5Id { get; set; }        public string BannerTitle5 { get; set; }        public string BannerSubTitle5 { get; set; }        public string BannerLink5 { get; set; }        public string BannerTitleColor5 { get; set; }        public string BannerSubTitleColor5 { get; set; }



        public int BannerPicture6Id { get; set; }        public string BannerTitle6 { get; set; }        public string BannerSubTitle6 { get; set; }        public string BannerLink6 { get; set; }        public string BannerTitleColor6 { get; set; }        public string BannerSubTitleColor6 { get; set; }

        // CAROUSEL
        public string CarouselText { get; set; }        public string CarouselTextColor { get; set; }



        public int CarouselPicture7Id { get; set; }        public string CarouselLink7 { get; set; }



        public int CarouselPicture8Id { get; set; }        public string CarouselLink8 { get; set; }



        public int CarouselPicture9Id { get; set; }        public string CarouselLink9 { get; set; }



        public int CarouselPicture10Id { get; set; }        public string CarouselLink10 { get; set; }



        public int CarouselPicture11Id { get; set; }        public string CarouselLink11 { get; set; }



        public int CarouselPicture12Id { get; set; }        public string CarouselLink12 { get; set; }



        public int CarouselPicture13Id { get; set; }        public string CarouselLink13 { get; set; }



        public int CarouselPicture14Id { get; set; }        public string CarouselLink14 { get; set; }

        public int CarouselArrowId { get; set; }

        // TAGS
        public int TagsQty { get; set; }        public bool TagsEnable { get; set; }

        // BOXES
        
        public int BoxPicture15Id { get; set; }        public string BoxLink15 { get; set; }
        
        public int BoxPicture16Id { get; set; }        public string BoxLink16 { get; set; }
        
        public int BoxPicture17Id { get; set; }        public string BoxLink17 { get; set; }
        
        public int BoxPicture18Id { get; set; }        public string BoxLink18 { get; set; }

        public int BoxPicture40Id { get; set; }        public string BoxLink40 { get; set; }

        public int BoxPicture41Id { get; set; }        public string BoxLink41 { get; set; }

        // COLLAGE
        public bool CollageEnable { get; set; }



        public int CollagePicture19Id { get; set; }        public string CollageLink19 { get; set; }



        public int CollagePicture20Id { get; set; }        public string CollageLink20 { get; set; }



        public int CollagePicture21Id { get; set; }        public string CollageLink21 { get; set; }



        public int CollagePicture22Id { get; set; }        public string CollageLink22 { get; set; }



        public int CollagePicture23Id { get; set; }        public string CollageLink23 { get; set; }



        public int CollagePicture24Id { get; set; }        public string CollageLink24 { get; set; }



        public int CollagePicture25Id { get; set; }        public string CollageLink25 { get; set; }



        public int CollagePicture26Id { get; set; }        public string CollageLink26 { get; set; }



        public int CollagePicture27Id { get; set; }        public string CollageLink27 { get; set; }



        public int CollagePicture28Id { get; set; }        public string CollageLink28 { get; set; }



        public int CollagePicture29Id { get; set; }        public string CollageLink29 { get; set; }



        public int CollagePicture30Id { get; set; }        public string CollageLink30 { get; set; }



        public int CollagePicture31Id { get; set; }        public string CollageLink31 { get; set; }



        public int CollagePicture32Id { get; set; }        public string CollageLink32 { get; set; }



        public int CollagePicture33Id { get; set; }        public string CollageLink33 { get; set; }



        public int CollagePicture34Id { get; set; }        public string CollageLink34 { get; set; }



        public int CollagePicture35Id { get; set; }        public string CollageLink35 { get; set; }



        public int CollagePicture36Id { get; set; }        public string CollageLink36 { get; set; }



        public int CollagePicture37Id { get; set; }        public string CollageLink37 { get; set; }



        public int CollagePicture38Id { get; set; }        public string CollageLink38 { get; set; }

        // POP UP
        public bool PopUpEnable { get; set; }



        public int PopUpPicture39Id { get; set; }



        public int PopUpPicture39ResponsiveId { get; set; }

        // PRODUCTS
        public string ProductSectionTitle { get; set; }        public IList<int> SelectedProductIds { get; set; }        public List<ProductOverviewModel> Products { get; set; }
    }}