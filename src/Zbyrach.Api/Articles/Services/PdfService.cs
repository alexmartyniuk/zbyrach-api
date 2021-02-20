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
        private readonly HttpClient _httpClient = default!;

        public PdfService(IConfiguration configuration, HttpClient client)
        {
            _httpClient = client;
            _httpClient.BaseAddress = new Uri(configuration["PdfServiceUrl"]);
        }

        protected PdfService()
        {

        }

        public async Task<Stream> ConvertUrlToPdf(string url, Device device, bool inline = false)
        {
            var deviceType = device switch
            {
                Device.Mobile => DeviceType.Mobile,
                Device.Tablet => DeviceType.Tablet,
                _ => DeviceType.Desktop
            };


            var request = new GeneratePdfRequest
            {
                ArticleUrl = url,
                DeviceType = deviceType,
                Inline = inline
            };

            var response = await _httpClient.PostAsync("/pdf", new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
            }

            return await response.Content.ReadAsStreamAsync();
        }

        public virtual async Task QueueArticle(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var request = new QueueArticleRequest
            {
                ArticleUrl = url
            };

            var response = await _httpClient.PostAsync("/queue", new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
            }
        }

        public async Task<StatisticResponse> GetStatistic()
        {
            var response = await _httpClient.GetAsync("/statistic");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatisticResponse>(content);
        }

        public async Task Cleanup(int daysCleanup)
        {
            var response = await _httpClient.DeleteAsync($"/cleanup/{daysCleanup}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response from PDF service: {response.StatusCode} {response.ReasonPhrase}");
            }
        }
    }

    public class GeneratePdfRequest
    {
        public string ArticleUrl { get; set; } = default!;
        public DeviceType DeviceType { get; set; }
        public bool Inline { get; set; }
    }

    public class QueueArticleRequest
    {
        public string ArticleUrl { get; set; } = default!;
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