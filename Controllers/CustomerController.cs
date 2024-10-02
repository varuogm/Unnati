using Unnati.Models;
using Unnati.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Unnati.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;   
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        //[AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Inside Get all customer data");
            var result = await _customerService.GetAll();

            if(result ==null)
                    return NotFound();
            return Ok(result);
        }

        [HttpGet("Getbycode")]
        //[ResponseCache/*(*/Duration = /*30, VaryByQueryKeys = new[] { "id" }*/)]
        public async Task<IActionResult> Getbycode(string code)
        {
            _logger.LogInformation("Inside Get by code");

            var data = await this._customerService.Getbycode(code);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Customermodel _data)
        {
            _logger.LogInformation("Inside create customer");

            var data = await this._customerService.Create(_data);
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(Customermodel _data, string code)
        {
            _logger.LogInformation("Inside update customer");

            var data = await this._customerService.Update(_data, code);
            return Ok(data);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(string code)
        {
            _logger.LogInformation("Inside remove customer");

            var data = await this._customerService.Remove(code);
            return Ok(data);
        }
    }
}
