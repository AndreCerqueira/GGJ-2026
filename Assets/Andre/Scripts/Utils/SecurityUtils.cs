using System.Security.Cryptography;
using System.Text;

namespace Project.Runtime.Scripts.Utils
{
    public static class SecurityUtils
    {
        public static string HashString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                
                var builder = new StringBuilder();
                foreach (var b in hash)
                    builder.Append(b.ToString("x2"));
                
                return builder.ToString();
            }
        }
    }
}