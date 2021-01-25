using FluentAssertions;
using Xunit;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tests.Articles
{
    public class TranslationServiceTests
    {
        [Theory]
        [InlineData("Привіт світ! Хай живе Україна!", "ukr")]
        [InlineData("Hello world! Glory to Ukraine!", "eng")]
        [InlineData("Привет мир! Пусть живет Украина!", "rus")]
        [InlineData("Jordão é um município que recebe bastante turistas", "por")]
        [InlineData("d 5 h s  і в п о d f g", null)]
        public void DetectLanguage_ForText_ShouldDetectCorrectly(string text, string language)
        {
            var service = new TranslationService();

            var result = service.DetectLanguage(text);
            result.Should().Be(language);
        }
    }
}