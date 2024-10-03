using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Unnati.Helper;
using Unnati.Models;
using Unnati.Repos;
using Unnati.Repos.Models;
using Unnati.Service;

namespace Unnati.Container
{
    public class PDFGeneratorService : IPDFGeneratorService
    {
        private readonly IMapper _mapper;
        private readonly UnnatiContext _context;
        private readonly ILogger<PDFGeneratorService> _logger;

        //EXplicit data
        private readonly string[] userColumnNames = new[] { "User Name", "Name", "Email", "Phone", "isActive", "Status", "Role" };
        private readonly string[] productColumnNames = new[] { "Code", "Name", "Price" };

        public PDFGeneratorService(UnnatiContext context, IMapper mapper, ILogger<PDFGeneratorService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<byte[]?> DownloadProductsPdfAsync()
        {
            try
            {
                List<Products> _response = new List<Products>();
                var _data = await this._context.TblProducts.ToListAsync();
                if (_data != null)
                {
                    _response = this._mapper.Map<List<TblProduct>, List<Products>>(_data);
                }

                var document = await PDFGenerator.createPdfContent(_response, productColumnNames);

                return document.GeneratePdf();

            }
            catch (Exception)
            {
                _logger.LogError("Somthing went wrong during generating PDF");
                throw;
            }
        }

        public async Task<byte[]?> DownloadUsersPdfAsync()
        {
            try
            {
                List<UserModel> _response = new List<UserModel>();
                var _data = await this._context.TblUsers.ToListAsync();
                if (_data != null)
                {
                    _response = this._mapper.Map<List<TblUser>, List<UserModel>>(_data);
                }

                var document = await PDFGenerator.createPdfContent(_response, userColumnNames);

                return document.GeneratePdf();
            }
            catch (Exception)
            {
                _logger.LogError("Somthing went wrong during generating PDF");

                throw;
            }
        }
    }
}

