using System.Linq;
using LanguageDetection;

namespace Zbyrach.Api.Articles
{
    public class TranslationService
    {
        private readonly LanguageDetector _detector;

        public TranslationService()
        {
            _detector = new LanguageDetector();
            _detector.AddAllLanguages();
        }

        public string DetectLanguage(string text)
        {
            var language = _detector.DetectAll(text).FirstOrDefault();            
            return language?.Probability > 0.9 ? language.Language : null;
        }
    }
}