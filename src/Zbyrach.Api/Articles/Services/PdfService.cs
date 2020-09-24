using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Wangkanai.Detection.Services;
using Wangkanai.Detection.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace Zbyrach.Api.Articles
{
    public class PdfService
    {
        private readonly IDetectionService _detectionService;
        private readonly string _pdfServiceUrl;

        public PdfService(IConfiguration configuration, IDetectionService detectionService)
        {
            _detectionService = detectionService;
            _pdfServiceUrl = configuration["PdfServiceUrl"];
        }

        public async Task<Stream> ConvertUrlToPdf(string url, bool inline = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var deviceType = _detectionService.Device.Type switch
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
    }

    public class GeneratePdfRequest
    {
        public string ArticleUrl { get; set; }
        public DeviceType DeviceType { get; set; }
        public bool Inline { get; set; }
    }

    public enum DeviceType
    {
        Unknown = 0,
        Mobile = 1,
        Tablet = 2,
        Desktop = 3
    }
}