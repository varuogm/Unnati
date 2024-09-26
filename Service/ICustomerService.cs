using Unnati.Helper;
using Unnati.Models;

namespace Unnati.Service
{
    public interface ICustomerService
    {
        Task<List<Customermodel>> GetAll();
        Task<Customermodel> Getbycode(string code);
        Task<APIResponse> Remove(string code);
        Task<APIResponse> Create(Customermodel data);
        Task<APIResponse> Update(Customermodel data, string code);
    }
}
