using AutoMapper;
using Unnati.Repos;
using Unnati.Models;
using Unnati.Helper;
using Unnati.Service;
using Unnati.Repos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Unnati.Container
{

    [Route("[controller]")]
    public class CustomerService : ICustomerService
    {
        private readonly UnnatiContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(UnnatiContext context, IMapper mapper, ILogger<CustomerService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<Customermodel>> GetAll()
        {
            List<Customermodel> result = new List<Customermodel>();
            var data = await this._context.TblCustomers.ToListAsync();

            if (data != null)
            {
                result = this._mapper.Map<List<TblCustomer>, List<Customermodel>>(data);
            }
            _logger.LogInformation("GetAll data fetched successfully");
            return result;
        }


        public async Task<Customermodel> Getbycode(string code)
        {
            Customermodel result = new Customermodel();
            var data = await this._context.TblCustomers.FindAsync(code);

            if (data != null)
            {
                result = this._mapper.Map<TblCustomer, Customermodel>(data);
            }
            return result;
        }


        public async Task<APIResponse> Create(Customermodel data)
        {
            APIResponse response = new APIResponse();
            try
            {
                TblCustomer _customer = this._mapper.Map<Customermodel, TblCustomer>(data);

                await this._context.TblCustomers.AddAsync(_customer);
                await this._context.SaveChangesAsync();
                response.ResponseCode = 201;
                response.Result = "Record created successfully";
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Message = ex.Message;
                _logger.LogError(ex.Message, ex);
            }
            return response;
        }

        public async Task<APIResponse> Remove(string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this._context.TblCustomers.FindAsync(code);
                if (_customer != null)
                {
                    this._context.TblCustomers.Remove(_customer);
                    await this._context.SaveChangesAsync();
                    response.ResponseCode = 200;
                    response.Result = "Removed Successfully";
                }
                else
                {
                    response.ResponseCode = 404;
                    response.Message = "Data not found";
                }

            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Message = ex.Message;
                _logger.LogError(ex.Message, ex);

            }
            return response;
        }

        public async Task<APIResponse> Update(Customermodel data, string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this._context.TblCustomers.FindAsync(code);

                if (_customer != null)
                {
                    _customer.Name = data.Name;
                    _customer.Email = data.Email;
                    _customer.Phone = data.Phone;
                    _customer.IsActive = data.IsActive;
                    _customer.Creditlimit = data.Creditlimit;

                    await this._context.SaveChangesAsync();
                    response.ResponseCode = 200;
                    response.Result = "Data modified successfully";
                }
                else
                {
                    response.ResponseCode = 404;
                    response.Message = "Data not found";
                }

            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Message = ex.Message;
                _logger.LogError(ex.Message, ex);
            }
            return response;
        }
    }
}
