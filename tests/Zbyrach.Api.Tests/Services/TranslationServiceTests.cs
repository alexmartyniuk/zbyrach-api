using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tests.Services
{
    public class TranslationServiceTests
    {
        [Theory]
        [InlineData("Привіт світ!", "Ukrainian")]
        [InlineData("Hello world!", "English")]
        [InlineData("Привет мир!", "Russian")]
        [InlineData("aklaskhd kjahsk!", null)]
        public async Task DetectLanguage_ForText_ShouldDetectCorrectly(string text, string language)
        {
            var service = new TranslationService();

            var result = await service.DetectLanguage(text);
            result.Should().Be(language);
        }
    }
}