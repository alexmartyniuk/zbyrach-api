using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Wangkanai.Detection.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace Zbyrach.Api.Articles
{
    public class PdfService
    {
        private readonly string _pdfServiceUrl;

        public PdfService(IConfiguration configuration)
        {            
            _pdfServiceUrl = configuration["PdfServiceUrl"];
        }

        public async Task<Stream> ConvertUrlToPdf(string url, Device device, bool inline = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var deviceType = device switch
            {
                Device.Mobile => DeviceType.Mobile,
                Device.Tablet => DeviceType.Tablet,
                _ => DeviceType.Desktop
            };

            using (var client = new HttpClient())
            {
                var request = new GeneratePdfRequest
                {
                    ArticleUrl = url,
                    DeviceType = deviceType,
                    Inline = inline
                };

                var response = await client.PostAsync($"{_pdfServiceUrl}/pdf", new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
                }

                return await response.Content.ReadAsStreamAsync();
            }
        }

        public async Task QueueArticle(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }           

            using (var client = new HttpClient())
            {
                var request = new QueueArticleRequest
                {
                    ArticleUrl = url
                };

                var response = await client.PostAsync($"{_pdfServiceUrl}/queue", new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
                }
            }
        }

        public async Task<StatisticResponse> GetStatistic()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{_pdfServiceUrl}/statistic");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<StatisticResponse>(content);
            }
        }

        public async Task Cleanup(int daysCleanup)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{_pdfServiceUrl}/cleanup/{daysCleanup}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
                }
            }
        }
    }

    public class GeneratePdfRequest
    {
        public string ArticleUrl { get; set; }
        public DeviceType DeviceType { get; set; }
        public bool Inline { get; set; }
    }

    public class QueueArticleRequest
    {
        public string ArticleUrl { get; set; }
    }

    public enum DeviceType
    {
        Unknown = 0,
        Mobile = 1,
        Tablet = 2,
        Desktop = 3
    }

    public class StatisticResponse
    {
        public long TotalRowsCount { get; set; }
        public long TotalSizeInBytes { get; set; }
    }
}