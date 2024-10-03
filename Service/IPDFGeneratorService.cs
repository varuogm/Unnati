namespace Unnati.Service
{
    public interface IPDFGeneratorService
    {
        Task<byte[]?> DownloadUsersPdfAsync();
        Task<byte[]?> DownloadProductsPdfAsync();
    }
}

