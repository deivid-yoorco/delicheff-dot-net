using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Teed.Plugin.MessageBird.Models
{
    #region Main

    public class WhatsAppSendModel : BaseNopModel
    {
        public WhatsAppSendModel()
        {
            Names = new List<SelectListItem>();
            Phones = new List<SelectListItem>();
        }

        public bool IsAllowed { get; set; }
        public MessageBirdSettings Settings { get; set; }

        public int CustomerId { get; set; }
        public string ToNumber { get; set; }
        public string ToName { get; set; }
        public string Template { get; set; }
        public string Variables { get; set; }

        public List<SelectListItem> Names { get; set; }
        public List<SelectListItem> Phones { get; set; }
    }

    #endregion

    #region Send request

    public class WhatsAppBody
    {
        public WhatsAppBody()
        {
            content = new BodyContent();
        }

        public string to { get; set; }
        public string from { get; set; }
        public string type { get; set; }
        public BodyContent content { get; set; }
    }

    public class BodyContent
    {
        public BodyContent()
        {
            hsm = new ContentHsm();
        }

        public ContentHsm hsm { get; set; }
    }

    public class ContentText
    {
        public ContentText()
        {
            text = "";
        }

        public string text { get; set; }
    }

    public class ContentHsm
    {
        public ContentHsm()
        {
            language = new HsmLanguage();
            components = new List<HsmComponent>();
        }

        public string nameSpace { get; set; }
        public string templateName { get; set; }
        public HsmLanguage language { get; set; }
        public List<HsmComponent> components { get; set; }
    }

    public class HsmLanguage
    {
        public string code { get; set; }
        public string policy { get; set; }
    }

    public class HsmComponent
    {
        public HsmComponent()
        {
            parameters = new List<ComponentParameter>();
        }

        public string type { get; set; }
        public List<ComponentParameter> parameters { get; set; }
    }

    public class ComponentParameter
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public enum BodyType
    {
        hsm,
        text
    }
    public enum LanguageType
    {
        es_MX
    }

    public enum ComponentType
    {
        body
    }

    public enum ParameterType
    {
        text
    }

    #endregion

    #region Receive response

    public class Language
    {
        public string policy { get; set; }
        public string code { get; set; }
    }

    public class Parameter
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class Component
    {
        public string type { get; set; }
        public List<Parameter> parameters { get; set; }
    }

    public class Hsm
    {
        public string @namespace { get; set; }
        public string templateName { get; set; }
        public Language language { get; set; }
        public List<Component> components { get; set; }
    }

    public class Content
    {
        public Hsm hsm { get; set; }
    }

    public class ReceiveMessageInfo
    {
        public string id { get; set; }
        public string conversationId { get; set; }
        public string platform { get; set; }
        public string to { get; set; }
        public string from { get; set; }
        public string channelId { get; set; }
        public string type { get; set; }
        public Content content { get; set; }
        public string direction { get; set; }
        public string status { get; set; }
        public DateTime createdDatetime { get; set; }
        public DateTime updatedDatetime { get; set; }
    }

    #endregion

    public enum SuccessType
    {
        pending,
        sent,
        read,
        received,
        delivered
    }

    public enum FailureType
    {
        rejected,
        failed,
        deleted,
        unknown
    }
}
