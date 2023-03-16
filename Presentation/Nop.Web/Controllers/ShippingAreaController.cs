using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Models.ShippingAreas;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class ShippingAreaController : BasePublicController
    {
        #region Fields

        private readonly ITopicModelFactory _topicModelFactory;
        public ShippingAreaController(ITopicModelFactory topicModelFactory)
        {
            this._topicModelFactory = topicModelFactory;
        }
         #endregion
           
        public async Task<IActionResult> Cobertura()
        {
            var url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/shippingbyaddress/GetShippingAreas";
            
            if (Services.TeedCommerceStores.CurrentStore == Services.TeedStores.CentralEnLinea)
            {
                url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/shippingarea/GetShippingAreas";
            }
            //var url = "https://localhost:44345/shippingarea/GetShippingAreas";

            CoberturaViewModel model = new CoberturaViewModel();
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var areaList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AreaData>>(json);
                    model.AreasData = areaList.OrderBy(x => x.PostalCode).ToList();
                }
                var body = _topicModelFactory.PrepareTopicModelById(10);
                if (body != null)
                {
                    body.Title = body.Title;
                    body.Body = body.Body.Replace("<script", "<>>");
                    body.Body = body.Body.Replace(".location.href", "'<>>");
                    body.Body = body.Body.Replace("</script>", "<>");
                  
                  }
                model.Body = body.Body.ToString();
                model.Title = body.Title.ToString();

            }
            return View(model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public async Task<string> GetGeoJson()
        {
            //var url = "https://localhost:44345/shippingarea/GetValidPostalCodes";
            var url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/shippingbyaddress/GetValidPostalCodes";
            if (Services.TeedCommerceStores.CurrentStore == Services.TeedStores.CentralEnLinea)
            {
                url = (Request.IsHttps ? "https" : "http") + $"://{Request.Host}/shippingarea/GetValidPostalCodes";
            }

            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    var validPostalCodes = await result.Content.ReadAsStringAsync();
                    var json = System.IO.File.ReadAllText("./wwwroot/files/cdmx-postalcodes-geolocation.json");
                    List<GeoLocationObject> objectDataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GeoLocationObject>>(json);
                    var geometryList = objectDataList.Where(x => validPostalCodes.Split(',').Contains(x.properties.d_cp)).ToList();

                    if (Services.TeedCommerceStores.CurrentStore == Services.TeedStores.ZanaAlquimia ||
                        Services.TeedCommerceStores.CurrentStore == Services.TeedStores.Masa ||
                        Services.TeedCommerceStores.CurrentStore == Services.TeedStores.CentralEnLinea)
                    {
                        var jsonEdmx = System.IO.File.ReadAllText("./wwwroot/files/edmx-postalcodes-geolocation.json");
                        List<GeoLocationObject> objectDataListEdmx = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GeoLocationObject>>(jsonEdmx);
                        var geometryListEdmx = objectDataListEdmx.Where(x => validPostalCodes.Split(',').Contains(x.properties.d_cp)).ToList();
                        geometryList = geometryList.Union(geometryListEdmx).ToList();
                    }

                    var geojsonObject = new GeoJsonObject()
                    {
                        type = "FeatureCollection",
                        features = geometryList
                    };

                    string geometryListJson = Newtonsoft.Json.JsonConvert.SerializeObject(geojsonObject);
                    return geometryListJson;
                }
            }

            // Something went wrong so we return an empty json
            return "";
        }

        public class GeoJsonObject
        {
            public string type { get; set; }
            public List<GeoLocationObject> features { get; set; }
        }

        public class GeoLocationObject
        {
            public string type { get; set; }
            public GeoLocationProperties properties { get; set; }
            public dynamic geometry { get; set; }
        }

        public class GeoLocationProperties
        {
            public string name { get; set; }
            public string description { get; set; }
            public string d_cp { get; set; }
        }
    }
}
