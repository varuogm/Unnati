using Unnati.Models;

namespace Unnati.Service
{
    public interface IEmailService
    {
        Task SendEmail(Mailrequest mailrequest);
    }
}