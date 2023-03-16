using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Logging;
using OfficeOpenXml;

namespace Nop.Services.Localization
{
    /// <summary>
    /// Provides information about localization
    /// </summary>
    public partial class LocalizationService : ILocalizationService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LOCALSTRINGRESOURCES_ALL_PUBLIC_KEY = "Nop.lsr.all.public-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LOCALSTRINGRESOURCES_ALL_KEY = "Nop.lsr.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LOCALSTRINGRESOURCES_ALL_ADMIN_KEY = "Nop.lsr.all.admin-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : resource key
        /// </remarks>
        private const string LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY = "Nop.lsr.{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string LOCALSTRINGRESOURCES_PATTERN_KEY = "Nop.lsr.";
        /// <summary>
        /// Key pattern to split resource by group
        /// </summary>
        private const string ADMIN_LOCALSTRINGRESOURCES_PATTERN = "Admin.";

        #endregion

        #region Fields

        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly CommonSettings _commonSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Static cache manager</param>
        /// <param name="logger">Logger</param>
        /// <param name="workContext">Work context</param>
        /// <param name="lsrRepository">Locale string resource repository</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="dbContext">Database Context</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="eventPublisher">Event published</param>
        public LocalizationService(IStaticCacheManager cacheManager,
            ILogger logger,
            IWorkContext workContext,
            IRepository<LocaleStringResource> lsrRepository,
            IDataProvider dataProvider,
            IDbContext dbContext,
            CommonSettings commonSettings,
            LocalizationSettings localizationSettings, 
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._logger = logger;
            this._workContext = workContext;
            this._lsrRepository = lsrRepository;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._commonSettings = commonSettings;
            this._localizationSettings = localizationSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Insert resources
        /// </summary>
        /// <param name="resources">Resources</param>
        protected virtual void InsertLocaleStringResources(IList<LocaleStringResource> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            //insert
            _lsrRepository.Insert(resources);

            //cache
            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            foreach (var resource in resources)
            {
                _eventPublisher.EntityInserted(resource);
            }
        }

        /// <summary>
        /// Update resources
        /// </summary>
        /// <param name="resources">Resources</param>
        protected virtual void UpdateLocaleStringResources(IList<LocaleStringResource> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            //update
            _lsrRepository.Update(resources);

            //cache
            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            foreach (var resource in resources)
            {
                _eventPublisher.EntityUpdated(resource);
            }
        }

        private static Dictionary<string, KeyValuePair<int, string>> ResourceValuesToDictionary(IEnumerable<LocaleStringResource> locales)
        {
            //format: <name, <id, value>>
            var dictionary = new Dictionary<string, KeyValuePair<int, string>>();
            foreach (var locale in locales)
            {
                var resourceName = locale.ResourceName.ToLowerInvariant();
                if (!dictionary.ContainsKey(resourceName))
                    dictionary.Add(resourceName, new KeyValuePair<int, string>(locale.Id, locale.ResourceValue));
            }
            return dictionary;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual void DeleteLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException(nameof(localeStringResource));

            _lsrRepository.Delete(localeStringResource);

            //cache
            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(localeStringResource);
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="localeStringResourceId">Locale string resource identifier</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceById(int localeStringResourceId)
        {
            if (localeStringResourceId == 0)
                return null;

            return _lsrRepository.GetById(localeStringResourceId);
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName)
        {
            if (_workContext.WorkingLanguage != null)
                return GetLocaleStringResourceByName(resourceName, _workContext.WorkingLanguage.Id);

            return null;
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId,
            bool logIfNotFound = true)
        {
            var query = from lsr in _lsrRepository.Table
                        orderby lsr.ResourceName
                        where lsr.LanguageId == languageId && lsr.ResourceName == resourceName
                        select lsr;
            var localeStringResource = query.FirstOrDefault();

            if (localeStringResource == null && logIfNotFound)
                _logger.Warning($"Resource string ({resourceName}) not found. Language ID = {languageId}");
            return localeStringResource;
        }

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources</returns>
        public virtual IList<LocaleStringResource> GetAllResources(int languageId)
        {
            var query = from l in _lsrRepository.Table
                        orderby l.ResourceName
                        where l.LanguageId == languageId
                        select l;
            var locales = query.ToList();
            return locales;
        }

        /// <summary>
        /// Inserts a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual void InsertLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException(nameof(localeStringResource));
            
            _lsrRepository.Insert(localeStringResource);

            //cache
            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(localeStringResource);
        }

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual void UpdateLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException(nameof(localeStringResource));

            _lsrRepository.Update(localeStringResource);

            //cache
            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(localeStringResource);
        }
        
        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="loadPublicLocales">A value indicating whether to load data for the public store only (if "false", then for admin area only. If null, then load all locales. We use it for performance optimization of the site startup</param>
        /// <returns>Locale string resources</returns>
        public virtual Dictionary<string, KeyValuePair<int,string>> GetAllResourceValues(int languageId, bool? loadPublicLocales)
        {
            var key = string.Format(LOCALSTRINGRESOURCES_ALL_KEY, languageId);

            //get all locale string resources by language identifier
            if (!loadPublicLocales.HasValue || _cacheManager.IsSet(key))
            {
                var rez = _cacheManager.Get(key, () =>
                {
                    //we use no tracking here for performance optimization
                    //anyway records are loaded only for read-only operations
                    var query = from l in _lsrRepository.TableNoTracking
                        orderby l.ResourceName
                        where l.LanguageId == languageId
                        select l;

                    return ResourceValuesToDictionary(query);
                });

                //remove separated resource 
                _cacheManager.Remove(string.Format(LOCALSTRINGRESOURCES_ALL_PUBLIC_KEY, languageId));
                _cacheManager.Remove(string.Format(LOCALSTRINGRESOURCES_ALL_ADMIN_KEY, languageId));

                return rez;
            }

            //performance optimization of the site startup
            key = string.Format(loadPublicLocales.Value ? LOCALSTRINGRESOURCES_ALL_PUBLIC_KEY : LOCALSTRINGRESOURCES_ALL_ADMIN_KEY, languageId);

            return _cacheManager.Get(key, () =>
            {
                //we use no tracking here for performance optimization
                //anyway records are loaded only for read-only operations
                var query = from l in _lsrRepository.TableNoTracking
                            orderby l.ResourceName
                            where l.LanguageId == languageId
                            select l;
                query = loadPublicLocales.Value ? query.Where(r =>  !r.ResourceName.StartsWith(ADMIN_LOCALSTRINGRESOURCES_PATTERN)) : query.Where(r => r.ResourceName.StartsWith(ADMIN_LOCALSTRINGRESOURCES_PATTERN));
                return ResourceValuesToDictionary(query);
            });
        }

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <returns>A string representing the requested resource string.</returns>
        public virtual string GetResource(string resourceKey)
        {
            if (_workContext.WorkingLanguage != null)
                return GetResource(resourceKey, _workContext.WorkingLanguage.Id);
            
            return "";
        }
        
        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether an empty string will be returned if a resource is not found and default value is set to empty string</param>
        /// <returns>A string representing the requested resource string.</returns>
        public virtual string GetResource(string resourceKey, int languageId,
            bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false)
        {
            var result = string.Empty;
            if (resourceKey == null)
                resourceKey = string.Empty;
            resourceKey = resourceKey.Trim().ToLowerInvariant();
            if (_localizationSettings.LoadAllLocaleRecordsOnStartup)
            {
                //load all records (we know they are cached)
                var resources = GetAllResourceValues(languageId, !resourceKey.StartsWith(ADMIN_LOCALSTRINGRESOURCES_PATTERN, StringComparison.InvariantCultureIgnoreCase));
                if (resources.ContainsKey(resourceKey))
                {
                    result = resources[resourceKey].Value;
                }
            }
            else
            {
                //gradual loading
                var key = string.Format(LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY, languageId, resourceKey);
                var lsr = _cacheManager.Get(key, () =>
                {
                    var query = from l in _lsrRepository.Table
                                where l.ResourceName == resourceKey
                                && l.LanguageId == languageId
                                select l.ResourceValue;
                    return query.FirstOrDefault();
                });

                if (lsr != null) 
                    result = lsr;
            }
            if (string.IsNullOrEmpty(result))
            {
                if (logIfNotFound)
                    _logger.Warning($"Resource string ({resourceKey}) is not found. Language ID = {languageId}");
                
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    if (!returnEmptyIfNotFound)
                        result = resourceKey;
                }
            }
            return result;
        }

        /// <summary>
        /// Export language resources to XML
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportResourcesToXml(Language language)
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Language");
            xmlWriter.WriteAttributeString("Name", language.Name);
            xmlWriter.WriteAttributeString("SupportedVersion", NopVersion.CurrentVersion);

            var resources = GetAllResources(language.Id);
            foreach (var resource in resources)
            {
                xmlWriter.WriteStartElement("LocaleResource");
                xmlWriter.WriteAttributeString("Name", resource.ResourceName);
                xmlWriter.WriteElementString("Value", null, resource.ResourceValue);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        /// <param name="updateExistingResources">A value indicating whether to update existing resources</param>
        public virtual void ImportResourcesFromXml(Language language, string xml, bool updateExistingResources = true)
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));

            if (string.IsNullOrEmpty(xml))
                return;
            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //SQL 2005 insists that your XML schema incoding be in UTF-16.
                //Otherwise, you'll get "XML parsing: line 1, character XXX, unable to switch the encoding"
                //so let's remove XML declaration
                var inDoc = new XmlDocument();
                inDoc.LoadXml(xml);
                var sb = new StringBuilder();
                using (var xWriter = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
                {
                    inDoc.Save(xWriter);
                    xWriter.Close();
                }
                var outDoc = new XmlDocument();
                outDoc.LoadXml(sb.ToString());
                xml = outDoc.OuterXml;

                //stored procedures are enabled and supported by the database.
                var pLanguageId = _dataProvider.GetParameter();
                pLanguageId.ParameterName = "LanguageId";
                pLanguageId.Value = language.Id;
                pLanguageId.DbType = DbType.Int32;

                var pXmlPackage = _dataProvider.GetParameter();
                pXmlPackage.ParameterName = "XmlPackage";
                pXmlPackage.Value = xml;
                pXmlPackage.DbType = DbType.Xml;

                var pUpdateExistingResources = _dataProvider.GetParameter();
                pUpdateExistingResources.ParameterName = "UpdateExistingResources";
                pUpdateExistingResources.Value = updateExistingResources;
                pUpdateExistingResources.DbType = DbType.Boolean;

                //long-running query. specify timeout (600 seconds)
                _dbContext.ExecuteSqlCommand("EXEC [LanguagePackImport] @LanguageId, @XmlPackage, @UpdateExistingResources",
                    false, 600, pLanguageId, pXmlPackage, pUpdateExistingResources);
            }
            else
            {
                //stored procedures aren't supported
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");

                var existingResources = GetAllResources(language.Id);
                var newResources = new List<LocaleStringResource>();

                foreach (XmlNode node in nodes)
                {
                    var name = node.Attributes["Name"].InnerText.Trim();
                    var value = "";
                    var valueNode = node.SelectSingleNode("Value");
                    if (valueNode != null)
                        value = valueNode.InnerText;

                    if (string.IsNullOrEmpty(name))
                        continue;

                    //do not use "Insert"/"Update" methods because they clear cache
                    //let's bulk insert
                    var resource = existingResources.FirstOrDefault(x => x.ResourceName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (resource != null)
                    {
                        if (updateExistingResources)
                        {
                            resource.ResourceValue = value;
                        }
                    }
                    else
                    {
                        newResources.Add(
                            new LocaleStringResource
                            {
                                LanguageId = language.Id,
                                ResourceName = name,
                                ResourceValue = value
                            });
                    }
                }
                InsertLocaleStringResources(newResources);
                UpdateLocaleStringResources(existingResources);
            }

            //clear cache
            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);
        }

        /// <summary>
        /// Export language resources to XLS
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XLS format</returns>
        public virtual byte[] ExportResourcesToXls(Language language)
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));

            using (var stream = new MemoryStream())
            {
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add(language.Name ?? "Language");
                    int row = 1;
                    worksheet.Cells[row, 1].Value = "Name";
                    worksheet.Cells[row, 2].Value = "Value";

                    var resources = GetAllResources(language.Id);
                    foreach (var resource in resources)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = resource.ResourceName;
                        worksheet.Cells[row, 2].Value = resource.ResourceValue;
                    }

                    for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                    {
                        worksheet.Column(i).AutoFit();
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    xlPackage.Save();
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        /// <param name="updateExistingResources">A value indicating whether to update existing resources</param>
        public virtual void ImportResourcesFromXls(Language language, IFormFile xls, bool updateExistingResources = true)
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));

            if (xls == null && xls.Length == 0)
                return;

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(xls.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    List<string> headers = new List<string>();
                    int totalRows = worksheet.Dimension.End.Row;
                    int totalColumns = worksheet.Dimension.End.Column;
                    var range = worksheet.Cells[1, 1, 1, totalColumns];
                    var dataList = new List<LocaleStringResource>();
                    try
                    {
                        var cells = worksheet.Cells.ToList();
                        var groups = GetCellGroups(cells, worksheet.Dimension.End.Row - 1);
                        if (groups == null) return;

                        for (int g = 0; g < groups.Count; g++)
                        {
                            int currentColumn = 0;
                            var name = groups[g][currentColumn].ResourceValue != null ? groups[g][currentColumn].ResourceValue.ToString() : null;
                            currentColumn++;
                            var value = groups[g][currentColumn].ResourceValue != null ? groups[g][currentColumn].ResourceValue.ToString() : null;
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                            {
                                dataList.Add(new LocaleStringResource
                                {
                                    LanguageId = language.Id,
                                    ResourceName = name,
                                    ResourceValue = value,
                                });
                            }
                        }

                        if (dataList.Any())
                        {
                            var sb = new StringBuilder();
                            var stringWriter = new StringWriter(sb);
                            var xmlWriter = new XmlTextWriter(stringWriter);
                            xmlWriter.WriteStartDocument();
                            xmlWriter.WriteStartElement("Language");
                            xmlWriter.WriteAttributeString("Name", language.Name);
                            xmlWriter.WriteAttributeString("SupportedVersion", NopVersion.CurrentVersion);

                            foreach (var resource in dataList)
                            {
                                xmlWriter.WriteStartElement("LocaleResource");
                                xmlWriter.WriteAttributeString("Name", resource.ResourceName);
                                xmlWriter.WriteElementString("Value", null, resource.ResourceValue);
                                xmlWriter.WriteEndElement();
                            }

                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndDocument();
                            xmlWriter.Close();

                            var outDoc = new XmlDocument();
                            outDoc.LoadXml(sb.ToString());
                            var xml = outDoc.OuterXml;

                            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
                            {
                                //stored procedures are enabled and supported by the database.
                                var pLanguageId = _dataProvider.GetParameter();
                                pLanguageId.ParameterName = "LanguageId";
                                pLanguageId.Value = language.Id;
                                pLanguageId.DbType = DbType.Int32;

                                var pXmlPackage = _dataProvider.GetParameter();
                                pXmlPackage.ParameterName = "XmlPackage";
                                pXmlPackage.Value = xml;
                                pXmlPackage.DbType = DbType.Xml;

                                var pUpdateExistingResources = _dataProvider.GetParameter();
                                pUpdateExistingResources.ParameterName = "UpdateExistingResources";
                                pUpdateExistingResources.Value = updateExistingResources;
                                pUpdateExistingResources.DbType = DbType.Boolean;

                                //long-running query. specify timeout (600 seconds)
                                _dbContext.ExecuteSqlCommand("EXEC [LanguagePackImport] @LanguageId, @XmlPackage, @UpdateExistingResources",
                                    false, 600, pLanguageId, pXmlPackage, pUpdateExistingResources);
                            }
                            else
                            {
                                //stored procedures aren't supported
                                var xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(xml);
                                var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");

                                var existingResources = GetAllResources(language.Id);
                                var newResources = new List<LocaleStringResource>();

                                foreach (XmlNode node in nodes)
                                {
                                    var name = node.Attributes["Name"].InnerText.Trim();
                                    var value = "";
                                    var valueNode = node.SelectSingleNode("Value");
                                    if (valueNode != null)
                                        value = valueNode.InnerText;

                                    if (string.IsNullOrEmpty(name))
                                        continue;

                                    //do not use "Insert"/"Update" methods because they clear cache
                                    //let's bulk insert
                                    var resource = existingResources.FirstOrDefault(x => x.ResourceName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                                    if (resource != null)
                                    {
                                        if (updateExistingResources)
                                        {
                                            resource.ResourceValue = value;
                                        }
                                    }
                                    else
                                    {
                                        newResources.Add(
                                            new LocaleStringResource
                                            {
                                                LanguageId = language.Id,
                                                ResourceName = name,
                                                ResourceValue = value
                                            });
                                    }
                                }
                                InsertLocaleStringResources(newResources);
                                UpdateLocaleStringResources(existingResources);
                            }

                            //clear cache
                            _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);
                        }
                    }
                    catch (Exception e)
                    {
                        return;
                    }
                }
            }
        }

        private List<List<LocaleStringResource>> GetCellGroups(List<ExcelRangeBase> elements, int finalRow)
        {
            int i = 0;
            int g = 0;
            try
            {
                var list = new List<List<LocaleStringResource>>();
                var headerLetters = elements.Where(x => x.Start.Row == 1).Select(x => x.Address).Select(x => new String(x.Where(y => Char.IsLetter(y)).ToArray())).ToList();
                for (i = 0; i < finalRow; i++)
                {
                    var listData = new List<LocaleStringResource>();
                    for (g = 0; g < headerLetters.Count; g++)
                    {
                        var address = headerLetters[g] + (i + 2).ToString();
                        var element = elements.Where(x => x.Address == address).FirstOrDefault();
                        if (element == null || element.Value == null)
                        {
                            listData.Add(new LocaleStringResource() { ResourceName = address, ResourceValue = null });
                        }
                        else
                        {
                            listData.Add(new LocaleStringResource() { ResourceName = address, ResourceValue = element.Value.ToString() });
                        }
                    }
                    list.Add(listData);
                }

                return list;
            }
            catch (Exception w)
            {
                return null;
            }
        }

        #endregion
    }
}
