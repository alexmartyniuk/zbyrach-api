using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tests.Services
{
    public class TranslationServiceTests
    {
        [Theory]
        [InlineData("Привіт світ! Хай живе Україна!", "uk")]
        [InlineData("Hello world! Glory to Ukraine!", "en")]
        [InlineData("Привет мир! Пусть живет Украина!", "ru")]
        [InlineData("Jordão é um município que recebe bastante turistas", "pt")]
        [InlineData("d 5 h s  і в п о d f g", null)]
        public void DetectLanguage_ForText_ShouldDetectCorrectly(string text, string language)
        {
            var service = new TranslationService();

            var result = service.DetectLanguage(text);
            result.Should().Be(language);
        }
    }
}