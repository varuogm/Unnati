using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json;
using Unnati.Models;
using Unnati.Service;

namespace Unnati.Container
{
    public class ExternalAPIService : IExternalAPIService
    {
        private readonly IHttpClientFactory _factory;
        private readonly CatSettings _settings;
        private ILogger<ExternalAPIService> _logger;
        public ExternalAPIService(IHttpClientFactory factory, IOptions<CatSettings> settings, ILogger<ExternalAPIService> logger)
        {
            _factory = factory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<IActionResult> GetCatImage()
        {
            try
            {
                using var client = _factory.CreateClient("cat");
                var response = await client.GetAsync("images/search");

                if (!response.IsSuccessStatusCode)
                    return new NotFoundResult();

                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<CatModel>>(responseBody);

                return new OkObjectResult(result);

            }
            catch (Exception)
            {
                _logger.LogError("EXception occured during fetching cat images");
                throw;
            }
        }
    }
}
