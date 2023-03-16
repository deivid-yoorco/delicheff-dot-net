using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tasks;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Services.Installation
{
    /// <summary>
    /// Code first installation service
    /// </summary>
    public partial class CodeFirstInstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerPassword> _customerPasswordRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IRepository<DeliveryDate> _deliveryDateRepository;
        private readonly IRepository<ProductAvailabilityRange> _productAvailabilityRangeRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IRepository<TopicTemplate> _topicTemplateRepository;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IRepository<ReturnRequestReason> _returnRequestReasonRepository;
        private readonly IRepository<ReturnRequestAction> _returnRequestActionRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<GiftCard> _giftCardRepository;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<SearchTerm> _searchTermRepository;
        private readonly IRepository<ShipmentItem> _shipmentItemRepository;
        private readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Ctor

        public CodeFirstInstallationService(IRepository<Store> storeRepository,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<TaxCategory> taxCategoryRepository,
            IRepository<Language> languageRepository,
            IRepository<Currency> currencyRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<CheckoutAttribute> checkoutAttributeRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<MessageTemplate> messageTemplateRepository,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Discount> discountRepository,
            IRepository<BlogPost> blogPostRepository,
            IRepository<Topic> topicRepository,
            IRepository<NewsItem> newsItemRepository,
            IRepository<Poll> pollRepository,
            IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<DeliveryDate> deliveryDateRepository,
            IRepository<ProductAvailabilityRange> productAvailabilityRangeRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductTemplate> productTemplateRepository,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IRepository<TopicTemplate> topicTemplateRepository,
            IRepository<ScheduleTask> scheduleTaskRepository,
            IRepository<ReturnRequestReason> returnRequestReasonRepository,
            IRepository<ReturnRequestAction> returnRequestActionRepository,
            IRepository<Address> addressRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<Affiliate> affiliateRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<GiftCard> giftCardRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<ShipmentItem> shipmentItemRepository,
            IRepository<SearchTerm> searchTermRepository,
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            IHostingEnvironment hostingEnvironment)
        {
            this._storeRepository = storeRepository;
            this._measureDimensionRepository = measureDimensionRepository;
            this._measureWeightRepository = measureWeightRepository;
            this._taxCategoryRepository = taxCategoryRepository;
            this._languageRepository = languageRepository;
            this._currencyRepository = currencyRepository;
            this._customerRepository = customerRepository;
            this._customerPasswordRepository = customerPasswordRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._checkoutAttributeRepository = checkoutAttributeRepository;
            this._productAttributeRepository = productAttributeRepository;
            this._categoryRepository = categoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._emailAccountRepository = emailAccountRepository;
            this._messageTemplateRepository = messageTemplateRepository;
            this._forumGroupRepository = forumGroupRepository;
            this._forumRepository = forumRepository;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._discountRepository = discountRepository;
            this._blogPostRepository = blogPostRepository;
            this._topicRepository = topicRepository;
            this._newsItemRepository = newsItemRepository;
            this._pollRepository = pollRepository;
            this._shippingMethodRepository = shippingMethodRepository;
            this._deliveryDateRepository = deliveryDateRepository;
            this._productAvailabilityRangeRepository = productAvailabilityRangeRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._activityLogRepository = activityLogRepository;
            this._productTagRepository = productTagRepository;
            this._productTemplateRepository = productTemplateRepository;
            this._categoryTemplateRepository = categoryTemplateRepository;
            this._manufacturerTemplateRepository = manufacturerTemplateRepository;
            this._topicTemplateRepository = topicTemplateRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._returnRequestReasonRepository = returnRequestReasonRepository;
            this._returnRequestActionRepository = returnRequestActionRepository;
            this._addressRepository = addressRepository;
            this._warehouseRepository = warehouseRepository;
            this._vendorRepository = vendorRepository;
            this._affiliateRepository = affiliateRepository;
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
            this._orderNoteRepository = orderNoteRepository;
            this._giftCardRepository = giftCardRepository;
            this._shipmentRepository = shipmentRepository;
            this._shipmentItemRepository = shipmentItemRepository;
            this._searchTermRepository = searchTermRepository;
            this._stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
            this._hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Utilities

        protected virtual string GetSamplesPath()
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, "images\\samples\\");
        }

        protected virtual void InstallStores()
        {
            //var storeUrl = "http://www.yourStore.com/";
            var storeUrl = _webHelper.GetStoreLocation(false);
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "TeedCommerce",
                    Url = storeUrl,
                    SslEnabled = false,
                    Hosts = "yourstore.com,www.yourstore.com",
                    DisplayOrder = 1,
                    //should we set some default company info?
                    CompanyName = "Teed Innovación Tecnológica",
                    CompanyAddress = "World Trade Center, Piso 39, Oficina 32, Ciudad de México",
                    CompanyPhoneNumber = "+52 (55) 9001 0126",
                    CompanyVat = null,
                },
            };

            _storeRepository.Insert(stores);
        }

        protected virtual void InstallMeasures()
        {
            var measureDimensions = new List<MeasureDimension>
            {
                new MeasureDimension
                {
                    Name = "pulgada(s)",
                    SystemKeyword = "inches",
                    Ratio = 1M,
                    DisplayOrder = 1,
                },
                new MeasureDimension
                {
                    Name = "pie(s)",
                    SystemKeyword = "feet",
                    Ratio = 0.08333333M,
                    DisplayOrder = 2,
                },
                new MeasureDimension
                {
                    Name = "metro(s)",
                    SystemKeyword = "meters",
                    Ratio = 0.0254M,
                    DisplayOrder = 3,
                },
                new MeasureDimension
                {
                    Name = "milimetro(s)",
                    SystemKeyword = "millimetres",
                    Ratio = 25.4M,
                    DisplayOrder = 4,
                }
            };

            _measureDimensionRepository.Insert(measureDimensions);

            var measureWeights = new List<MeasureWeight>
            {
                new MeasureWeight
                {
                    Name = "onza(s)",
                    SystemKeyword = "ounce",
                    Ratio = 16M,
                    DisplayOrder = 1,
                },
                new MeasureWeight
                {
                    Name = "libra(s)",
                    SystemKeyword = "lb",
                    Ratio = 1M,
                    DisplayOrder = 2,
                },
                new MeasureWeight
                {
                    Name = "kilogramo(s)",
                    SystemKeyword = "kg",
                    Ratio = 0.45359237M,
                    DisplayOrder = 3,
                },
                new MeasureWeight
                {
                    Name = "gramo(s)",
                    SystemKeyword = "grams",
                    Ratio = 453.59237M,
                    DisplayOrder = 4,
                }
            };

            _measureWeightRepository.Insert(measureWeights);
        }

        protected virtual void InstallTaxCategories()
        {
            var taxCategories = new List<TaxCategory>
                               {
                                   new TaxCategory
                                       {
                                           Name = "IVA",
                                           DisplayOrder = 1,
                                       }
                               };
            _taxCategoryRepository.Insert(taxCategories);

        }

        protected virtual void InstallLanguages()
        {
            var language = new Language
            {
                Name = "Español",
                LanguageCulture = "es-MX",
                UniqueSeoCode = "es",
                FlagImageFileName = "mx.png",
                Published = true,
                DisplayOrder = 1
            };
            _languageRepository.Insert(language);
        }

        protected virtual void InstallLocaleResources()
        {
            //Installing for Spanish
            var language = _languageRepository.Table.Single(l => l.Name == "Español");

            //save resources
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath("~/App_Data/Localization/"), "es_MX.xml", SearchOption.TopDirectoryOnly))
            {
                var localesXml = File.ReadAllText(filePath);
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                localizationService.ImportResourcesFromXml(language, localesXml);
            }

        }

        protected virtual void InstallCurrencies()
        {
            var currencies = new List<Currency>
            {
                new Currency
                {
                    Name = "Peso Mexicano",
                    CurrencyCode = "MXN",
                    Rate = 1,
                    DisplayLocale = "es-MX",
                    CustomFormatting = "",
                    Published = true,
                    DisplayOrder = 0,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "US Dollar",
                    CurrencyCode = "USD",
                    Rate = 1,
                    DisplayLocale = "en-US",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 1,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Australian Dollar",
                    CurrencyCode = "AUD",
                    Rate = 1.36M,
                    DisplayLocale = "en-AU",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 2,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "British Pound",
                    CurrencyCode = "GBP",
                    Rate = 0.82M,
                    DisplayLocale = "en-GB",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 3,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Canadian Dollar",
                    CurrencyCode = "CAD",
                    Rate = 1.32M,
                    DisplayLocale = "en-CA",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 4,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Chinese Yuan Renminbi",
                    CurrencyCode = "CNY",
                    Rate = 6.93M,
                    DisplayLocale = "zh-CN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 5,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Euro",
                    CurrencyCode = "EUR",
                    Rate = 0.95M,
                    DisplayLocale = "",
                    //CustomFormatting = "ˆ0.00",
                    CustomFormatting = $"{"\u20ac"}0.00",
                    Published = false,
                    DisplayOrder = 6,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Hong Kong Dollar",
                    CurrencyCode = "HKD",
                    Rate = 7.75M,
                    DisplayLocale = "zh-HK",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 7,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Japanese Yen",
                    CurrencyCode = "JPY",
                    Rate = 116.64M,
                    DisplayLocale = "ja-JP",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 8,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Russian Rouble",
                    CurrencyCode = "RUB",
                    Rate = 59.75M,
                    DisplayLocale = "ru-RU",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 9,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Swedish Krona",
                    CurrencyCode = "SEK",
                    Rate = 9.08M,
                    DisplayLocale = "sv-SE",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 10,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding1
                },
                new Currency
                {
                    Name = "Romanian Leu",
                    CurrencyCode = "RON",
                    Rate = 4.28M,
                    DisplayLocale = "ro-RO",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 11,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Indian Rupee",
                    CurrencyCode = "INR",
                    Rate = 68.17M,
                    DisplayLocale = "en-IN",
                    CustomFormatting = "",
                    Published = false,
                    DisplayOrder = 12,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
            };
            _currencyRepository.Insert(currencies);
        }

        protected virtual void InstallCountriesAndStates()
        {
            var cUsa = new Country
            {
                Name = "United States",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
                NumericIsoCode = 840,
                SubjectToVat = false,
                DisplayOrder = 1,
                Published = false,
            };
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "AA (Armed Forces Americas)",
                Abbreviation = "AA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "AE (Armed Forces Europe)",
                Abbreviation = "AE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Alabama",
                Abbreviation = "AL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Alaska",
                Abbreviation = "AK",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "American Samoa",
                Abbreviation = "AS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "AP (Armed Forces Pacific)",
                Abbreviation = "AP",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Arizona",
                Abbreviation = "AZ",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Arkansas",
                Abbreviation = "AR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "California",
                Abbreviation = "CA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Colorado",
                Abbreviation = "CO",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Connecticut",
                Abbreviation = "CT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Delaware",
                Abbreviation = "DE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "District of Columbia",
                Abbreviation = "DC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Federated States of Micronesia",
                Abbreviation = "FM",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Florida",
                Abbreviation = "FL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Georgia",
                Abbreviation = "GA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Guam",
                Abbreviation = "GU",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Hawaii",
                Abbreviation = "HI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Idaho",
                Abbreviation = "ID",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Illinois",
                Abbreviation = "IL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Indiana",
                Abbreviation = "IN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Iowa",
                Abbreviation = "IA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Kansas",
                Abbreviation = "KS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Kentucky",
                Abbreviation = "KY",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Louisiana",
                Abbreviation = "LA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Maine",
                Abbreviation = "ME",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Marshall Islands",
                Abbreviation = "MH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Maryland",
                Abbreviation = "MD",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Massachusetts",
                Abbreviation = "MA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Michigan",
                Abbreviation = "MI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Minnesota",
                Abbreviation = "MN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Mississippi",
                Abbreviation = "MS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Missouri",
                Abbreviation = "MO",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Montana",
                Abbreviation = "MT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Nebraska",
                Abbreviation = "NE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Nevada",
                Abbreviation = "NV",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "New Hampshire",
                Abbreviation = "NH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "New Jersey",
                Abbreviation = "NJ",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "New Mexico",
                Abbreviation = "NM",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "New York",
                Abbreviation = "NY",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "North Carolina",
                Abbreviation = "NC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "North Dakota",
                Abbreviation = "ND",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Northern Mariana Islands",
                Abbreviation = "MP",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Ohio",
                Abbreviation = "OH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Oklahoma",
                Abbreviation = "OK",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Oregon",
                Abbreviation = "OR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Palau",
                Abbreviation = "PW",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Pennsylvania",
                Abbreviation = "PA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Puerto Rico",
                Abbreviation = "PR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Rhode Island",
                Abbreviation = "RI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "South Carolina",
                Abbreviation = "SC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "South Dakota",
                Abbreviation = "SD",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Tennessee",
                Abbreviation = "TN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Texas",
                Abbreviation = "TX",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Utah",
                Abbreviation = "UT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Vermont",
                Abbreviation = "VT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Virgin Islands",
                Abbreviation = "VI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Virginia",
                Abbreviation = "VA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Washington",
                Abbreviation = "WA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "West Virginia",
                Abbreviation = "WV",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Wisconsin",
                Abbreviation = "WI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince
            {
                Name = "Wyoming",
                Abbreviation = "WY",
                Published = true,
                DisplayOrder = 1,
            });
            var cCanada = new Country
            {
                Name = "Canada",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "CA",
                ThreeLetterIsoCode = "CAN",
                NumericIsoCode = 124,
                SubjectToVat = false,
                DisplayOrder = 100,
                Published = false,
            };
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Alberta",
                Abbreviation = "AB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "British Columbia",
                Abbreviation = "BC",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Manitoba",
                Abbreviation = "MB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "New Brunswick",
                Abbreviation = "NB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Newfoundland and Labrador",
                Abbreviation = "NL",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Northwest Territories",
                Abbreviation = "NT",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Nova Scotia",
                Abbreviation = "NS",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Nunavut",
                Abbreviation = "NU",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Ontario",
                Abbreviation = "ON",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Prince Edward Island",
                Abbreviation = "PE",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Quebec",
                Abbreviation = "QC",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Saskatchewan",
                Abbreviation = "SK",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince
            {
                Name = "Yukon Territory",
                Abbreviation = "YT",
                Published = true,
                DisplayOrder = 1,
            });
            var cMexico = new Country
            {
                Name = "Mexico",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "MX",
                ThreeLetterIsoCode = "MEX",
                NumericIsoCode = 484,
                SubjectToVat = false,
                DisplayOrder = 100,
                Published = true
            };
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Ciudad de México",
                Abbreviation = "CMX",
                Published = true,
                DisplayOrder = 0,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Aguascalientes",
                Abbreviation = "AGU",
                Published = true,
                DisplayOrder = 1,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Baja California",
                Abbreviation = "BCN",
                Published = true,
                DisplayOrder = 2,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Baja California Sur",
                Abbreviation = "BCS",
                Published = true,
                DisplayOrder = 3,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Campeche",
                Abbreviation = "CAM",
                Published = true,
                DisplayOrder = 4,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Coahuila",
                Abbreviation = "COA",
                Published = true,
                DisplayOrder = 5,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Colima",
                Abbreviation = "COL",
                Published = true,
                DisplayOrder = 6,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Chiapas",
                Abbreviation = "CHP",
                Published = true,
                DisplayOrder = 7,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Chihuahua",
                Abbreviation = "CHH",
                Published = true,
                DisplayOrder = 8,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Durango",
                Abbreviation = "DUR",
                Published = true,
                DisplayOrder = 9,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Guanajuato",
                Abbreviation = "GUA",
                Published = true,
                DisplayOrder = 10,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Guerrero",
                Abbreviation = "GRO",
                Published = true,
                DisplayOrder = 11,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Hidalgo",
                Abbreviation = "HID",
                Published = true,
                DisplayOrder = 12,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Jalisco",
                Abbreviation = "JAL",
                Published = true,
                DisplayOrder = 13,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Estado de México",
                Abbreviation = "MEX",
                Published = true,
                DisplayOrder = 14,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Michoacán",
                Abbreviation = "MIC",
                Published = true,
                DisplayOrder = 15,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Morelos",
                Abbreviation = "MOR",
                Published = true,
                DisplayOrder = 16,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Nayarit",
                Abbreviation = "NAY",
                Published = true,
                DisplayOrder = 17,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Nuevo León",
                Abbreviation = "NLE",
                Published = true,
                DisplayOrder = 18,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Oaxaca",
                Abbreviation = "OAX",
                Published = true,
                DisplayOrder = 19,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Puebla",
                Abbreviation = "PUE",
                Published = true,
                DisplayOrder = 20,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Querétaro",
                Abbreviation = "QUE",
                Published = true,
                DisplayOrder = 21,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Quintana Roo",
                Abbreviation = "ROO",
                Published = true,
                DisplayOrder = 22,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "San Luis Potosí",
                Abbreviation = "SLP",
                Published = true,
                DisplayOrder = 23,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Sinaloa",
                Abbreviation = "SIN",
                Published = true,
                DisplayOrder = 24,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Sonora",
                Abbreviation = "SON",
                Published = true,
                DisplayOrder = 25,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Tabasco",
                Abbreviation = "TAB",
                Published = true,
                DisplayOrder = 26,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Tamaulipas",
                Abbreviation = "TAM",
                Published = true,
                DisplayOrder = 27,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Tlaxcala",
                Abbreviation = "TLA",
                Published = true,
                DisplayOrder = 28,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Veracruz",
                Abbreviation = "VER",
                Published = true,
                DisplayOrder = 29,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Yucatán",
                Abbreviation = "YUC",
                Published = true,
                DisplayOrder = 30,
            });
            cMexico.StateProvinces.Add(new StateProvince
            {
                Name = "Zacatecas",
                Abbreviation = "ZAC",
                Published = true,
                DisplayOrder = 31,
            });
            var countries = new List<Country>
            {
                cUsa,
                cCanada,
                cMexico,
                //other countries
                new Country
                {
                    Name = "Argentina",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AR",
                    ThreeLetterIsoCode = "ARG",
                    NumericIsoCode = 32,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Armenia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AM",
                    ThreeLetterIsoCode = "ARM",
                    NumericIsoCode = 51,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Aruba",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AW",
                    ThreeLetterIsoCode = "ABW",
                    NumericIsoCode = 533,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Australia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AU",
                    ThreeLetterIsoCode = "AUS",
                    NumericIsoCode = 36,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Austria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AT",
                    ThreeLetterIsoCode = "AUT",
                    NumericIsoCode = 40,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Azerbaijan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AZ",
                    ThreeLetterIsoCode = "AZE",
                    NumericIsoCode = 31,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bahamas",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BS",
                    ThreeLetterIsoCode = "BHS",
                    NumericIsoCode = 44,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bangladesh",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BD",
                    ThreeLetterIsoCode = "BGD",
                    NumericIsoCode = 50,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Belarus",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BY",
                    ThreeLetterIsoCode = "BLR",
                    NumericIsoCode = 112,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Belgium",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BE",
                    ThreeLetterIsoCode = "BEL",
                    NumericIsoCode = 56,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Belize",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BZ",
                    ThreeLetterIsoCode = "BLZ",
                    NumericIsoCode = 84,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bermuda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BM",
                    ThreeLetterIsoCode = "BMU",
                    NumericIsoCode = 60,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bolivia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BO",
                    ThreeLetterIsoCode = "BOL",
                    NumericIsoCode = 68,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bosnia and Herzegowina",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BA",
                    ThreeLetterIsoCode = "BIH",
                    NumericIsoCode = 70,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Brazil",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BR",
                    ThreeLetterIsoCode = "BRA",
                    NumericIsoCode = 76,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bulgaria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BG",
                    ThreeLetterIsoCode = "BGR",
                    NumericIsoCode = 100,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cayman Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KY",
                    ThreeLetterIsoCode = "CYM",
                    NumericIsoCode = 136,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Chile",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CL",
                    ThreeLetterIsoCode = "CHL",
                    NumericIsoCode = 152,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "China",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CN",
                    ThreeLetterIsoCode = "CHN",
                    NumericIsoCode = 156,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Colombia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CO",
                    ThreeLetterIsoCode = "COL",
                    NumericIsoCode = 170,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Costa Rica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CR",
                    ThreeLetterIsoCode = "CRI",
                    NumericIsoCode = 188,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Croatia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HR",
                    ThreeLetterIsoCode = "HRV",
                    NumericIsoCode = 191,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cuba",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CU",
                    ThreeLetterIsoCode = "CUB",
                    NumericIsoCode = 192,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cyprus",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CY",
                    ThreeLetterIsoCode = "CYP",
                    NumericIsoCode = 196,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Czech Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CZ",
                    ThreeLetterIsoCode = "CZE",
                    NumericIsoCode = 203,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Denmark",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DK",
                    ThreeLetterIsoCode = "DNK",
                    NumericIsoCode = 208,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Dominican Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DO",
                    ThreeLetterIsoCode = "DOM",
                    NumericIsoCode = 214,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "East Timor",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TL",
                    ThreeLetterIsoCode = "TLS",
                    NumericIsoCode = 626,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Ecuador",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EC",
                    ThreeLetterIsoCode = "ECU",
                    NumericIsoCode = 218,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Egypt",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EG",
                    ThreeLetterIsoCode = "EGY",
                    NumericIsoCode = 818,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Finland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FI",
                    ThreeLetterIsoCode = "FIN",
                    NumericIsoCode = 246,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "France",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FR",
                    ThreeLetterIsoCode = "FRA",
                    NumericIsoCode = 250,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Georgia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GE",
                    ThreeLetterIsoCode = "GEO",
                    NumericIsoCode = 268,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Germany",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DE",
                    ThreeLetterIsoCode = "DEU",
                    NumericIsoCode = 276,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Gibraltar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GI",
                    ThreeLetterIsoCode = "GIB",
                    NumericIsoCode = 292,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Greece",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GR",
                    ThreeLetterIsoCode = "GRC",
                    NumericIsoCode = 300,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Guatemala",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GT",
                    ThreeLetterIsoCode = "GTM",
                    NumericIsoCode = 320,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Hong Kong",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HK",
                    ThreeLetterIsoCode = "HKG",
                    NumericIsoCode = 344,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Hungary",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HU",
                    ThreeLetterIsoCode = "HUN",
                    NumericIsoCode = 348,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "India",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IN",
                    ThreeLetterIsoCode = "IND",
                    NumericIsoCode = 356,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Indonesia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ID",
                    ThreeLetterIsoCode = "IDN",
                    NumericIsoCode = 360,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Ireland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IE",
                    ThreeLetterIsoCode = "IRL",
                    NumericIsoCode = 372,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Israel",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IL",
                    ThreeLetterIsoCode = "ISR",
                    NumericIsoCode = 376,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Italy",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IT",
                    ThreeLetterIsoCode = "ITA",
                    NumericIsoCode = 380,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Jamaica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "JM",
                    ThreeLetterIsoCode = "JAM",
                    NumericIsoCode = 388,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Japan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "JP",
                    ThreeLetterIsoCode = "JPN",
                    NumericIsoCode = 392,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Jordan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "JO",
                    ThreeLetterIsoCode = "JOR",
                    NumericIsoCode = 400,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Kazakhstan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KZ",
                    ThreeLetterIsoCode = "KAZ",
                    NumericIsoCode = 398,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Korea, Democratic People's Republic of",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KP",
                    ThreeLetterIsoCode = "PRK",
                    NumericIsoCode = 408,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Kuwait",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KW",
                    ThreeLetterIsoCode = "KWT",
                    NumericIsoCode = 414,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Malaysia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MY",
                    ThreeLetterIsoCode = "MYS",
                    NumericIsoCode = 458,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Netherlands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NL",
                    ThreeLetterIsoCode = "NLD",
                    NumericIsoCode = 528,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "New Zealand",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NZ",
                    ThreeLetterIsoCode = "NZL",
                    NumericIsoCode = 554,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Norway",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NO",
                    ThreeLetterIsoCode = "NOR",
                    NumericIsoCode = 578,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Pakistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PK",
                    ThreeLetterIsoCode = "PAK",
                    NumericIsoCode = 586,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Palestine",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PS",
                    ThreeLetterIsoCode = "PSE",
                    NumericIsoCode = 275,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Paraguay",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PY",
                    ThreeLetterIsoCode = "PRY",
                    NumericIsoCode = 600,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Peru",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PE",
                    ThreeLetterIsoCode = "PER",
                    NumericIsoCode = 604,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Philippines",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PH",
                    ThreeLetterIsoCode = "PHL",
                    NumericIsoCode = 608,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Poland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PL",
                    ThreeLetterIsoCode = "POL",
                    NumericIsoCode = 616,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Portugal",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PT",
                    ThreeLetterIsoCode = "PRT",
                    NumericIsoCode = 620,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Puerto Rico",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PR",
                    ThreeLetterIsoCode = "PRI",
                    NumericIsoCode = 630,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Qatar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "QA",
                    ThreeLetterIsoCode = "QAT",
                    NumericIsoCode = 634,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Romania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RO",
                    ThreeLetterIsoCode = "ROM",
                    NumericIsoCode = 642,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Russian Federation",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RU",
                    ThreeLetterIsoCode = "RUS",
                    NumericIsoCode = 643,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Saudi Arabia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SA",
                    ThreeLetterIsoCode = "SAU",
                    NumericIsoCode = 682,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Singapore",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SG",
                    ThreeLetterIsoCode = "SGP",
                    NumericIsoCode = 702,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Slovakia (Slovak Republic)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SK",
                    ThreeLetterIsoCode = "SVK",
                    NumericIsoCode = 703,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Slovenia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SI",
                    ThreeLetterIsoCode = "SVN",
                    NumericIsoCode = 705,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "South Africa",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ZA",
                    ThreeLetterIsoCode = "ZAF",
                    NumericIsoCode = 710,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Spain",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ES",
                    ThreeLetterIsoCode = "ESP",
                    NumericIsoCode = 724,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Sweden",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SE",
                    ThreeLetterIsoCode = "SWE",
                    NumericIsoCode = 752,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Switzerland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CH",
                    ThreeLetterIsoCode = "CHE",
                    NumericIsoCode = 756,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Taiwan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TW",
                    ThreeLetterIsoCode = "TWN",
                    NumericIsoCode = 158,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Thailand",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TH",
                    ThreeLetterIsoCode = "THA",
                    NumericIsoCode = 764,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Turkey",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TR",
                    ThreeLetterIsoCode = "TUR",
                    NumericIsoCode = 792,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Ukraine",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UA",
                    ThreeLetterIsoCode = "UKR",
                    NumericIsoCode = 804,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "United Arab Emirates",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AE",
                    ThreeLetterIsoCode = "ARE",
                    NumericIsoCode = 784,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "United Kingdom",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GB",
                    ThreeLetterIsoCode = "GBR",
                    NumericIsoCode = 826,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "United States minor outlying islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UM",
                    ThreeLetterIsoCode = "UMI",
                    NumericIsoCode = 581,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Uruguay",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UY",
                    ThreeLetterIsoCode = "URY",
                    NumericIsoCode = 858,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Uzbekistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UZ",
                    ThreeLetterIsoCode = "UZB",
                    NumericIsoCode = 860,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Venezuela",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VE",
                    ThreeLetterIsoCode = "VEN",
                    NumericIsoCode = 862,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Serbia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RS",
                    ThreeLetterIsoCode = "SRB",
                    NumericIsoCode = 688,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Afghanistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AF",
                    ThreeLetterIsoCode = "AFG",
                    NumericIsoCode = 4,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Albania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AL",
                    ThreeLetterIsoCode = "ALB",
                    NumericIsoCode = 8,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Algeria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DZ",
                    ThreeLetterIsoCode = "DZA",
                    NumericIsoCode = 12,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "American Samoa",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AS",
                    ThreeLetterIsoCode = "ASM",
                    NumericIsoCode = 16,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Andorra",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AD",
                    ThreeLetterIsoCode = "AND",
                    NumericIsoCode = 20,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Angola",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AO",
                    ThreeLetterIsoCode = "AGO",
                    NumericIsoCode = 24,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Anguilla",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AI",
                    ThreeLetterIsoCode = "AIA",
                    NumericIsoCode = 660,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Antarctica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AQ",
                    ThreeLetterIsoCode = "ATA",
                    NumericIsoCode = 10,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Antigua and Barbuda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AG",
                    ThreeLetterIsoCode = "ATG",
                    NumericIsoCode = 28,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bahrain",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BH",
                    ThreeLetterIsoCode = "BHR",
                    NumericIsoCode = 48,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Barbados",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BB",
                    ThreeLetterIsoCode = "BRB",
                    NumericIsoCode = 52,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Benin",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BJ",
                    ThreeLetterIsoCode = "BEN",
                    NumericIsoCode = 204,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bhutan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BT",
                    ThreeLetterIsoCode = "BTN",
                    NumericIsoCode = 64,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Botswana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BW",
                    ThreeLetterIsoCode = "BWA",
                    NumericIsoCode = 72,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Bouvet Island",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BV",
                    ThreeLetterIsoCode = "BVT",
                    NumericIsoCode = 74,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "British Indian Ocean Territory",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IO",
                    ThreeLetterIsoCode = "IOT",
                    NumericIsoCode = 86,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Brunei Darussalam",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BN",
                    ThreeLetterIsoCode = "BRN",
                    NumericIsoCode = 96,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Burkina Faso",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BF",
                    ThreeLetterIsoCode = "BFA",
                    NumericIsoCode = 854,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Burundi",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BI",
                    ThreeLetterIsoCode = "BDI",
                    NumericIsoCode = 108,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cambodia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KH",
                    ThreeLetterIsoCode = "KHM",
                    NumericIsoCode = 116,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cameroon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CM",
                    ThreeLetterIsoCode = "CMR",
                    NumericIsoCode = 120,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cape Verde",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CV",
                    ThreeLetterIsoCode = "CPV",
                    NumericIsoCode = 132,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Central African Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CF",
                    ThreeLetterIsoCode = "CAF",
                    NumericIsoCode = 140,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Chad",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TD",
                    ThreeLetterIsoCode = "TCD",
                    NumericIsoCode = 148,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Christmas Island",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CX",
                    ThreeLetterIsoCode = "CXR",
                    NumericIsoCode = 162,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cocos (Keeling) Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CC",
                    ThreeLetterIsoCode = "CCK",
                    NumericIsoCode = 166,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Comoros",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KM",
                    ThreeLetterIsoCode = "COM",
                    NumericIsoCode = 174,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Congo",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CG",
                    ThreeLetterIsoCode = "COG",
                    NumericIsoCode = 178,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Congo (Democratic Republic of the)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CD",
                    ThreeLetterIsoCode = "COD",
                    NumericIsoCode = 180,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cook Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CK",
                    ThreeLetterIsoCode = "COK",
                    NumericIsoCode = 184,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Cote D'Ivoire",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CI",
                    ThreeLetterIsoCode = "CIV",
                    NumericIsoCode = 384,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Djibouti",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DJ",
                    ThreeLetterIsoCode = "DJI",
                    NumericIsoCode = 262,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Dominica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DM",
                    ThreeLetterIsoCode = "DMA",
                    NumericIsoCode = 212,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "El Salvador",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SV",
                    ThreeLetterIsoCode = "SLV",
                    NumericIsoCode = 222,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Equatorial Guinea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GQ",
                    ThreeLetterIsoCode = "GNQ",
                    NumericIsoCode = 226,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Eritrea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ER",
                    ThreeLetterIsoCode = "ERI",
                    NumericIsoCode = 232,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Estonia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EE",
                    ThreeLetterIsoCode = "EST",
                    NumericIsoCode = 233,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Ethiopia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ET",
                    ThreeLetterIsoCode = "ETH",
                    NumericIsoCode = 231,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Falkland Islands (Malvinas)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FK",
                    ThreeLetterIsoCode = "FLK",
                    NumericIsoCode = 238,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Faroe Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FO",
                    ThreeLetterIsoCode = "FRO",
                    NumericIsoCode = 234,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Fiji",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FJ",
                    ThreeLetterIsoCode = "FJI",
                    NumericIsoCode = 242,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "French Guiana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GF",
                    ThreeLetterIsoCode = "GUF",
                    NumericIsoCode = 254,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "French Polynesia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PF",
                    ThreeLetterIsoCode = "PYF",
                    NumericIsoCode = 258,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "French Southern Territories",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TF",
                    ThreeLetterIsoCode = "ATF",
                    NumericIsoCode = 260,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Gabon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GA",
                    ThreeLetterIsoCode = "GAB",
                    NumericIsoCode = 266,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Gambia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GM",
                    ThreeLetterIsoCode = "GMB",
                    NumericIsoCode = 270,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Ghana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GH",
                    ThreeLetterIsoCode = "GHA",
                    NumericIsoCode = 288,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Greenland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GL",
                    ThreeLetterIsoCode = "GRL",
                    NumericIsoCode = 304,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Grenada",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GD",
                    ThreeLetterIsoCode = "GRD",
                    NumericIsoCode = 308,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Guadeloupe",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GP",
                    ThreeLetterIsoCode = "GLP",
                    NumericIsoCode = 312,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Guam",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GU",
                    ThreeLetterIsoCode = "GUM",
                    NumericIsoCode = 316,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Guinea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GN",
                    ThreeLetterIsoCode = "GIN",
                    NumericIsoCode = 324,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Guinea-bissau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GW",
                    ThreeLetterIsoCode = "GNB",
                    NumericIsoCode = 624,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Guyana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GY",
                    ThreeLetterIsoCode = "GUY",
                    NumericIsoCode = 328,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Haiti",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HT",
                    ThreeLetterIsoCode = "HTI",
                    NumericIsoCode = 332,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Heard and Mc Donald Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HM",
                    ThreeLetterIsoCode = "HMD",
                    NumericIsoCode = 334,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Honduras",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HN",
                    ThreeLetterIsoCode = "HND",
                    NumericIsoCode = 340,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Iceland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IS",
                    ThreeLetterIsoCode = "ISL",
                    NumericIsoCode = 352,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Iran (Islamic Republic of)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IR",
                    ThreeLetterIsoCode = "IRN",
                    NumericIsoCode = 364,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Iraq",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IQ",
                    ThreeLetterIsoCode = "IRQ",
                    NumericIsoCode = 368,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Kenya",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KE",
                    ThreeLetterIsoCode = "KEN",
                    NumericIsoCode = 404,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Kiribati",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KI",
                    ThreeLetterIsoCode = "KIR",
                    NumericIsoCode = 296,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Korea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KR",
                    ThreeLetterIsoCode = "KOR",
                    NumericIsoCode = 410,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Kyrgyzstan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KG",
                    ThreeLetterIsoCode = "KGZ",
                    NumericIsoCode = 417,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Lao People's Democratic Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LA",
                    ThreeLetterIsoCode = "LAO",
                    NumericIsoCode = 418,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Latvia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LV",
                    ThreeLetterIsoCode = "LVA",
                    NumericIsoCode = 428,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Lebanon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LB",
                    ThreeLetterIsoCode = "LBN",
                    NumericIsoCode = 422,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Lesotho",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LS",
                    ThreeLetterIsoCode = "LSO",
                    NumericIsoCode = 426,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Liberia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LR",
                    ThreeLetterIsoCode = "LBR",
                    NumericIsoCode = 430,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Libyan Arab Jamahiriya",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LY",
                    ThreeLetterIsoCode = "LBY",
                    NumericIsoCode = 434,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Liechtenstein",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LI",
                    ThreeLetterIsoCode = "LIE",
                    NumericIsoCode = 438,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Lithuania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LT",
                    ThreeLetterIsoCode = "LTU",
                    NumericIsoCode = 440,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Luxembourg",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LU",
                    ThreeLetterIsoCode = "LUX",
                    NumericIsoCode = 442,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Macau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MO",
                    ThreeLetterIsoCode = "MAC",
                    NumericIsoCode = 446,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Macedonia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MK",
                    ThreeLetterIsoCode = "MKD",
                    NumericIsoCode = 807,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Madagascar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MG",
                    ThreeLetterIsoCode = "MDG",
                    NumericIsoCode = 450,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Malawi",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MW",
                    ThreeLetterIsoCode = "MWI",
                    NumericIsoCode = 454,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Maldives",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MV",
                    ThreeLetterIsoCode = "MDV",
                    NumericIsoCode = 462,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Mali",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ML",
                    ThreeLetterIsoCode = "MLI",
                    NumericIsoCode = 466,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Malta",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MT",
                    ThreeLetterIsoCode = "MLT",
                    NumericIsoCode = 470,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Marshall Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MH",
                    ThreeLetterIsoCode = "MHL",
                    NumericIsoCode = 584,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Martinique",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MQ",
                    ThreeLetterIsoCode = "MTQ",
                    NumericIsoCode = 474,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Mauritania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MR",
                    ThreeLetterIsoCode = "MRT",
                    NumericIsoCode = 478,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Mauritius",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MU",
                    ThreeLetterIsoCode = "MUS",
                    NumericIsoCode = 480,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Mayotte",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "YT",
                    ThreeLetterIsoCode = "MYT",
                    NumericIsoCode = 175,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Micronesia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FM",
                    ThreeLetterIsoCode = "FSM",
                    NumericIsoCode = 583,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Moldova",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MD",
                    ThreeLetterIsoCode = "MDA",
                    NumericIsoCode = 498,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Monaco",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MC",
                    ThreeLetterIsoCode = "MCO",
                    NumericIsoCode = 492,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Mongolia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MN",
                    ThreeLetterIsoCode = "MNG",
                    NumericIsoCode = 496,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Montenegro",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ME",
                    ThreeLetterIsoCode = "MNE",
                    NumericIsoCode = 499,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Montserrat",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MS",
                    ThreeLetterIsoCode = "MSR",
                    NumericIsoCode = 500,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Morocco",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MA",
                    ThreeLetterIsoCode = "MAR",
                    NumericIsoCode = 504,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Mozambique",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MZ",
                    ThreeLetterIsoCode = "MOZ",
                    NumericIsoCode = 508,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Myanmar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MM",
                    ThreeLetterIsoCode = "MMR",
                    NumericIsoCode = 104,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Namibia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NA",
                    ThreeLetterIsoCode = "NAM",
                    NumericIsoCode = 516,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Nauru",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NR",
                    ThreeLetterIsoCode = "NRU",
                    NumericIsoCode = 520,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Nepal",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NP",
                    ThreeLetterIsoCode = "NPL",
                    NumericIsoCode = 524,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Netherlands Antilles",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AN",
                    ThreeLetterIsoCode = "ANT",
                    NumericIsoCode = 530,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "New Caledonia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NC",
                    ThreeLetterIsoCode = "NCL",
                    NumericIsoCode = 540,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Nicaragua",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NI",
                    ThreeLetterIsoCode = "NIC",
                    NumericIsoCode = 558,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Niger",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NE",
                    ThreeLetterIsoCode = "NER",
                    NumericIsoCode = 562,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Nigeria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NG",
                    ThreeLetterIsoCode = "NGA",
                    NumericIsoCode = 566,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Niue",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NU",
                    ThreeLetterIsoCode = "NIU",
                    NumericIsoCode = 570,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Norfolk Island",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NF",
                    ThreeLetterIsoCode = "NFK",
                    NumericIsoCode = 574,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Northern Mariana Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MP",
                    ThreeLetterIsoCode = "MNP",
                    NumericIsoCode = 580,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Oman",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "OM",
                    ThreeLetterIsoCode = "OMN",
                    NumericIsoCode = 512,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Palau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PW",
                    ThreeLetterIsoCode = "PLW",
                    NumericIsoCode = 585,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Panama",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PA",
                    ThreeLetterIsoCode = "PAN",
                    NumericIsoCode = 591,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Papua New Guinea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PG",
                    ThreeLetterIsoCode = "PNG",
                    NumericIsoCode = 598,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Pitcairn",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PN",
                    ThreeLetterIsoCode = "PCN",
                    NumericIsoCode = 612,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Reunion",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RE",
                    ThreeLetterIsoCode = "REU",
                    NumericIsoCode = 638,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Rwanda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RW",
                    ThreeLetterIsoCode = "RWA",
                    NumericIsoCode = 646,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Saint Kitts and Nevis",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KN",
                    ThreeLetterIsoCode = "KNA",
                    NumericIsoCode = 659,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Saint Lucia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LC",
                    ThreeLetterIsoCode = "LCA",
                    NumericIsoCode = 662,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Saint Vincent and the Grenadines",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VC",
                    ThreeLetterIsoCode = "VCT",
                    NumericIsoCode = 670,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Samoa",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "WS",
                    ThreeLetterIsoCode = "WSM",
                    NumericIsoCode = 882,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "San Marino",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SM",
                    ThreeLetterIsoCode = "SMR",
                    NumericIsoCode = 674,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Sao Tome and Principe",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ST",
                    ThreeLetterIsoCode = "STP",
                    NumericIsoCode = 678,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Senegal",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SN",
                    ThreeLetterIsoCode = "SEN",
                    NumericIsoCode = 686,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Seychelles",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SC",
                    ThreeLetterIsoCode = "SYC",
                    NumericIsoCode = 690,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Sierra Leone",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SL",
                    ThreeLetterIsoCode = "SLE",
                    NumericIsoCode = 694,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Solomon Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SB",
                    ThreeLetterIsoCode = "SLB",
                    NumericIsoCode = 90,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Somalia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SO",
                    ThreeLetterIsoCode = "SOM",
                    NumericIsoCode = 706,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "South Georgia & South Sandwich Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GS",
                    ThreeLetterIsoCode = "SGS",
                    NumericIsoCode = 239,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "South Sudan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SS",
                    ThreeLetterIsoCode = "SSD",
                    NumericIsoCode = 728,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Sri Lanka",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LK",
                    ThreeLetterIsoCode = "LKA",
                    NumericIsoCode = 144,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "St. Helena",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SH",
                    ThreeLetterIsoCode = "SHN",
                    NumericIsoCode = 654,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "St. Pierre and Miquelon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PM",
                    ThreeLetterIsoCode = "SPM",
                    NumericIsoCode = 666,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Sudan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SD",
                    ThreeLetterIsoCode = "SDN",
                    NumericIsoCode = 736,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Suriname",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SR",
                    ThreeLetterIsoCode = "SUR",
                    NumericIsoCode = 740,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Svalbard and Jan Mayen Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SJ",
                    ThreeLetterIsoCode = "SJM",
                    NumericIsoCode = 744,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Swaziland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SZ",
                    ThreeLetterIsoCode = "SWZ",
                    NumericIsoCode = 748,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Syrian Arab Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SY",
                    ThreeLetterIsoCode = "SYR",
                    NumericIsoCode = 760,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Tajikistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TJ",
                    ThreeLetterIsoCode = "TJK",
                    NumericIsoCode = 762,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Tanzania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TZ",
                    ThreeLetterIsoCode = "TZA",
                    NumericIsoCode = 834,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Togo",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TG",
                    ThreeLetterIsoCode = "TGO",
                    NumericIsoCode = 768,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Tokelau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TK",
                    ThreeLetterIsoCode = "TKL",
                    NumericIsoCode = 772,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Tonga",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TO",
                    ThreeLetterIsoCode = "TON",
                    NumericIsoCode = 776,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Trinidad and Tobago",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TT",
                    ThreeLetterIsoCode = "TTO",
                    NumericIsoCode = 780,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Tunisia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TN",
                    ThreeLetterIsoCode = "TUN",
                    NumericIsoCode = 788,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Turkmenistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TM",
                    ThreeLetterIsoCode = "TKM",
                    NumericIsoCode = 795,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Turks and Caicos Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TC",
                    ThreeLetterIsoCode = "TCA",
                    NumericIsoCode = 796,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Tuvalu",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TV",
                    ThreeLetterIsoCode = "TUV",
                    NumericIsoCode = 798,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Uganda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UG",
                    ThreeLetterIsoCode = "UGA",
                    NumericIsoCode = 800,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Vanuatu",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VU",
                    ThreeLetterIsoCode = "VUT",
                    NumericIsoCode = 548,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Vatican City State (Holy See)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VA",
                    ThreeLetterIsoCode = "VAT",
                    NumericIsoCode = 336,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Viet Nam",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VN",
                    ThreeLetterIsoCode = "VNM",
                    NumericIsoCode = 704,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Virgin Islands (British)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VG",
                    ThreeLetterIsoCode = "VGB",
                    NumericIsoCode = 92,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Virgin Islands (U.S.)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VI",
                    ThreeLetterIsoCode = "VIR",
                    NumericIsoCode = 850,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Wallis and Futuna Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "WF",
                    ThreeLetterIsoCode = "WLF",
                    NumericIsoCode = 876,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Western Sahara",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EH",
                    ThreeLetterIsoCode = "ESH",
                    NumericIsoCode = 732,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Yemen",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "YE",
                    ThreeLetterIsoCode = "YEM",
                    NumericIsoCode = 887,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Zambia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ZM",
                    ThreeLetterIsoCode = "ZMB",
                    NumericIsoCode = 894,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
                new Country
                {
                    Name = "Zimbabwe",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ZW",
                    ThreeLetterIsoCode = "ZWE",
                    NumericIsoCode = 716,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = false
                },
            };
            _countryRepository.Insert(countries);
        }

        protected virtual void InstallShippingMethods()
        {
            var shippingMethods = new List<ShippingMethod>
            {
                new ShippingMethod
                {
                    Name = "Terrestre",
                    Description =
                        "Envío de productos por tierra.",
                    DisplayOrder = 1
                }
            };
            _shippingMethodRepository.Insert(shippingMethods);
        }

        protected virtual void InstallDeliveryDates()
        {
            var deliveryDates = new List<DeliveryDate>
            {
                new DeliveryDate
                {
                    Name = "1-2 días",
                    DisplayOrder = 1
                },
                new DeliveryDate
                {
                    Name = "3-5 días",
                    DisplayOrder = 5
                },
                new DeliveryDate
                {
                    Name = "1 semana",
                    DisplayOrder = 10
                },
            };
            _deliveryDateRepository.Insert(deliveryDates);
        }

        protected virtual void InstallProductAvailabilityRanges()
        {
            var productAvailabilityRanges = new List<ProductAvailabilityRange>
            {
                new ProductAvailabilityRange
                {
                    Name = "2-4 días",
                    DisplayOrder = 1
                },
                new ProductAvailabilityRange
                {
                    Name = "7-10 días",
                    DisplayOrder = 2
                },
                new ProductAvailabilityRange
                {
                    Name = "2 semanas",
                    DisplayOrder = 3
                },
            };
            _productAvailabilityRangeRepository.Insert(productAvailabilityRanges);
        }

        protected virtual void InstallCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var crAdministrators = new CustomerRole
            {
                Name = "Administrador",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Administrators,
            };
            var crForumModerators = new CustomerRole
            {
                Name = "Moderador del Foro",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.ForumModerators,
            };
            var crRegistered = new CustomerRole
            {
                Name = "Registrado",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered,
            };
            var crGuests = new CustomerRole
            {
                Name = "Invitado",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Guests,
            };
            var crVendors = new CustomerRole
            {
                Name = "Vendedor",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Vendors,
            };
            var customerRoles = new List<CustomerRole>
            {
                crAdministrators,
                crForumModerators,
                crRegistered,
                crGuests,
                crVendors
            };
            _customerRoleRepository.Insert(customerRoles);

            //default store 
            var defaultStore = _storeRepository.Table.FirstOrDefault();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //admin user
            var adminUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            var defaultAdminUserAddress = new Address
            {
                FirstName = "Cesar",
                LastName = "Martinez",
                PhoneNumber = "5576683844",
                Email = defaultUserEmail,
                FaxNumber = "",
                Company = "Teed Innovación Tecnológica",
                Address1 = "World Trade Center",
                Address2 = "Piso 21 Oficina 36",
                City = "Benito Juárez",
                StateProvince = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "Ciudad de México"),
                Country = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "MEX"),
                ZipPostalCode = "03810",
                CreatedOnUtc = DateTime.UtcNow,
            };
            adminUser.Addresses.Add(defaultAdminUserAddress);
            adminUser.BillingAddress = defaultAdminUserAddress;
            adminUser.ShippingAddress = defaultAdminUserAddress;

            adminUser.CustomerRoles.Add(crAdministrators);
            adminUser.CustomerRoles.Add(crForumModerators);
            adminUser.CustomerRoles.Add(crRegistered);

            _customerRepository.Insert(adminUser);
            //set default customer name
            _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.FirstName, "Cesar");
            _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.LastName, "Martinez");

            //set hashed admin password
            var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword));

            //search engine (crawler) built-in user
            var searchEngineUser = new Customer
            {
                Email = "builtin@search_engine_record.com",
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "Built-in system guest record used for requests from search engines.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.SearchEngine,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            searchEngineUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(searchEngineUser);


            //built-in user for background tasks
            var backgroundTaskUser = new Customer
            {
                Email = "builtin@background-task-record.com",
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "Built-in system record used for background tasks.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.BackgroundTask,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            backgroundTaskUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(backgroundTaskUser);
        }

        protected virtual void InstallOrders()
        {
            //default store
            var defaultStore = _storeRepository.Table.FirstOrDefault();
            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            //first order
            //var firstCustomer = _customerRepository.Table.First(c => c.Email.Equals("steve_gates@nopCommerce.com"));
            //var firstOrder = new Order()
            //{
            //    StoreId = defaultStore.Id,
            //    OrderGuid = Guid.NewGuid(),
            //    Customer = firstCustomer,
            //    CustomerLanguageId = _languageRepository.Table.First().Id,
            //    CustomerIp = "127.0.0.1",
            //    OrderSubtotalInclTax = 1855M,
            //    OrderSubtotalExclTax = 1855M,
            //    OrderSubTotalDiscountInclTax = decimal.Zero,
            //    OrderSubTotalDiscountExclTax = decimal.Zero,
            //    OrderShippingInclTax = decimal.Zero,
            //    OrderShippingExclTax = decimal.Zero,
            //    PaymentMethodAdditionalFeeInclTax = decimal.Zero,
            //    PaymentMethodAdditionalFeeExclTax = decimal.Zero,
            //    TaxRates = "0:0;",
            //    OrderTax = decimal.Zero,
            //    OrderTotal = 1855M,
            //    RefundedAmount = decimal.Zero,
            //    OrderDiscount = decimal.Zero,
            //    CheckoutAttributeDescription = string.Empty,
            //    CheckoutAttributesXml = string.Empty,
            //    CustomerCurrencyCode = "USD",
            //    CurrencyRate = 1M,
            //    AffiliateId = 0,
            //    OrderStatus = OrderStatus.Processing,
            //    AllowStoringCreditCardNumber = false,
            //    CardType = string.Empty,
            //    CardName = string.Empty,
            //    CardNumber = string.Empty,
            //    MaskedCreditCardNumber = string.Empty,
            //    CardCvv2 = string.Empty,
            //    CardExpirationMonth = string.Empty,
            //    CardExpirationYear = string.Empty,
            //    PaymentMethodSystemName = "Payments.CheckMoneyOrder",
            //    AuthorizationTransactionId = string.Empty,
            //    AuthorizationTransactionCode = string.Empty,
            //    AuthorizationTransactionResult = string.Empty,
            //    CaptureTransactionId = string.Empty,
            //    CaptureTransactionResult = string.Empty,
            //    SubscriptionTransactionId = string.Empty,
            //    PaymentStatus = PaymentStatus.Paid,
            //    PaidDateUtc = DateTime.UtcNow,
            //    BillingAddress = (Address)firstCustomer.BillingAddress.Clone(),
            //    ShippingAddress = (Address)firstCustomer.ShippingAddress.Clone(),
            //    ShippingStatus = ShippingStatus.NotYetShipped,
            //    ShippingMethod = "Ground",
            //    PickUpInStore = false,
            //    ShippingRateComputationMethodSystemName = "Shipping.FixedOrByWeight",
            //    CustomValuesXml = string.Empty,
            //    VatNumber = string.Empty,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    CustomOrderNumber = string.Empty
            //};
            //_orderRepository.Insert(firstOrder);
            //firstOrder.CustomOrderNumber = firstOrder.Id.ToString();
            //_orderRepository.Update(firstOrder);

            ////item Apple iCam
            //var firstOrderItem1 = new OrderItem()
            //{
            //    OrderItemGuid = Guid.NewGuid(),
            //    Order = firstOrder,
            //    ProductId = _productRepository.Table.First(p => p.Name.Equals("Apple iCam")).Id,
            //    UnitPriceInclTax = 1300M,
            //    UnitPriceExclTax = 1300M,
            //    PriceInclTax = 1300M,
            //    PriceExclTax = 1300M,
            //    OriginalProductCost = decimal.Zero,
            //    AttributeDescription = string.Empty,
            //    AttributesXml = string.Empty,
            //    Quantity = 1,
            //    DiscountAmountInclTax = decimal.Zero,
            //    DiscountAmountExclTax = decimal.Zero,
            //    DownloadCount = 0,
            //    IsDownloadActivated = false,
            //    LicenseDownloadId = 0,
            //    ItemWeight = null,
            //    RentalStartDateUtc = null,
            //    RentalEndDateUtc = null
            //};
            //_orderItemRepository.Insert(firstOrderItem1);

            ////item Leica T Mirrorless Digital Camera
            //var fierstOrderItem2 = new OrderItem()
            //{
            //    OrderItemGuid = Guid.NewGuid(),
            //    Order = firstOrder,
            //    ProductId = _productRepository.Table.First(p => p.Name.Equals("Leica T Mirrorless Digital Camera")).Id,
            //    UnitPriceInclTax = 530M,
            //    UnitPriceExclTax = 530M,
            //    PriceInclTax = 530M,
            //    PriceExclTax = 530M,
            //    OriginalProductCost = decimal.Zero,
            //    AttributeDescription = string.Empty,
            //    AttributesXml = string.Empty,
            //    Quantity = 1,
            //    DiscountAmountInclTax = decimal.Zero,
            //    DiscountAmountExclTax = decimal.Zero,
            //    DownloadCount = 0,
            //    IsDownloadActivated = false,
            //    LicenseDownloadId = 0,
            //    ItemWeight = null,
            //    RentalStartDateUtc = null,
            //    RentalEndDateUtc = null
            //};
            //_orderItemRepository.Insert(fierstOrderItem2);

            ////item $25 Virtual Gift Card
            //var firstOrderItem3 = new OrderItem()
            //{
            //    OrderItemGuid = Guid.NewGuid(),
            //    Order = firstOrder,
            //    ProductId = _productRepository.Table.First(p => p.Name.Equals("$25 Virtual Gift Card")).Id,
            //    UnitPriceInclTax = 25M,
            //    UnitPriceExclTax = 25M,
            //    PriceInclTax = 25M,
            //    PriceExclTax = 25M,
            //    OriginalProductCost = decimal.Zero,
            //    AttributeDescription = "From: Steve Gates &lt;steve_gates@nopCommerce.com&gt;<br />For: Brenda Lindgren &lt;brenda_lindgren@nopCommerce.com&gt;",
            //    AttributesXml = "<Attributes><GiftCardInfo><RecipientName>Brenda Lindgren</RecipientName><RecipientEmail>brenda_lindgren@nopCommerce.com</RecipientEmail><SenderName>Steve Gates</SenderName><SenderEmail>steve_gates@gmail.com</SenderEmail><Message></Message></GiftCardInfo></Attributes>",
            //    Quantity = 1,
            //    DiscountAmountInclTax = decimal.Zero,
            //    DiscountAmountExclTax = decimal.Zero,
            //    DownloadCount = 0,
            //    IsDownloadActivated = false,
            //    LicenseDownloadId = 0,
            //    ItemWeight = null,
            //    RentalStartDateUtc = null,
            //    RentalEndDateUtc = null
            //};
            //_orderItemRepository.Insert(firstOrderItem3);

            //var firstOrderGiftcard = new GiftCard
            //{
            //    GiftCardType = GiftCardType.Virtual,
            //    PurchasedWithOrderItem = firstOrderItem3,
            //    Amount = 25M,
            //    IsGiftCardActivated = false,
            //    GiftCardCouponCode = string.Empty,
            //    RecipientName = "Brenda Lindgren",
            //    RecipientEmail = "brenda_lindgren@nopCommerce.com",
            //    SenderName = "Steve Gates",
            //    SenderEmail = "steve_gates@nopCommerce.com",
            //    Message = string.Empty,
            //    IsRecipientNotified = false,
            //    CreatedOnUtc = DateTime.UtcNow
            //};
            //_giftCardRepository.Insert(firstOrderGiftcard);

            ////order notes
            //_orderNoteRepository.Insert(new OrderNote()
            //{
            //    CreatedOnUtc = DateTime.UtcNow,
            //    Note = "Order placed",
            //    Order = firstOrder
            //});
            //_orderNoteRepository.Insert(new OrderNote()
            //{
            //    CreatedOnUtc = DateTime.UtcNow,
            //    Note = "Order paid",
            //    Order = firstOrder
            //});
        }

        protected virtual void InstallActivityLog(string defaultUserEmail)
        {
            //default customer/user
            var defaultCustomer = _customerRepository.Table.FirstOrDefault(x => x.Email == defaultUserEmail);
            if (defaultCustomer == null)
                throw new Exception("Cannot load default customer");
        }

        protected virtual void InstallSearchTerms()
        {
            //default store
            var defaultStore = _storeRepository.Table.FirstOrDefault();
            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            _searchTermRepository.Insert(new SearchTerm()
            {
                Count = 34,
                Keyword = "computer",
                StoreId = defaultStore.Id
            });
            _searchTermRepository.Insert(new SearchTerm()
            {
                Count = 30,
                Keyword = "camera",
                StoreId = defaultStore.Id
            });
            _searchTermRepository.Insert(new SearchTerm()
            {
                Count = 27,
                Keyword = "jewelry",
                StoreId = defaultStore.Id
            });
            _searchTermRepository.Insert(new SearchTerm()
            {
                Count = 26,
                Keyword = "shoes",
                StoreId = defaultStore.Id
            });
            _searchTermRepository.Insert(new SearchTerm()
            {
                Count = 19,
                Keyword = "jeans",
                StoreId = defaultStore.Id
            });
            _searchTermRepository.Insert(new SearchTerm()
            {
                Count = 10,
                Keyword = "gift",
                StoreId = defaultStore.Id
            });
        }

        protected virtual void InstallEmailAccounts()
        {
            var emailAccounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    Email = "postmaster@teedapps.com",
                    DisplayName = "TeedCommerce",
                    Host = "smtp.mailgun.org",
                    Port = 587,
                    Username = "postmaster@teedapps.com",
                    Password = "14be341078f9a429ba1b20eda819fdcf-115fe3a6-4463b511",
                    EnableSsl = true,
                    UseDefaultCredentials = false
                },
            };
            _emailAccountRepository.Insert(emailAccounts);
        }

        protected virtual void InstallMessageTemplates()
        {
            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");

            var messageTemplates = new List<MessageTemplate>
            {
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.BlogCommentNotification,
                    Subject = "%Store.Name%. New blog comment.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new blog comment has been created for blog post \"%BlogComment.BlogPostTitle%\".{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.BackInStockNotification,
                    Subject = "%Store.Name%. Back in stock notification",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}Product <a target=\"_blank\" href=\"%BackInStockSubscription.ProductUrl%\">%BackInStockSubscription.ProductName%</a> is in stock.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerEmailValidationMessage,
                    Subject = "%Store.Name%. Email validation",
                    Body = $"<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}To activate your account <a href=\"%Customer.AccountActivationURL%\">click here</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Store.Name%{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerEmailRevalidationMessage,
                    Subject = "%Store.Name%. Email validation",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%!{Environment.NewLine}<br />{Environment.NewLine}To validate your new email address <a href=\"%Customer.EmailRevalidationURL%\">click here</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Store.Name%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.PrivateMessageNotification,
                    Subject = "%Store.Name%. You have received a new private message",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}You have received a new private message.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerPasswordRecoveryMessage,
                    Subject = "%Store.Name%. Password recovery",
                    Body = $"<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}To change your password <a href=\"%Customer.PasswordRecoveryURL%\">click here</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Store.Name%{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerWelcomeMessage,
                    Subject = "Welcome to %Store.Name%",
                    Body = $"We welcome you to <a href=\"%Store.URL%\"> %Store.Name%</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}You can now take part in the various services we have to offer you. Some of these services include:{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.{Environment.NewLine}<br />{Environment.NewLine}Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.{Environment.NewLine}<br />{Environment.NewLine}Order History - View your history of purchases that you have made with us.{Environment.NewLine}<br />{Environment.NewLine}Products Reviews - Share your opinions on products with our other customers.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}For help with any of our online services, please email the store-owner: <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Note: This email address was provided on our registration page. If you own the email and did not register on our site, please send an email to <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewForumPostMessage,
                    Subject = "%Store.Name%. New Post Notification.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new post has been created in the topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Click <a href=\"%Forums.TopicURL%\">here</a> for more info.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Post author: %Forums.PostAuthor%{Environment.NewLine}<br />{Environment.NewLine}Post body: %Forums.PostBody%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewForumTopicMessage,
                    Subject = "%Store.Name%. New Topic Notification.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> has been created at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Click <a href=\"%Forums.TopicURL%\">here</a> for more info.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.GiftCardNotification,
                    Subject = "%GiftCard.SenderName% has sent you a gift card for %Store.Name%",
                    Body = $"<p>{Environment.NewLine}You have received a gift card for %Store.Name%{Environment.NewLine}</p>{Environment.NewLine}<p>{Environment.NewLine}Dear %GiftCard.RecipientName%,{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%GiftCard.SenderName% (%GiftCard.SenderEmail%) has sent you a %GiftCard.Amount% gift cart for <a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}</p>{Environment.NewLine}<p>{Environment.NewLine}You gift card code is %GiftCard.CouponCode%{Environment.NewLine}</p>{Environment.NewLine}<p>{Environment.NewLine}%GiftCard.Message%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerRegisteredNotification,
                    Subject = "%Store.Name%. New customer registration",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new customer registered with your store. Below are the customer's details:{Environment.NewLine}<br />{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}<br />{Environment.NewLine}Email: %Customer.Email%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification,
                    Subject = "%Store.Name%. New return request.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Customer.FullName% has just submitted a new return request. Details are below:{Environment.NewLine}<br />{Environment.NewLine}Request ID: %ReturnRequest.CustomNumber%{Environment.NewLine}<br />{Environment.NewLine}Product: %ReturnRequest.Product.Quantity% x Product: %ReturnRequest.Product.Name%{Environment.NewLine}<br />{Environment.NewLine}Reason for return: %ReturnRequest.Reason%{Environment.NewLine}<br />{Environment.NewLine}Requested action: %ReturnRequest.RequestedAction%{Environment.NewLine}<br />{Environment.NewLine}Customer comments:{Environment.NewLine}<br />{Environment.NewLine}%ReturnRequest.CustomerComment%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewReturnRequestCustomerNotification,
                    Subject = "%Store.Name%. New return request.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%!{Environment.NewLine}<br />{Environment.NewLine}You have just submitted a new return request. Details are below:{Environment.NewLine}<br />{Environment.NewLine}Request ID: %ReturnRequest.CustomNumber%{Environment.NewLine}<br />{Environment.NewLine}Product: %ReturnRequest.Product.Quantity% x Product: %ReturnRequest.Product.Name%{Environment.NewLine}<br />{Environment.NewLine}Reason for return: %ReturnRequest.Reason%{Environment.NewLine}<br />{Environment.NewLine}Requested action: %ReturnRequest.RequestedAction%{Environment.NewLine}<br />{Environment.NewLine}Customer comments:{Environment.NewLine}<br />{Environment.NewLine}%ReturnRequest.CustomerComment%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewsCommentNotification,
                    Subject = "%Store.Name%. New news comment.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new news comment has been created for news \"%NewsComment.NewsTitle%\".{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage,
                    Subject = "%Store.Name%. Subscription activation message.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%NewsLetterSubscription.ActivationUrl%\">Click here to confirm your subscription to our list.</a>{Environment.NewLine}</p>{Environment.NewLine}<p>{Environment.NewLine}If you received this email by mistake, simply delete it.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage,
                    Subject = "%Store.Name%. Subscription deactivation message.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%NewsLetterSubscription.DeactivationUrl%\">Click here to unsubscribe from our newsletter.</a>{Environment.NewLine}</p>{Environment.NewLine}<p>{Environment.NewLine}If you received this email by mistake, simply delete it.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewVatSubmittedStoreOwnerNotification,
                    Subject = "%Store.Name%. New VAT number is submitted.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Customer.FullName% (%Customer.Email%) has just submitted a new VAT number. Details are below:{Environment.NewLine}<br />{Environment.NewLine}VAT number: %Customer.VatNumber%{Environment.NewLine}<br />{Environment.NewLine}VAT number status: %Customer.VatNumberStatus%{Environment.NewLine}<br />{Environment.NewLine}Received name: %VatValidationResult.Name%{Environment.NewLine}<br />{Environment.NewLine}Received address: %VatValidationResult.Address%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderCancelledCustomerNotification,
                    Subject = "%Store.Name%. Your order cancelled",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your order has been cancelled. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderCompletedCustomerNotification,
                    Subject = "%Store.Name%. Your order completed",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your order has been completed. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ShipmentDeliveredCustomerNotification,
                    Subject = "Your order from %Store.Name% has been delivered.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Good news! You order has been delivered.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% Delivered Products:{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Shipment.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderPlacedCustomerNotification,
                    Subject = "Order receipt from %Store.Name%.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderPlacedStoreOwnerNotification,
                    Subject = "%Store.Name%. Purchase Receipt for Order #%Order.OrderNumber%",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ShipmentSentCustomerNotification,
                    Subject = "Your order from %Store.Name% has been shipped.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}<br />{Environment.NewLine}Good news! You order has been shipped.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% Shipped Products:{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Shipment.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ProductReviewNotification,
                    Subject = "%Store.Name%. New product review.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new product review has been written for product \"%ProductReview.ProductName%\".{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.QuantityBelowStoreOwnerNotification,
                    Subject = "%Store.Name%. Quantity below notification. %Product.Name%",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Product.Name% (ID: %Product.ID%) low quantity.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Quantity: %Product.StockQuantity%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.QuantityBelowAttributeCombinationStoreOwnerNotification,
                    Subject = "%Store.Name%. Quantity below notification. %Product.Name%",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Product.Name% (ID: %Product.ID%) low quantity.{Environment.NewLine}<br />{Environment.NewLine}%AttributeCombination.Formatted%{Environment.NewLine}<br />{Environment.NewLine}Quantity: %AttributeCombination.StockQuantity%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ReturnRequestStatusChangedCustomerNotification,
                    Subject = "%Store.Name%. Return request status was changed.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}Your return request #%ReturnRequest.CustomNumber% status has been changed.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.EmailAFriendMessage,
                    Subject = "%Store.Name%. Referred Item",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%EmailAFriend.Email% was shopping on %Store.Name% and wanted to share the following item with you.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<b><a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">%Product.Name%</a></b>{Environment.NewLine}<br />{Environment.NewLine}%Product.ShortDescription%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}For more info click <a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">here</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%EmailAFriend.PersonalMessage%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Store.Name%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.WishlistToFriendMessage,
                    Subject = "%Store.Name%. Wishlist",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Wishlist.Email% was shopping on %Store.Name% and wanted to share a wishlist with you.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}For more info click <a target=\"_blank\" href=\"%Wishlist.URLForCustomer%\">here</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Wishlist.PersonalMessage%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Store.Name%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewOrderNoteAddedCustomerNotification,
                    Subject = "%Store.Name%. New order note has been added",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}New order note has been added to your account:{Environment.NewLine}<br />{Environment.NewLine}\"%Order.NewNoteText%\".{Environment.NewLine}<br />{Environment.NewLine}<a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RecurringPaymentCancelledStoreOwnerNotification,
                    Subject = "%Store.Name%. Recurring payment cancelled",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%RecurringPayment.CancelAfterFailedPayment%) The last payment for the recurring payment ID=%RecurringPayment.ID% failed, so it was cancelled. endif% %if (!%RecurringPayment.CancelAfterFailedPayment%) %Customer.FullName% (%Customer.Email%) has just cancelled a recurring payment ID=%RecurringPayment.ID%. endif%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RecurringPaymentCancelledCustomerNotification,
                    Subject = "%Store.Name%. Recurring payment cancelled",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}%if (%RecurringPayment.CancelAfterFailedPayment%) It appears your credit card didn't go through for this recurring payment (<a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>){Environment.NewLine}<br />{Environment.NewLine}So your subscription has been canceled. endif% %if (!%RecurringPayment.CancelAfterFailedPayment%) The recurring payment ID=%RecurringPayment.ID% was cancelled. endif%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RecurringPaymentFailedCustomerNotification,
                    Subject = "%Store.Name%. Last recurring payment failed",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}It appears your credit card didn't go through for this recurring payment (<a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>){Environment.NewLine}<br /> %if (%RecurringPayment.RecurringPaymentType% == \"Manual\") {Environment.NewLine}You can recharge balance and manually retry payment or cancel it on the order history page. endif% %if (%RecurringPayment.RecurringPaymentType% == \"Automatic\") {Environment.NewLine}You can recharge balance and wait, we will try to make the payment again, or you can cancel it on the order history page. endif%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderPlacedVendorNotification,
                    Subject = "%Store.Name%. Order placed",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Customer.FullName% (%Customer.Email%) has just placed an order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderRefundedCustomerNotification,
                    Subject = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Order #%Order.OrderNumber% has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Amount refunded: %Order.AmountRefunded%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br /{Environment.NewLine}>Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderRefundedStoreOwnerNotification,
                    Subject = "%Store.Name%. Order #%Order.OrderNumber% refunded",
                    Body = $"%Store.Name%. Order #%Order.OrderNumber% refunded', N'{Environment.NewLine}<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order #%Order.OrderNumber% has been just refunded{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Amount refunded: %Order.AmountRefunded%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}</p>{Environment.NewLine}",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderPaidStoreOwnerNotification,
                    Subject = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order #%Order.OrderNumber% has been just paid{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}</p>{Environment.NewLine}",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderPaidCustomerNotification,
                    Subject = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Order #%Order.OrderNumber% has been just paid. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.OrderPaidVendorNotification,
                    Subject = "%Store.Name%. Order #%Order.OrderNumber% paid",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order #%Order.OrderNumber% has been just paid.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    //this template is disabled by default
                    IsActive = false,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.NewVendorAccountApplyStoreOwnerNotification,
                    Subject = "%Store.Name%. New vendor account submitted.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Customer.FullName% (%Customer.Email%) has just submitted for a vendor account. Details are below:{Environment.NewLine}<br />{Environment.NewLine}Vendor name: %Vendor.Name%{Environment.NewLine}<br />{Environment.NewLine}Vendor email: %Vendor.Email%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}You can activate it in admin area.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.VendorInformationChangeNotification,
                    Subject = "%Store.Name%. Vendor information change.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Vendor %Vendor.Name% (%Vendor.Email%) has just changed information about itself.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ContactUsMessage,
                    Subject = "%Store.Name%. Contact us",
                    Body = $"<p>{Environment.NewLine}%ContactUs.Body%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ContactVendorMessage,
                    Subject = "%Store.Name%. Contact us",
                    Body = $"<p>{Environment.NewLine}%ContactUs.Body%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerNewShareCode,
                    Subject = "%Store.Name%. ¡Comparte tu código y gana!",
                    Body = $"<p>{Environment.NewLine}Comparte el código con tus amigos y gana una %Discount.Amount% MXN. Tu nuevo código es:  %Customer.NewShareCode%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerNewCouponCode,
                    Subject = "%Store.Name%. ¡Tienes un nuevo código de descuento!",
                    Body = $"<p>{Environment.NewLine}¡Tienes un nuevo cupón!{Environment.NewLine}</p><p>{Environment.NewLine}%Discount.DiscountReason%{Environment.NewLine}</p><p>{Environment.NewLine}<strong>Tu código:</strong> %Discount.Code%<br />{Environment.NewLine}</p><p>{Environment.NewLine}<strong>Condiciones:</strong><br /><div>%Discount.DiscountConditions%</div>{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id,
                }
            };
            _messageTemplateRepository.Insert(messageTemplates);
        }

        protected virtual void InstallTopics()
        {
            var defaultTopicTemplate =
                _topicTemplateRepository.Table.FirstOrDefault(tt => tt.Name == "Default template");
            if (defaultTopicTemplate == null)
                throw new Exception("Topic template cannot be loaded");

            var topics = new List<Topic>
            {
                new Topic
                {
                    SystemName = "AboutUs",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    IncludeInFooterColumn1 = true,
                    DisplayOrder = 20,
                    Published = true,
                    Title = "Quiénes Somos",
                    Body =
                        "<p>Coloca aquí la información de la página &quot;Quiénes Somos&quot;. Puedes editar esta información desde el panel de administración.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "CheckoutAsGuestOrRegister",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "",
                    Body =
                        "<p><strong>¡Regístrate y ahorra tiempo!</strong><br />Regístrate para obtener los siguientes beneficios:</p><ul><li>Pagos más rápidos.</li><li>Fácil acceso a tu historial de compras.</li></ul>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "ConditionsOfUse",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    IncludeInFooterColumn1 = true,
                    DisplayOrder = 15,
                    Published = true,
                    Title = "Términos y Condiciones",
                    Body = "<p>Escribe aquí las condiciones de uso del sitio.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "ContactUs",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "",
                    Body = "<p>Escribe aquí la información de contacto.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "ForumWelcomeMessage",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "Foros",
                    Body = "<p>Coloca aquí el mensaje de bienvenida al foro.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "HomePageText",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "¡Bienvenido a nuestra tienda!",
                    Body =
                        "<p>Esta es una nueva tienda desarrollada por Teed Innovaci&oacute;n Tecnol&oacute;gica. Puedes editar esta informaci&oacute;n desde el panel de administraci&oacute;n.&nbsp;</p><p>&iexcl;Suerte con el proyecto!</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "LoginRegistrationInfo",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "Sobre registro / ininio de sesión",
                    Body =
                        "<p>Coloca aquí la información de inicio de sesión / registro</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "PrivacyInfo",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    IncludeInFooterColumn1 = true,
                    DisplayOrder = 10,
                    Published = true,
                    Title = "Aviso de Privacidad",
                    Body = "<p>Coloca aquí tu aviso de privacidad.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "PageNotFound",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "",
                    Body =
                        "<p><strong>&iexcl;P&aacute;gina no encontrada!</strong></p><p>Lo sentimos, pero la direcci&oacute;n web que has especificado no es una p&aacute;gina activa de nuestro sitio.</p><p>&iexcl;Pero no te preocupes! Te invitamos a seguir navegando y ver nuestra gran variedad de productos.</p><p><a href='/'>Volver a la p&aacute;gina principal</a></p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "ShippingInfo",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    IncludeInFooterColumn1 = true,
                    DisplayOrder = 5,
                    Published = true,
                    Title = "Envíos y devoluciones",
                    Body =
                        "<p>Coloca aquí la política de envíos y devoluciones.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                }
            };
            _topicRepository.Insert(topics);

            //search engine names
            foreach (var topic in topics)
            {
                _urlRecordRepository.Insert(new UrlRecord
                {
                    EntityId = topic.Id,
                    EntityName = "Topic",
                    LanguageId = 0,
                    IsActive = true,
                    Slug = topic.ValidateSeName("", !string.IsNullOrEmpty(topic.Title) ? topic.Title : topic.SystemName, true)
                });
            }
        }

        protected virtual void InstallSettings(bool installSampleData)
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            settingService.SaveSetting(new PdfSettings
            {
                LogoPictureId = 0,
                LetterPageSizeEnabled = false,
                RenderOrderNotes = true,
                FontFileName = "FreeSerif.ttf",
                InvoiceFooterTextColumn1 = null,
                InvoiceFooterTextColumn2 = null,
            });

            settingService.SaveSetting(new CommonSettings
            {
                UseSystemEmailForContactUsForm = true,
                UseStoredProceduresIfSupported = true,
                UseStoredProcedureForLoadingCategories = false,
                SitemapEnabled = true,
                SitemapIncludeCategories = true,
                SitemapIncludeManufacturers = true,
                SitemapIncludeProducts = false,
                SitemapIncludeProductTags = false,
                DisplayJavaScriptDisabledWarning = false,
                UseFullTextSearch = false,
                FullTextMode = FulltextSearchMode.ExactMatch,
                Log404Errors = true,
                BreadcrumbDelimiter = "/",
                RenderXuaCompatible = false,
                XuaCompatibleValue = "IE=edge",
                BbcodeEditorOpenLinksInNewWindow = false,
                PopupForTermsOfServiceLinks = true
            });

            settingService.SaveSetting(new SeoSettings
            {
                PageTitleSeparator = " - ",
                PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                DefaultTitle = "TeedCommerce",
                DefaultMetaKeywords = "",
                DefaultMetaDescription = "",
                GenerateProductMetaDescription = true,
                ConvertNonWesternChars = false,
                AllowUnicodeCharsInUrls = true,
                CanonicalUrlsEnabled = false,
                QueryStringInCanonicalUrlsEnabled = false,
                WwwRequirement = WwwRequirement.NoMatter,
                //we disable bundling out of the box because it requires a lot of server resources
                EnableJsBundling = false,
                EnableCssBundling = false,
                TwitterMetaTags = true,
                OpenGraphMetaTags = true,
                ReservedUrlRecordSlugs = new List<string>
                {
                    "admin",
                    "install",
                    "recentlyviewedproducts",
                    "newproducts",
                    "compareproducts",
                    "clearcomparelist",
                    "setproductreviewhelpfulness",
                    "login",
                    "register",
                    "logout",
                    "cart",
                    "wishlist",
                    "emailwishlist",
                    "checkout",
                    "onepagecheckout",
                    "contactus",
                    "passwordrecovery",
                    "subscribenewsletter",
                    "blog",
                    "boards",
                    "inboxupdate",
                    "sentupdate",
                    "news",
                    "sitemap",
                    "search",
                    "config",
                    "eucookielawaccept",
                    "page-not-found",
                    //system names are not allowed (anyway they will cause a runtime error),
                    "con",
                    "lpt1",
                    "lpt2",
                    "lpt3",
                    "lpt4",
                    "lpt5",
                    "lpt6",
                    "lpt7",
                    "lpt8",
                    "lpt9",
                    "com1",
                    "com2",
                    "com3",
                    "com4",
                    "com5",
                    "com6",
                    "com7",
                    "com8",
                    "com9",
                    "null",
                    "prn",
                    "aux"
                },
                CustomHeadTags = ""
            });

            settingService.SaveSetting(new AdminAreaSettings
            {
                DefaultGridPageSize = 15,
                PopupGridPageSize = 10,
                GridPageSizes = "10, 15, 20, 50, 100",
                RichEditorAdditionalSettings = null,
                RichEditorAllowJavaScript = false,
                UseRichEditorInMessageTemplates = false,
                UseIsoDateTimeConverterInJson = true,
                UseNestedSetting = true
            });

            settingService.SaveSetting(new ProductEditorSettings
            {
                Weight = true,
                Dimensions = true,
                ProductAttributes = true,
                SpecificationAttributes = true
            });

            settingService.SaveSetting(new CatalogSettings
            {
                AllowViewUnpublishedProductPage = true,
                DisplayDiscontinuedMessageForUnpublishedProducts = true,
                PublishBackProductWhenCancellingOrders = false,
                ShowSkuOnProductDetailsPage = true,
                ShowSkuOnCatalogPages = false,
                ShowManufacturerPartNumber = false,
                ShowGtin = false,
                ShowFreeShippingNotification = true,
                AllowProductSorting = true,
                AllowProductViewModeChanging = true,
                DefaultViewMode = "grid",
                ShowProductsFromSubcategories = false,
                ShowCategoryProductNumber = false,
                ShowCategoryProductNumberIncludingSubcategories = false,
                CategoryBreadcrumbEnabled = true,
                ShowShareButton = true,
                PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style \"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\" src=\"http://s7.addthis.com/js/250/addthis_widget.js#pubid=nopsolutions\"></script><!-- AddThis Button END -->",
                ProductReviewsMustBeApproved = false,
                DefaultProductRatingValue = 5,
                AllowAnonymousUsersToReviewProduct = false,
                ProductReviewPossibleOnlyAfterPurchasing = false,
                NotifyStoreOwnerAboutNewProductReviews = false,
                EmailAFriendEnabled = true,
                AllowAnonymousUsersToEmailAFriend = false,
                RecentlyViewedProductsNumber = 3,
                RecentlyViewedProductsEnabled = true,
                NewProductsNumber = 6,
                NewProductsEnabled = true,
                CompareProductsEnabled = true,
                CompareProductsNumber = 4,
                ProductSearchAutoCompleteEnabled = true,
                ProductSearchAutoCompleteNumberOfProducts = 10,
                ProductSearchTermMinimumLength = 3,
                ShowProductImagesInSearchAutoComplete = false,
                ShowBestsellersOnHomepage = true,
                NumberOfBestsellersOnHomepage = 4,
                SearchPageProductsPerPage = 6,
                SearchPageAllowCustomersToSelectPageSize = true,
                SearchPagePageSizeOptions = "12, 6, 3, 9, 18",
                ProductsAlsoPurchasedEnabled = true,
                ProductsAlsoPurchasedNumber = 4,
                AjaxProcessAttributeChange = true,
                NumberOfProductTags = 15,
                ProductsByTagPageSize = 6,
                IncludeShortDescriptionInCompareProducts = false,
                IncludeFullDescriptionInCompareProducts = false,
                IncludeFeaturedProductsInNormalLists = false,
                DisplayTierPricesWithDiscounts = true,
                IgnoreDiscounts = false,
                IgnoreFeaturedProducts = false,
                IgnoreAcl = true,
                IgnoreStoreLimitations = true,
                CacheProductPrices = false,
                ProductsByTagAllowCustomersToSelectPageSize = true,
                ProductsByTagPageSizeOptions = "6, 3, 9, 18",
                MaximumBackInStockSubscriptions = 200,
                ManufacturersBlockItemsToDisplay = 2,
                DisplayTaxShippingInfoFooter = false,
                DisplayTaxShippingInfoProductDetailsPage = false,
                DisplayTaxShippingInfoProductBoxes = false,
                DisplayTaxShippingInfoShoppingCart = false,
                DisplayTaxShippingInfoWishlist = false,
                DisplayTaxShippingInfoOrderDetailsPage = false,
                DefaultCategoryPageSizeOptions = "6, 3, 9",
                DefaultCategoryPageSize = 6,
                DefaultManufacturerPageSizeOptions = "6, 3, 9",
                DefaultManufacturerPageSize = 6,
                ShowProductReviewsTabOnAccountPage = true,
                ProductReviewsPageSizeOnAccountPage = 10,
                ExportImportProductAttributes = true,
                ExportImportProductSpecificationAttributes = true,
                ExportImportUseDropdownlistsForAssociatedEntities = true
            });

            settingService.SaveSetting(new LocalizationSettings
            {
                DefaultAdminLanguageId = _languageRepository.Table.Single(l => l.Name == "Español").Id,
                UseImagesForLanguageSelection = false,
                SeoFriendlyUrlsForLanguagesEnabled = false,
                AutomaticallyDetectLanguage = false,
                LoadAllLocaleRecordsOnStartup = true,
                LoadAllLocalizedPropertiesOnStartup = true,
                LoadAllUrlRecordsOnStartup = false,
                IgnoreRtlPropertyForAdminArea = false
            });

            settingService.SaveSetting(new CustomerSettings
            {
                UsernamesEnabled = false,
                CheckUsernameAvailabilityEnabled = false,
                AllowUsersToChangeUsernames = false,
                DefaultPasswordFormat = PasswordFormat.Hashed,
                HashedPasswordFormat = "SHA512",
                PasswordMinLength = 6,
                UnduplicatedPasswordsNumber = 4,
                PasswordRecoveryLinkDaysValid = 7,
                PasswordLifetime = 90,
                FailedPasswordAllowedAttempts = 0,
                FailedPasswordLockoutMinutes = 30,
                UserRegistrationType = UserRegistrationType.Standard,
                AllowCustomersToUploadAvatars = false,
                AvatarMaximumSizeBytes = 20000,
                DefaultAvatarEnabled = true,
                ShowCustomersLocation = false,
                ShowCustomersJoinDate = false,
                AllowViewingProfiles = false,
                NotifyNewCustomerRegistration = false,
                HideDownloadableProductsTab = false,
                HideBackInStockSubscriptionsTab = false,
                DownloadableProductsValidateUser = false,
                CustomerNameFormat = CustomerNameFormat.ShowFirstName,
                GenderEnabled = true,
                DateOfBirthEnabled = true,
                DateOfBirthRequired = false,
                DateOfBirthMinimumAge = null,
                CompanyEnabled = false,
                StreetAddressEnabled = false,
                StreetAddress2Enabled = false,
                ZipPostalCodeEnabled = false,
                CityEnabled = false,
                CountryEnabled = false,
                CountryRequired = false,
                StateProvinceEnabled = false,
                StateProvinceRequired = false,
                PhoneEnabled = false,
                FaxEnabled = false,
                AcceptPrivacyPolicyEnabled = false,
                NewsletterEnabled = true,
                NewsletterTickedByDefault = true,
                HideNewsletterBlock = false,
                NewsletterBlockAllowToUnsubscribe = false,
                OnlineCustomerMinutes = 20,
                StoreLastVisitedPage = false,
                StoreIpAddresses = true,
                SuffixDeletedCustomers = false,
                EnteringEmailTwice = false,
                RequireRegistrationForDownloadableProducts = false,
                DeleteGuestTaskOlderThanMinutes = 1440
            });

            settingService.SaveSetting(new AddressSettings
            {
                CompanyEnabled = false,
                StreetAddressEnabled = true,
                StreetAddressRequired = true,
                StreetAddress2Enabled = true,
                ZipPostalCodeEnabled = true,
                ZipPostalCodeRequired = true,
                CityEnabled = true,
                CityRequired = true,
                CountryEnabled = true,
                StateProvinceEnabled = true,
                PhoneEnabled = true,
                PhoneRequired = true,
                FaxEnabled = true,
            });

            settingService.SaveSetting(new MediaSettings
            {
                AvatarPictureSize = 120,
                ProductThumbPictureSize = 415,
                ProductDetailsPictureSize = 550,
                ProductThumbPictureSizeOnProductDetailsPage = 100,
                AssociatedProductPictureSize = 220,
                CategoryThumbPictureSize = 450,
                ManufacturerThumbPictureSize = 420,
                VendorThumbPictureSize = 450,
                CartThumbPictureSize = 80,
                MiniCartThumbPictureSize = 70,
                AutoCompleteSearchThumbPictureSize = 20,
                ImageSquarePictureSize = 32,
                MaximumImageSize = 1980,
                DefaultPictureZoomEnabled = false,
                DefaultImageQuality = 80,
                MultipleThumbDirectories = false,
                ImportProductImagesUsingHash = true,
                AzureCacheControlHeader = string.Empty
            });

            settingService.SaveSetting(new StoreInformationSettings
            {
                StoreClosed = false,
                DefaultStoreTheme = "TeedMaterial",
                AllowCustomerToSelectTheme = false,
                DisplayMiniProfilerInPublicStore = false,
                DisplayMiniProfilerForAdminOnly = false,
                DisplayEuCookieLawWarning = false,
                FacebookLink = "https://www.facebook.com/TEEDmx/",
                TwitterLink = "https://twitter.com/teedmx",
                YoutubeLink = "https://www.youtube.com/user/TeedTecEdu",
                GooglePlusLink = "",
                HidePoweredByNopCommerce = false
            });

            settingService.SaveSetting(new ExternalAuthenticationSettings
            {
                RequireEmailValidation = false,
                AllowCustomersToRemoveAssociations = true
            });

            settingService.SaveSetting(new RewardPointsSettings
            {
                Enabled = true,
                ExchangeRate = 1,
                PointsForRegistration = 0,
                PointsForPurchases_Amount = 10,
                PointsForPurchases_Points = 1,
                ActivationDelay = 0,
                ActivationDelayPeriodId = 0,
                DisplayHowMuchWillBeEarned = true,
                PointsAccumulatedForAllStores = true,
                PageSize = 10
            });

            settingService.SaveSetting(new CurrencySettings
            {
                DisplayCurrencyLabel = false,
                PrimaryStoreCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "MXN").Id,
                PrimaryExchangeRateCurrencyId = _currencyRepository.Table.Single(c => c.CurrencyCode == "MXN").Id,
                ActiveExchangeRateProviderSystemName = "CurrencyExchange.MoneyConverter",
                AutoUpdateEnabled = false
            });

            settingService.SaveSetting(new MeasureSettings
            {
                BaseDimensionId = _measureDimensionRepository.Table.Single(m => m.SystemKeyword == "inches").Id,
                BaseWeightId = _measureWeightRepository.Table.Single(m => m.SystemKeyword == "lb").Id,
            });

            settingService.SaveSetting(new MessageTemplatesSettings
            {
                CaseInvariantReplacement = false,
                Color1 = "#b9babe",
                Color2 = "#ebecee",
                Color3 = "#dde2e6",
            });

            settingService.SaveSetting(new ShoppingCartSettings
            {
                DisplayCartAfterAddingProduct = false,
                DisplayWishlistAfterAddingProduct = false,
                MaximumShoppingCartItems = 1000,
                MaximumWishlistItems = 1000,
                AllowOutOfStockItemsToBeAddedToWishlist = false,
                MoveItemsFromWishlistToCart = true,
                CartsSharedBetweenStores = false,
                ShowProductImagesOnShoppingCart = true,
                ShowProductImagesOnWishList = true,
                ShowDiscountBox = false,
                ShowGiftCardBox = false,
                CrossSellsNumber = 4,
                EmailWishlistEnabled = true,
                AllowAnonymousUsersToEmailWishlist = false,
                MiniShoppingCartEnabled = true,
                ShowProductImagesInMiniShoppingCart = true,
                MiniShoppingCartProductNumber = 5,
                RoundPricesDuringCalculation = true,
                GroupTierPricesForDistinctShoppingCartItems = false,
                AllowCartItemEditing = true,
                RenderAssociatedAttributeValueQuantity = true
            });

            settingService.SaveSetting(new OrderSettings
            {
                ReturnRequestNumberMask = "{ID}",
                IsReOrderAllowed = true,
                MinOrderSubtotalAmount = 0,
                MinOrderSubtotalAmountIncludingTax = false,
                MinOrderTotalAmount = 0,
                AutoUpdateOrderTotalsOnEditingOrder = false,
                AnonymousCheckoutAllowed = true,
                TermsOfServiceOnShoppingCartPage = true,
                TermsOfServiceOnOrderConfirmPage = false,
                OnePageCheckoutEnabled = true,
                OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab = false,
                DisableBillingAddressCheckoutStep = false,
                DisableOrderCompletedPage = false,
                AttachPdfInvoiceToOrderPlacedEmail = false,
                AttachPdfInvoiceToOrderCompletedEmail = false,
                GeneratePdfInvoiceInCustomerLanguage = true,
                AttachPdfInvoiceToOrderPaidEmail = false,
                ReturnRequestsEnabled = true,
                ReturnRequestsAllowFiles = false,
                ReturnRequestsFileMaximumSize = 2048,
                NumberOfDaysReturnRequestAvailable = 365,
                MinimumOrderPlacementInterval = 30,
                ActivateGiftCardsAfterCompletingOrder = false,
                DeactivateGiftCardsAfterCancellingOrder = false,
                DeactivateGiftCardsAfterDeletingOrder = false,
                CompleteOrderWhenDelivered = true,
                CustomOrderNumberMask = "{ID}",
                ExportWithProducts = true,
                AllowAdminsToBuyCallForPriceProducts = true
            });

            settingService.SaveSetting(new SecuritySettings
            {
                ForceSslForAllPages = true,
                EncryptionKey = CommonHelper.GenerateRandomDigitCode(16),
                AdminAreaAllowedIpAddresses = null,
                EnableXsrfProtectionForAdminArea = true,
                EnableXsrfProtectionForPublicStore = true,
                HoneypotEnabled = false,
                HoneypotInputName = "hpinput"
            });

            settingService.SaveSetting(new ShippingSettings
            {
                ActiveShippingRateComputationMethodSystemNames = new List<string> { "Shipping.FixedOrByWeight" },
                ActivePickupPointProviderSystemNames = new List<string> { "Pickup.PickupInStore" },
                ShipToSameAddress = true,
                AllowPickUpInStore = false,
                DisplayPickupPointsOnMap = false,
                UseWarehouseLocation = false,
                NotifyCustomerAboutShippingFromMultipleLocations = false,
                FreeShippingOverXEnabled = false,
                FreeShippingOverXValue = decimal.Zero,
                FreeShippingOverXIncludingTax = false,
                EstimateShippingEnabled = false,
                DisplayShipmentEventsToCustomers = false,
                DisplayShipmentEventsToStoreOwner = false,
                HideShippingTotal = false,
                ReturnValidOptionsIfThereAreAny = true,
                BypassShippingMethodSelectionIfOnlyOne = false,
                UseCubeRootMethod = true,
                ConsiderAssociatedProductsDimensions = true
            });

            settingService.SaveSetting(new PaymentSettings
            {
                ActivePaymentMethodSystemNames = new List<string>
                    {
                        "Payments.Manual"
                    },
                AllowRePostingPayments = true,
                BypassPaymentMethodSelectionIfOnlyOne = true,
                ShowPaymentMethodDescriptions = true,
                SkipPaymentInfoStepForRedirectionPaymentMethods = false,
                CancelRecurringPaymentsAfterFailedPayment = false
            });

            settingService.SaveSetting(new TaxSettings
            {
                TaxBasedOn = TaxBasedOn.BillingAddress,
                TaxBasedOnPickupPointAddress = false,
                TaxDisplayType = TaxDisplayType.IncludingTax,
                ActiveTaxProviderSystemName = "Tax.FixedOrByCountryStateZip",
                DefaultTaxAddressId = 0,
                DisplayTaxSuffix = false,
                DisplayTaxRates = false,
                PricesIncludeTax = true,
                AllowCustomersToSelectTaxDisplayType = false,
                ForceTaxExclusionFromOrderSubtotal = false,
                DefaultTaxCategoryId = 1,
                HideZeroTax = false,
                HideTaxInOrderSummary = false,
                ShippingIsTaxable = true,
                ShippingPriceIncludesTax = true,
                ShippingTaxClassId = 1,
                PaymentMethodAdditionalFeeIsTaxable = false,
                PaymentMethodAdditionalFeeIncludesTax = false,
                PaymentMethodAdditionalFeeTaxClassId = 0,
                EuVatEnabled = false,
                EuVatShopCountryId = 0,
                EuVatAllowVatExemption = true,
                EuVatUseWebService = false,
                EuVatAssumeValid = false,
                EuVatEmailAdminWhenNewVatSubmitted = false,
                LogErrors = false
            });

            settingService.SaveSetting(new DateTimeSettings
            {
                DefaultStoreTimeZoneId = "",
                AllowCustomersToSetTimeZone = false
            });

            settingService.SaveSetting(new BlogSettings
            {
                Enabled = false,
                PostsPageSize = 10,
                AllowNotRegisteredUsersToLeaveComments = true,
                NotifyAboutNewBlogComments = false,
                NumberOfTags = 15,
                ShowHeaderRssUrl = false,
                BlogCommentsMustBeApproved = false,
                ShowBlogCommentsPerStore = false
            });
            settingService.SaveSetting(new NewsSettings
            {
                Enabled = false,
                AllowNotRegisteredUsersToLeaveComments = true,
                NotifyAboutNewNewsComments = false,
                ShowNewsOnMainPage = true,
                MainPageNewsCount = 3,
                NewsArchivePageSize = 10,
                ShowHeaderRssUrl = false,
                NewsCommentsMustBeApproved = false,
                ShowNewsCommentsPerStore = false
            });

            settingService.SaveSetting(new ForumSettings
            {
                ForumsEnabled = false,
                RelativeDateTimeFormattingEnabled = true,
                AllowCustomersToDeletePosts = false,
                AllowCustomersToEditPosts = false,
                AllowCustomersToManageSubscriptions = false,
                AllowGuestsToCreatePosts = false,
                AllowGuestsToCreateTopics = false,
                AllowPostVoting = true,
                MaxVotesPerDay = 30,
                TopicSubjectMaxLength = 450,
                PostMaxLength = 4000,
                StrippedTopicMaxLength = 45,
                TopicsPageSize = 10,
                PostsPageSize = 10,
                SearchResultsPageSize = 10,
                ActiveDiscussionsPageSize = 50,
                LatestCustomerPostsPageSize = 10,
                ShowCustomersPostCount = true,
                ForumEditor = EditorType.BBCodeEditor,
                SignaturesEnabled = true,
                AllowPrivateMessages = false,
                ShowAlertForPM = false,
                PrivateMessagesPageSize = 10,
                ForumSubscriptionsPageSize = 10,
                NotifyAboutPrivateMessages = false,
                PMSubjectMaxLength = 450,
                PMTextMaxLength = 4000,
                HomePageActiveDiscussionsTopicCount = 5,
                ActiveDiscussionsFeedEnabled = false,
                ActiveDiscussionsFeedCount = 25,
                ForumFeedsEnabled = false,
                ForumFeedCount = 10,
                ForumSearchTermMinimumLength = 3,
            });

            settingService.SaveSetting(new VendorSettings
            {
                DefaultVendorPageSizeOptions = "6, 3, 9",
                VendorsBlockItemsToDisplay = 0,
                ShowVendorOnProductDetailsPage = true,
                AllowCustomersToContactVendors = false,
                AllowCustomersToApplyForVendorAccount = false,
                TermsOfServiceEnabled = false,
                AllowVendorsToEditInfo = false,
                NotifyStoreOwnerAboutVendorInformationChange = true,
                MaximumProductNumber = 3000,
                AllowVendorsToImportProducts = true
            });

            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            settingService.SaveSetting(new EmailAccountSettings
            {
                DefaultEmailAccountId = eaGeneral.Id
            });

            settingService.SaveSetting(new WidgetSettings
            {
                ActiveWidgetSystemNames = new List<string> { "Widgets.NivoSlider" },
            });

            settingService.SaveSetting(new DisplayDefaultMenuItemSettings
            {
                DisplayHomePageMenuItem = !installSampleData,
                DisplayNewProductsMenuItem = !installSampleData,
                DisplayProductSearchMenuItem = !installSampleData,
                DisplayCustomerInfoMenuItem = !installSampleData,
                DisplayBlogMenuItem = !installSampleData,
                DisplayForumsMenuItem = !installSampleData,
                DisplayContactUsMenuItem = !installSampleData
            });
        }

        protected virtual void InstallCheckoutAttributes()
        {

        }

        protected virtual void InstallSpecificationAttributes()
        {

        }

        protected virtual void InstallProductAttributes()
        {

        }

        protected virtual void InstallCategories()
        {

        }

        protected virtual void InstallManufacturers()
        {

        }

        protected virtual void InstallProducts(string defaultUserEmail)
        {

        }

        protected virtual void InstallForums()
        {

        }

        protected virtual void InstallDiscounts()
        {

        }

        protected virtual void InstallBlogPosts(string defaultUserEmail)
        {

        }

        protected virtual void InstallNews(string defaultUserEmail)
        {

        }

        protected virtual void InstallPolls()
        {

        }

        protected virtual void InstallActivityLogTypes()
        {
            var activityLogTypes = new List<ActivityLogType>
            {
                //admin area activities
                new ActivityLogType
                {
                    SystemKeyword = "AddNewAddressAttribute",
                    Enabled = true,
                    Name = "Add a new address attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewAddressAttributeValue",
                    Enabled = true,
                    Name = "Add a new address attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewAffiliate",
                    Enabled = true,
                    Name = "Add a new affiliate"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewBlogPost",
                    Enabled = true,
                    Name = "Add a new blog post"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCampaign",
                    Enabled = true,
                    Name = "Add a new campaign"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCategory",
                    Enabled = true,
                    Name = "Add a new category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCheckoutAttribute",
                    Enabled = true,
                    Name = "Add a new checkout attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCountry",
                    Enabled = true,
                    Name = "Add a new country"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCurrency",
                    Enabled = true,
                    Name = "Add a new currency"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomer",
                    Enabled = true,
                    Name = "Add a new customer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomerAttribute",
                    Enabled = true,
                    Name = "Add a new customer attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomerAttributeValue",
                    Enabled = true,
                    Name = "Add a new customer attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomerRole",
                    Enabled = true,
                    Name = "Add a new customer role"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewDiscount",
                    Enabled = true,
                    Name = "Add a new discount"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewEmailAccount",
                    Enabled = true,
                    Name = "Add a new email account"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewGiftCard",
                    Enabled = true,
                    Name = "Add a new gift card"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewLanguage",
                    Enabled = true,
                    Name = "Add a new language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewManufacturer",
                    Enabled = true,
                    Name = "Add a new manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewMeasureDimension",
                    Enabled = true,
                    Name = "Add a new measure dimension"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewMeasureWeight",
                    Enabled = true,
                    Name = "Add a new measure weight"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewNews",
                    Enabled = true,
                    Name = "Add a new news"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewProduct",
                    Enabled = true,
                    Name = "Add a new product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewProductAttribute",
                    Enabled = true,
                    Name = "Add a new product attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewSetting",
                    Enabled = true,
                    Name = "Add a new setting"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewSpecAttribute",
                    Enabled = true,
                    Name = "Add a new specification attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewStateProvince",
                    Enabled = true,
                    Name = "Add a new state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewStore",
                    Enabled = true,
                    Name = "Add a new store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewTopic",
                    Enabled = true,
                    Name = "Add a new topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewVendor",
                    Enabled = true,
                    Name = "Add a new vendor"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewWarehouse",
                    Enabled = true,
                    Name = "Add a new warehouse"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewWidget",
                    Enabled = true,
                    Name = "Add a new widget"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteActivityLog",
                    Enabled = true,
                    Name = "Delete activity log"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAddressAttribute",
                    Enabled = true,
                    Name = "Delete an address attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAddressAttributeValue",
                    Enabled = true,
                    Name = "Delete an address attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAffiliate",
                    Enabled = true,
                    Name = "Delete an affiliate"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteBlogPost",
                    Enabled = true,
                    Name = "Delete a blog post"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteBlogPostComment",
                    Enabled = true,
                    Name = "Delete a blog post comment"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCampaign",
                    Enabled = true,
                    Name = "Delete a campaign"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCategory",
                    Enabled = true,
                    Name = "Delete category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCheckoutAttribute",
                    Enabled = true,
                    Name = "Delete a checkout attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCountry",
                    Enabled = true,
                    Name = "Delete a country"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCurrency",
                    Enabled = true,
                    Name = "Delete a currency"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomer",
                    Enabled = true,
                    Name = "Delete a customer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomerAttribute",
                    Enabled = true,
                    Name = "Delete a customer attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomerAttributeValue",
                    Enabled = true,
                    Name = "Delete a customer attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomerRole",
                    Enabled = true,
                    Name = "Delete a customer role"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteDiscount",
                    Enabled = true,
                    Name = "Delete a discount"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteEmailAccount",
                    Enabled = true,
                    Name = "Delete an email account"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteGiftCard",
                    Enabled = true,
                    Name = "Delete a gift card"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteLanguage",
                    Enabled = true,
                    Name = "Delete a language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteManufacturer",
                    Enabled = true,
                    Name = "Delete a manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteMeasureDimension",
                    Enabled = true,
                    Name = "Delete a measure dimension"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteMeasureWeight",
                    Enabled = true,
                    Name = "Delete a measure weight"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteMessageTemplate",
                    Enabled = true,
                    Name = "Delete a message template"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteNews",
                    Enabled = true,
                    Name = "Delete a news"
                },
                 new ActivityLogType
                {
                    SystemKeyword = "DeleteNewsComment",
                    Enabled = true,
                    Name = "Delete a news comment"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteOrder",
                    Enabled = true,
                    Name = "Delete an order"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeletePlugin",
                    Enabled = true,
                    Name = "Delete a plugin"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteProduct",
                    Enabled = true,
                    Name = "Delete a product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteProductAttribute",
                    Enabled = true,
                    Name = "Delete a product attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteProductReview",
                    Enabled = true,
                    Name = "Delete a product review"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteReturnRequest",
                    Enabled = true,
                    Name = "Delete a return request"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteSetting",
                    Enabled = true,
                    Name = "Delete a setting"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteSpecAttribute",
                    Enabled = true,
                    Name = "Delete a specification attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteStateProvince",
                    Enabled = true,
                    Name = "Delete a state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteStore",
                    Enabled = true,
                    Name = "Delete a store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteSystemLog",
                    Enabled = true,
                    Name = "Delete system log"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteTopic",
                    Enabled = true,
                    Name = "Delete a topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteVendor",
                    Enabled = true,
                    Name = "Delete a vendor"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteWarehouse",
                    Enabled = true,
                    Name = "Delete a warehouse"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteWidget",
                    Enabled = true,
                    Name = "Delete a widget"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditActivityLogTypes",
                    Enabled = true,
                    Name = "Edit activity log types"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditAddressAttribute",
                    Enabled = true,
                    Name = "Edit an address attribute"
                },
                 new ActivityLogType
                {
                    SystemKeyword = "EditAddressAttributeValue",
                    Enabled = true,
                    Name = "Edit an address attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditAffiliate",
                    Enabled = true,
                    Name = "Edit an affiliate"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditBlogPost",
                    Enabled = true,
                    Name = "Edit a blog post"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCampaign",
                    Enabled = true,
                    Name = "Edit a campaign"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCategory",
                    Enabled = true,
                    Name = "Edit category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCheckoutAttribute",
                    Enabled = true,
                    Name = "Edit a checkout attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCountry",
                    Enabled = true,
                    Name = "Edit a country"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCurrency",
                    Enabled = true,
                    Name = "Edit a currency"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomer",
                    Enabled = true,
                    Name = "Edit a customer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomerAttribute",
                    Enabled = true,
                    Name = "Edit a customer attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomerAttributeValue",
                    Enabled = true,
                    Name = "Edit a customer attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomerRole",
                    Enabled = true,
                    Name = "Edit a customer role"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditDiscount",
                    Enabled = true,
                    Name = "Edit a discount"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditEmailAccount",
                    Enabled = true,
                    Name = "Edit an email account"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditGiftCard",
                    Enabled = true,
                    Name = "Edit a gift card"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditLanguage",
                    Enabled = true,
                    Name = "Edit a language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditManufacturer",
                    Enabled = true,
                    Name = "Edit a manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditMeasureDimension",
                    Enabled = true,
                    Name = "Edit a measure dimension"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditMeasureWeight",
                    Enabled = true,
                    Name = "Edit a measure weight"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditMessageTemplate",
                    Enabled = true,
                    Name = "Edit a message template"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditNews",
                    Enabled = true,
                    Name = "Edit a news"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditOrder",
                    Enabled = true,
                    Name = "Edit an order"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditPlugin",
                    Enabled = true,
                    Name = "Edit a plugin"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditProduct",
                    Enabled = true,
                    Name = "Edit a product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditProductAttribute",
                    Enabled = true,
                    Name = "Edit a product attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditProductReview",
                    Enabled = true,
                    Name = "Edit a product review"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditPromotionProviders",
                    Enabled = true,
                    Name = "Edit promotion providers"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditReturnRequest",
                    Enabled = true,
                    Name = "Edit a return request"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditSettings",
                    Enabled = true,
                    Name = "Edit setting(s)"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditStateProvince",
                    Enabled = true,
                    Name = "Edit a state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditStore",
                    Enabled = true,
                    Name = "Edit a store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditTask",
                    Enabled = true,
                    Name = "Edit a task"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditSpecAttribute",
                    Enabled = true,
                    Name = "Edit a specification attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditVendor",
                    Enabled = true,
                    Name = "Edit a vendor"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditWarehouse",
                    Enabled = true,
                    Name = "Edit a warehouse"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditTopic",
                    Enabled = true,
                    Name = "Edit a topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditWidget",
                    Enabled = true,
                    Name = "Edit a widget"
                },
                new ActivityLogType
                {
                    SystemKeyword = "Impersonation.Started",
                    Enabled = true,
                    Name = "Customer impersonation session. Started"
                },
                new ActivityLogType
                {
                    SystemKeyword = "Impersonation.Finished",
                    Enabled = true,
                    Name = "Customer impersonation session. Finished"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportCategories",
                    Enabled = true,
                    Name = "Categories were imported"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportManufacturers",
                    Enabled = true,
                    Name = "Manufacturers were imported"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportProducts",
                    Enabled = true,
                    Name = "Products were imported"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportStates",
                    Enabled = true,
                    Name = "States were imported"
                },
                new ActivityLogType
                {
                    SystemKeyword = "InstallNewPlugin",
                    Enabled = true,
                    Name = "Install a new plugin"
                },
                new ActivityLogType
                {
                    SystemKeyword = "UninstallPlugin",
                    Enabled = true,
                    Name = "Uninstall a plugin"
                },
                //public store activities
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.ViewCategory",
                    Enabled = false,
                    Name = "Public store. View a category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.ViewManufacturer",
                    Enabled = false,
                    Name = "Public store. View a manufacturer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.ViewProduct",
                    Enabled = false,
                    Name = "Public store. View a product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.PlaceOrder",
                    Enabled = false,
                    Name = "Public store. Place an order"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.SendPM",
                    Enabled = false,
                    Name = "Public store. Send PM"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.ContactUs",
                    Enabled = false,
                    Name = "Public store. Use contact us form"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddToCompareList",
                    Enabled = false,
                    Name = "Public store. Add to compare list"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddToShoppingCart",
                    Enabled = false,
                    Name = "Public store. Add to shopping cart"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddToWishlist",
                    Enabled = false,
                    Name = "Public store. Add to wishlist"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.Login",
                    Enabled = false,
                    Name = "Public store. Login"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.Logout",
                    Enabled = false,
                    Name = "Public store. Logout"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddProductReview",
                    Enabled = false,
                    Name = "Public store. Add product review"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddNewsComment",
                    Enabled = false,
                    Name = "Public store. Add news comment"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddBlogComment",
                    Enabled = false,
                    Name = "Public store. Add blog comment"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddForumTopic",
                    Enabled = false,
                    Name = "Public store. Add forum topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.EditForumTopic",
                    Enabled = false,
                    Name = "Public store. Edit forum topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.DeleteForumTopic",
                    Enabled = false,
                    Name = "Public store. Delete forum topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddForumPost",
                    Enabled = false,
                    Name = "Public store. Add forum post"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.EditForumPost",
                    Enabled = false,
                    Name = "Public store. Edit forum post"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.DeleteForumPost",
                    Enabled = false,
                    Name = "Public store. Delete forum post"
                },
                new ActivityLogType
                {
                    SystemKeyword = "UploadNewPlugin",
                    Enabled = true,
                    Name = "Upload a plugin"
                },
                new ActivityLogType
                {
                    SystemKeyword = "UploadNewTheme",
                    Enabled = true,
                    Name = "Upload a theme"
                }
            };
            _activityLogTypeRepository.Insert(activityLogTypes);
        }

        protected virtual void InstallProductTemplates()
        {
            var productTemplates = new List<ProductTemplate>
            {
                new ProductTemplate
                {
                    Name = "Producto simple",
                    ViewPath = "ProductTemplate.Simple",
                    DisplayOrder = 10,
                    IgnoredProductTypes = ((int) ProductType.GroupedProduct).ToString()
                },
                new ProductTemplate
                {
                    Name = "Productos agrupados (con variantes)",
                    ViewPath = "ProductTemplate.Grouped",
                    DisplayOrder = 100,
                    IgnoredProductTypes = ((int) ProductType.SimpleProduct).ToString()
                }
            };
            _productTemplateRepository.Insert(productTemplates);
        }

        protected virtual void InstallCategoryTemplates()
        {
            var categoryTemplates = new List<CategoryTemplate>
            {
                new CategoryTemplate
                {
                    Name = "Products in Grid or Lines",
                    ViewPath = "CategoryTemplate.ProductsInGridOrLines",
                    DisplayOrder = 1
                },
            };
            _categoryTemplateRepository.Insert(categoryTemplates);
        }

        protected virtual void InstallManufacturerTemplates()
        {
            var manufacturerTemplates = new List<ManufacturerTemplate>
            {
                new ManufacturerTemplate
                {
                    Name = "Productos en cuadrícula o lista",
                    ViewPath = "ManufacturerTemplate.ProductsInGridOrLines",
                    DisplayOrder = 1
                },
            };
            _manufacturerTemplateRepository.Insert(manufacturerTemplates);
        }

        protected virtual void InstallTopicTemplates()
        {
            var topicTemplates = new List<TopicTemplate>
            {
                new TopicTemplate
                {
                    Name = "Default template",
                    ViewPath = "TopicDetails",
                    DisplayOrder = 1
                },
            };
            _topicTemplateRepository.Insert(topicTemplates);
        }

        protected virtual void InstallScheduleTasks()
        {
            var tasks = new List<ScheduleTask>
            {
                new ScheduleTask
                {
                    Name = "Send emails",
                    Seconds = 60,
                    Type = "Nop.Services.Messages.QueuedMessagesSendTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = "Nop.Services.Common.KeepAliveTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Delete guests",
                    Seconds = 600,
                    Type = "Nop.Services.Customers.DeleteGuestsTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Clear cache",
                    Seconds = 600,
                    Type = "Nop.Services.Caching.ClearCacheTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Clear log",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Nop.Services.Logging.ClearLogTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask
                {
                    Name = "Update currency exchange rates",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Nop.Services.Directory.UpdateExchangeRateTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
            };

            _scheduleTaskRepository.Insert(tasks);
        }

        protected virtual void InstallReturnRequestReasons()
        {
            var returnRequestReasons = new List<ReturnRequestReason>
            {
                new ReturnRequestReason
                {
                    Name = "Recibí el producto equivocado",
                    DisplayOrder = 1
                },
                new ReturnRequestReason
                {
                    Name = "Ordené el producto equivocado",
                    DisplayOrder = 2
                },
                new ReturnRequestReason
                {
                    Name = "Hubo un problema con el producto",
                    DisplayOrder = 3
                }
            };
            _returnRequestReasonRepository.Insert(returnRequestReasons);
        }

        protected virtual void InstallReturnRequestActions()
        {
            var returnRequestActions = new List<ReturnRequestAction>
            {
                new ReturnRequestAction
                {
                    Name = "Reparación",
                    DisplayOrder = 1
                },
                new ReturnRequestAction
                {
                    Name = "Reemplazar",
                    DisplayOrder = 2
                },
                new ReturnRequestAction
                {
                    Name = "Crédito en la tienda",
                    DisplayOrder = 3
                }
            };
            _returnRequestActionRepository.Insert(returnRequestActions);
        }

        protected virtual void InstallWarehouses()
        {

        }

        protected virtual void InstallVendors()
        {

        }

        protected virtual void InstallAffiliates()
        {

        }

        private void AddProductTag(Product product, string tag)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Install data
        /// </summary>
        /// <param name="defaultUserEmail">Default user email</param>
        /// <param name="defaultUserPassword">Default user password</param>
        /// <param name="installSampleData">A value indicating whether to install sample data</param>
        public virtual void InstallData(string defaultUserEmail,
            string defaultUserPassword, bool installSampleData = true)
        {
            InstallStores();
            InstallMeasures();
            InstallTaxCategories();
            InstallLanguages();
            InstallCurrencies();
            InstallCountriesAndStates();
            InstallShippingMethods();
            InstallDeliveryDates();
            InstallProductAvailabilityRanges();
            InstallEmailAccounts();
            InstallMessageTemplates();
            InstallTopicTemplates();
            InstallSettings(installSampleData);
            InstallCustomersAndUsers(defaultUserEmail, defaultUserPassword);
            InstallTopics();
            InstallLocaleResources();
            InstallActivityLogTypes();
            InstallProductTemplates();
            InstallCategoryTemplates();
            InstallManufacturerTemplates();
            InstallScheduleTasks();
            InstallReturnRequestReasons();
            InstallReturnRequestActions();

            if (installSampleData)
            {
                InstallCheckoutAttributes();
                InstallSpecificationAttributes();
                InstallProductAttributes();
                InstallCategories();
                InstallManufacturers();
                InstallProducts(defaultUserEmail);
                InstallForums();
                InstallDiscounts();
                InstallBlogPosts(defaultUserEmail);
                InstallNews(defaultUserEmail);
                InstallPolls();
                InstallWarehouses();
                InstallVendors();
                InstallAffiliates();
                InstallOrders();
                InstallActivityLog(defaultUserEmail);
                InstallSearchTerms();
            }
        }

        #endregion
    }
}