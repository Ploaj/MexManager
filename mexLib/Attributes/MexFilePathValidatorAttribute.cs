using System.ComponentModel.DataAnnotations;

namespace mexLib.Attributes
{
    public enum MexFilePathType
    {
        Files,
        Audio,
        Assets,
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MexFilePathValidatorAttribute : ValidationAttribute
    {
        private bool CanBeNull { get; set; } = true;

        private string Folder { get; set; } = "";

        private MexFilePathType Type { get; set; }

        public MexFilePathValidatorAttribute(MexFilePathType type, bool nullable = true) 
        {
            Type = type;
            CanBeNull = nullable;
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

            string filePath = "";
            var ws = MexWorkspace.LastOpened;

            switch (Type)
            {
                case MexFilePathType.Files: 
                    filePath = ws.GetFilePath(stringValue); 
                    break;
                case MexFilePathType.Audio: 
                    filePath = ws.GetFilePath($"audio//{stringValue}"); 
                    break;
                case MexFilePathType.Assets: 
                    filePath = ws.GetAssetPath($"{stringValue}"); 
                    break;
            }

            if (!MexWorkspace.LastOpened.FileManager.Exists(filePath))
            {
                return new ValidationResult("File not found");
            }

            return ValidationResult.Success;
        }
    }

}
