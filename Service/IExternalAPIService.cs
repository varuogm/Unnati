using Microsoft.AspNetCore.Mvc;

namespace Unnati.Service
{
    public interface IExternalAPIService
    {
        Task<IActionResult> GetCatImage();
    }
}