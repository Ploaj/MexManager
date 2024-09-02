using System.ComponentModel.DataAnnotations;

namespace mexLib.DataValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MexFilePathValidatorAttribute : ValidationAttribute
    {
        private bool CanBeNull { get; set; } = true;

        private bool IsMusic { get; set; } = false;

        public MexFilePathValidatorAttribute() { }

        public MexFilePathValidatorAttribute(bool nullable, bool isMusic = false)
        {
            CanBeNull = nullable;
            IsMusic = isMusic;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (MexWorkspace.LastOpened == null)
            {
                return new ValidationResult("Workspace is not opened.");
            }

            if (!(value is string stringValue))
            {
                return new ValidationResult("Value is not a valid string.");
            }

            if (string.IsNullOrEmpty(stringValue))
            {
                if (CanBeNull)
                    return ValidationResult.Success;
                else
                    return new ValidationResult("File is required.");
            }

            string filePath = MexWorkspace.LastOpened.GetFilePath(IsMusic ? $"audio\\{stringValue}" : stringValue);
            if (!MexWorkspace.LastOpened.FileManager.Exists(filePath))
            {
                return new ValidationResult("File not found in Filesystem.");
            }

            return ValidationResult.Success;
        }
    }

}
