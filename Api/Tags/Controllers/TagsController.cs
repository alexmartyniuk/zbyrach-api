using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediumGrabber.Api.Tags
{
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly MediumTagsService _mediumTagsService;
        private readonly TagService _tagService;

        public TagsController(MediumTagsService mediumTagsService, TagService tagService)
        {
            _mediumTagsService = mediumTagsService;
            _tagService = tagService;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/tags/{tagName}")]
        public async Task<IActionResult> GetTagInfo(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return BadRequest("Tag name is empty.");
            }

            var tagInfo = await _mediumTagsService.GetFullTagInfoByName(tagName);
            return Ok(tagInfo);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/tags/{tagName}/related")]
        public async Task<IActionResult> GetRelatedTags(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return BadRequest("Tag name is empty.");
            }

            var relatedTags = await _mediumTagsService.GetRelatedTags(tagName);
            return Ok(relatedTags);
        }

        [HttpGet]
        [Route("/tags/my")]
        public async Task<IActionResult> GetMyTags()
        {
            var myTags = await _tagService.GetMyTags();
            return Ok(myTags.Select(t => new TagDto { Name = t.Name }));
        }

        [HttpPost]
        [Route("/tags/my")]
        public async Task<IActionResult> SetMyTags([FromBody] List<string> values)
        {
            var tags = values.Select(v => new Tag
            {
                Name = v
            });
            await _tagService.SetMyTags(tags);

            var myTags = await _tagService.GetMyTags();
            return Ok(myTags.Select(t => new TagDto { Name = t.Name }));
        }
    }
}