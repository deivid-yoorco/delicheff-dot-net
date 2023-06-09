﻿using System.Collections.Generic;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerNavigationModel : BaseNopModel
    {
        public CustomerNavigationModel()
        {
            CustomerNavigationItems = new List<CustomerNavigationItemModel>();
        }

        public IList<CustomerNavigationItemModel> CustomerNavigationItems { get; set; }

        public CustomerNavigationEnum SelectedTab { get; set; }

        public CustomerInfoModel CustomerInfo { get; set; }
    }

    public class CustomerNavigationItemModel : BaseNopModel
    {
        public string RouteName { get; set; }
        public string Title { get; set; }
        public CustomerNavigationEnum Tab { get; set; }
        public string ItemClass { get; set; }
    }

    public enum CustomerNavigationEnum
    {
        Info = 0,
        Addresses = 10,
        Orders = 20,
        BackInStockSubscriptions = 30,
        ReturnRequests = 40,
        DownloadableProducts = 50,
        RewardPoints = 60,
        ChangePassword = 70,
        Avatar = 80,
        ForumSubscriptions = 90,
        ProductReviews = 100,
        VendorInfo = 110,
        GrowthHacking = 120,
        Points = 130,
        Leaderboards = 140,
        CustomerBadges = 150,
    }
}