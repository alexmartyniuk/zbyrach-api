using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zbyrach.Api.Tags
{
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly MediumTagsService _mediumTagsService;
        private readonly TagService _tagService;
        private readonly UsersService _userService;

        public TagsController(MediumTagsService mediumTagsService, TagService tagService, UsersService userService)
        {
            _mediumTagsService = mediumTagsService;
            _tagService = tagService;
            _userService = userService;
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
            var currentUser = await _userService.GetCurrentUser();
            var myTags = await _tagService.GetByUser(currentUser);
            return Ok(myTags.Select(t => new TagDto { Name = t.Name }));
        }

        [HttpPost]
        [Route("/tags/my")]
        public async Task<IActionResult> SetMyTags([FromBody] List<string> values)
        {
            var currentUser = await _userService.GetCurrentUser();
            var tags = values.Select(v => new Tag
            {
                Name = v
            });
            await _tagService.SetByUser(currentUser, tags);

            var myTags = await _tagService.GetByUser(currentUser);
            return Ok(myTags.Select(t => new TagDto { Name = t.Name }));
        }
    }
}