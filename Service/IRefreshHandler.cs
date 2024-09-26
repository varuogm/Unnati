namespace Unnati.Service
{
    public interface IRefreshHandler
    {
        Task<string> GenerateToken(string username);
    }
}
