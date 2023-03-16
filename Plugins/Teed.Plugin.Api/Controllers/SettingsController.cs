using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Topics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Settings;

namespace Teed.Plugin.Api.Controllers
{
    public class SettingsController : ApiBaseController
    {
        #region Properties

        private readonly ITopicService _topicService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public SettingsController(ITopicService topicService,
            IPictureService pictureService)
        {
            _topicService = topicService;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        public IActionResult GetSettingsPages()
        {
            List<TopicDto> topics = _topicService.GetAllTopics(1)
                .Where(x => x.IncludeInMobileApp && x.Published)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new TopicDto()
                {
                    Id = x.Id,
                    Name = x.GetLocalized(y => y.Title)
                }).ToList();

            return Ok(topics);
        }

        public IActionResult GetPageBody(int topicId)
        {
            var topic = _topicService.GetTopicById(topicId);
            if (topic == null) return NotFound();
            return Ok(topic.Body);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPicture(int id)
        {
            var picture = _pictureService.GetPictureById(id);
            if (picture == null)
            {
                picture = new Nop.Core.Domain.Media.Picture();
                using (WebClient client = new WebClient())
                {
                    picture.PictureBinary = client.DownloadData(_pictureService.GetDefaultPictureUrl());
                    picture.MimeType = "image/png";
                }
            }
            return File(picture.PictureBinary, picture.MimeType);
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
