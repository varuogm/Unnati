namespace Unnati.Controllers
{
    using Unnati.Helper;
    using Unnati.Repos;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using Microsoft.AspNetCore.Authorization;
    using System.Linq;
    using System.IO.Compression;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly UnnatiContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IWebHostEnvironment environment, UnnatiContext context ,ILogger<ProductController> logger)
        {
            this.environment = environment;
            this._context = context;
            _logger = logger;
        }
        
        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(string productcode)
        {
            string Imageurl = string.Empty;
            string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilepath(productcode);
                string imagepath = Filepath + "\\" + productcode + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    Imageurl = hosturl + "/Upload/product/" + productcode + "/" + productcode + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound();
            }
            return Ok(Imageurl);

        }

        [HttpGet("GetMultiImage")]
        public async Task<IActionResult> GetMultiImage(string productcode)
        {
            List<string> Imageurl = new List<string>();
            string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            
            try
            {
                string Filepath = GetFilepath(productcode);

                if (Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagepath = Filepath + "\\" + filename;
                        if (System.IO.File.Exists(imagepath))
                        {
                            string _Imageurl = hosturl + "/Upload/product/" + productcode + "/" + filename;
                            Imageurl.Add(_Imageurl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(Imageurl);
            }
            return Ok(Imageurl);

        }

        [HttpPut("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile formFile, string productcode)
        {
            APIResponse response = new APIResponse();
            try
            {
                string Filepath = GetFilepath(productcode);
                if (!Directory.Exists(Filepath))
                {
                    Directory.CreateDirectory(Filepath);
                }

                string imagepath = Filepath + "\\" + productcode + ".png";

                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }

                using (FileStream stream = System.IO.File.Create(imagepath))
                {
                    await formFile.CopyToAsync(stream);
                    response.ResponseCode = 200;
                    response.Result = "Image Uploaded Successfully";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.ResponseCode = 500;
                _logger.LogError(ex.Message,ex);
            }
            return Ok(response);
        }

        [HttpPut("MultiUploadImage")]
        public async Task<IActionResult> MultiImageUpload(IFormFileCollection filecollection, string productcode)
        {
            APIResponse response = new APIResponse();
            int passcount = 0; int errorcount = 0;
            try
            {
                string Filepath = GetFilepath(productcode);
                if (!Directory.Exists(Filepath))
                {
                    Directory.CreateDirectory(Filepath);
                }
                foreach (var file in filecollection)
                {
                    string imagepath = Filepath + "\\" + file.FileName;
                    if (System.IO.File.Exists(imagepath))
                    {
                        System.IO.File.Delete(imagepath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        await file.CopyToAsync(stream);
                        passcount++;
                    }
                }
            }
            catch (Exception ex)
            {
                errorcount++;
                response.Message = ex.Message;
                _logger.LogError(ex.Message, ex);
            }
            response.ResponseCode = 200;
            response.Result = passcount + " Files uploaded & " + errorcount + " files failed";
            return Ok(response);
        }

        [HttpGet("Download")]
        public async Task<IActionResult> Download(string productcode)
        {
            try
            {
                string Filepath = GetFilepath(productcode);
                string imagepath = Filepath + "\\" + productcode + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    MemoryStream stream = new MemoryStream();
                    using (FileStream fileStream = new FileStream(imagepath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(stream);
                    }
                    stream.Position = 0;
                    return File(stream, "image/png", productcode + ".png");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound();
            }
        }

        [HttpGet("Remove")]
        public async Task<IActionResult> Remove(string productcode)
        {
            try
            {
                string Filepath = GetFilepath(productcode);
                string imagepath = Filepath + "\\" + productcode + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                    return Ok("Image deleted sucessfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound();
            }

        }

        [HttpGet("Multiremove")]
        public async Task<IActionResult> MultipleRemove(string productcode)
        {
            try
            {
                string Filepath = GetFilepath(productcode);
                if (Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        fileInfo.Delete();
                    }
                    return Ok("Images removed successfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound();
            }
        }

        [HttpPut("DBMultiUploadImage")]
        public async Task<IActionResult> DBMultipleUploadImage(IFormFileCollection filecollection, string productcode)
        {
            APIResponse response = new APIResponse();
            int passcount = 0; int errorcount = 0;
            try
            {
                foreach (var file in filecollection)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        this._context.TblProductimages.Add(new Repos.Models.TblProductimage()
                        {
                            Productcode = productcode,
                            Productimage = stream.ToArray()
                        });
                        await this._context.SaveChangesAsync();
                        passcount++;
                    }
                }
            }
            catch (Exception ex)
            {
                errorcount++;
                response.Message = ex.Message;
                _logger.LogError(ex.Message, ex);
            }
            response.ResponseCode = 200;
            response.Result = passcount + " Files uploaded & " + errorcount + " files failed";
            
            return Ok(response);
        }


        [HttpGet("GetDBMultiImage")]
        public async Task<IActionResult> GetDBMultipleImage(string productcode)
        {
            List<string> Imageurl = new List<string>();
            try
            {
                var _productimage = this._context.TblProductimages.Where(item => item.Productcode == productcode).ToList();
                if (_productimage != null && _productimage.Count > 0)
                {
                    _productimage.ForEach(item =>
                    {
                        Imageurl.Add(Convert.ToBase64String(item.Productimage));
                    });
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound();
            }
            return Ok(Imageurl);

        }


        [HttpGet("DBDownload")]
        public async Task<IActionResult> dbdownload(string productcode)
        {
            try
            {
                var _productimages = await this._context.TblProductimages.Where(item => item.Productcode == productcode).ToListAsync();

                if (_productimages != null && _productimages.Count > 0)
                {
                    var archiveStream = new MemoryStream();
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var image in _productimages)
                        {
                            var entry = archive.CreateEntry(image.Productcode + ".png");
                            using (var entryStream = entry.Open())
                            {
                                entryStream.Write(buffer: image.Productimage, 0, count: image.Productimage.Length);
                            }
                        }
                    }
                    archiveStream.Position = 0;

                    return File(archiveStream, "application/zip", productcode + ".zip");
                }
                return NotFound();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound();
            }
        }

        [NonAction]
        private string GetFilepath(string productcode)
        {
            return this.environment.WebRootPath + "\\Upload\\product\\" + productcode;
        }
    }
}