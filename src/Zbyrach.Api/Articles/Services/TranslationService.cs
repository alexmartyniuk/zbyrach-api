using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Zbyrach.Api.Articles
{
    public class TranslationService
    {
        public async Task<string> DetectLanguage(string text)
        {
            var toLanguage = "uk";
            var fromLanguage = "auto";
            var encodedText = WebUtility.UrlEncode(text);
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={encodedText}";
            var webClient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            var result = webClient.DownloadString(url);
            try
            {
                var obj = JArray.Parse(result);
                var probText = obj[8][2][0].ToString();
                var probability = double.Parse(probText, CultureInfo.InvariantCulture);

                if (probability < 0.5)
                {
                    return null;
                }

                var language = obj[8][3][0].ToString();

                foreach (var ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                {
                    if (language == ci.Name)
                    {
                        return ci.EnglishName;
                    };
                }

                return null;

            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}