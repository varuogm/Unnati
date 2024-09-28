using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Unnati.Repos;
using Unnati.Repos.Models;
using Unnati.Service;

namespace Unnati.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly UnnatiContext _context;
        public RefreshHandler(UnnatiContext context)
        {
            this._context = context;
        }
        public async Task<string> GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string refreshtoken = Convert.ToBase64String(randomnumber);
                var Existtoken = this._context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username).Result;
                if (Existtoken != null)
                {
                    Existtoken.Refreshtoken = refreshtoken;
                }
                else
                {
                    await this._context.TblRefreshtokens.AddAsync(new TblRefreshtoken
                    {
                        Userid = username,
                        Tokenid = new Random().Next().ToString(),
                        Refreshtoken = refreshtoken
                    });
                }
                await this._context.SaveChangesAsync();

                return refreshtoken;

            }
        }
    }
}
