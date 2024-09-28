using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Unnati.Models
{
    public class JwtSettings
    {
        public string securitykey { get; set; }

        public int expirationSeconds { get; set; }
    }
}
