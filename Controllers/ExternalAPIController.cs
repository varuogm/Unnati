using Microsoft.AspNetCore.Mvc;
using Unnati.Service;

namespace Unnati.Controllers
{
    [Route("[controller]/api")]
    [ApiController]
    public class ExternalAPIController : ControllerBase
    {
        private readonly IExternalAPIService _externalAPIService;
        private readonly ILogger<ExternalAPIController> _logger;

        public ExternalAPIController(IExternalAPIService externalAPIService, ILogger<ExternalAPIController> logger)
        {
            _externalAPIService = externalAPIService;
            _logger = logger;
        }

        [HttpGet]
        [Route("CAT")]
        public async Task<IActionResult> GetCatImage()
        {
            try
            {
                return await _externalAPIService.GetCatImage();
            }
            catch (Exception)
            {
                _logger.LogError("EXception occured during fetching cat images");

                throw;
            }
        }
    }
}