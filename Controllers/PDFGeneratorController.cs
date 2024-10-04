using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Unnati.Service;

namespace Unnati.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PDFsController : ControllerBase
    {
        private readonly IPDFGeneratorService _pdfService;
        private readonly ILogger<PDFsController> _logger;

        public PDFsController(IPDFGeneratorService pdf, ILogger<PDFsController> logger)
        {
            _pdfService = pdf;
            _logger = logger;
        }

        [HttpGet]
        [Route("User")]
        public async Task<IActionResult> DownloadUsers()
        {
            var pdfConent = await _pdfService.DownloadUsersPdfAsync();
            if (pdfConent == null)
            {
                return NotFound(new
                {
                    message = "content is not found"
                });
            }
            return File(pdfConent, "application/pdf", "UsersList.pdf");
        }

        [HttpGet]
        [Route("Product")]

        public async Task<IActionResult> DownloadProductS()
        {
            var pdfConent = await _pdfService.DownloadProductsPdfAsync();
            if (pdfConent == null)
            {
                return NotFound(new
                {
                    message = "content is not found"
                });
            }
            return File(pdfConent, "application/pdf", "ProductList.pdf");
        }
    }
}