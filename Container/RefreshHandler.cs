using Unnati.Repos;
using Unnati.Service;

namespace Unnati.Container
{
    public class RefreshHandler : IRefreshHandler
    {

        private readonly UnnatiContext _context;
        public RefreshHandler( UnnatiContext  context)
        {
            _context = context;
        }

        public Task<string> GenerateToken(string username)
        {
            throw new NotImplementedException();
        }
    }
}
