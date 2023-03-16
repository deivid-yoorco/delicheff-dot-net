using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.TicketControl.Attributes
{
    public class ResponseFilter : Attribute, IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var jsonSerializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (context.Result is ObjectResult objectResult)
            {
                var result = new ObjectResult(objectResult.Value);
                result.StatusCode = objectResult.StatusCode;
                result.ContentTypes = objectResult.ContentTypes;
                result.Formatters.RemoveType<JsonOutputFormatter>();
                result.Formatters.Add(new JsonOutputFormatter(jsonSerializationSettings, ArrayPool<char>.Shared));
                context.Result = result;
            }

            await next();
        }
    }
}
