using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System;

namespace Nop.Web.Components
{
    public class TopicBlockViewComponent : NopViewComponent
    {
        private readonly ITopicModelFactory _topicModelFactory;

        public TopicBlockViewComponent(ITopicModelFactory topicModelFactory)
        {
            this._topicModelFactory = topicModelFactory;
        }

        public IViewComponentResult Invoke(string systemName)
        {
            var model = _topicModelFactory.PrepareTopicModelBySystemName(systemName);
            if (model == null)
                return Content("");
            if(model.Body != null)
            {
                model.Body = model.Body.Replace("<script", "<>>");
                model.Body = model.Body.Replace("</script>", "<>");
                string[] tokens = model.Body.Split(new[] { "<>" }, StringSplitOptions.RemoveEmptyEntries);
                //model.Body = "";
            }

            return View(model);
        }
    }
}
