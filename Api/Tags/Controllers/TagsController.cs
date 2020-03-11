using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediumGrabber.Api.Tags
{
    [AllowAnonymous]
    public class TagsController : ControllerBase
    {
        private readonly MediumTagsService _mediumTagsService;
        public TagsController(MediumTagsService mediumTagsService)
        {
            _mediumTagsService = mediumTagsService;            
        }

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
    }
}