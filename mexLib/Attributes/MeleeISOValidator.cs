using PropertyModels.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
namespace mexLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MeleeISOValidator : ValidationAttribute
    {
        private const string MeleeUSA102 = "0e63d4223b01d9aba596259dc155a174";

        public MeleeISOValidator()
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not string path ||
                path.IsNullOrEmpty())
                return new ValidationResult("ISO path not set");

            if (!File.Exists(path))
                return new ValidationResult("File not found");

            using MD5 md5 = MD5.Create();
            string hash = ComputeHash(path, md5);

            if (!hash.Equals(MeleeUSA102))
                return new ValidationResult($"ISO hash verification failed.\nExpected:\n\"{MeleeUSA102}\"\nGot:\n\"{hash}\"");

            return ValidationResult.Success;
        }

        private static string ComputeHash(string filePath, HashAlgorithm hashAlgorithm, int bufferSize = 4 * 1024 * 1024) // 4MB buffer
        {
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
            byte[] hashBytes = hashAlgorithm.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
