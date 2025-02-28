using System.Security.Cryptography;
using System.Text;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class GenerateCodeServices
    {
        public string GenerateCode(string code)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(code));

            string hexString = string.Concat(bytes.Select(b => b.ToString("x2")));

            return hexString;
        }
    }
}
