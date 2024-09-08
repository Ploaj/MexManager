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

        private MexFilePathType Type { get; set; }

        public MexFilePathValidatorAttribute(MexFilePathType type, bool nullable = true)
        {
            Type = type;
            CanBeNull = nullable;
        }

        public string GetFullPath(MexWorkspace ws, string fileName)
        {
            string filePath = "";

            if (fileName.EndsWith("."))
                fileName += "usd";
            else
            if (Path.GetExtension(fileName) == "")
                fileName += ".usd";

            switch (Type)
            {
                case MexFilePathType.Files:
                    filePath = ws.GetFilePath(fileName);
                    break;
                case MexFilePathType.Audio:
                    filePath = ws.GetFilePath($"audio//{fileName}");
                    break;
                case MexFilePathType.Assets:
                    filePath = ws.GetAssetPath($"{fileName}");
                    break;
            }

            return filePath;
        }

        public ValidationResult? IsValid(MexWorkspace? workspace, object? value)
        {
            if (workspace == null)
            {
                return new ValidationResult("Workspace is not opened.");
            }

            if (value is not string stringValue)
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

            string filePath = GetFullPath(workspace, stringValue);

            if (!workspace.FileManager.Exists(filePath))
            {
                return new ValidationResult("File not found");
            }

            return ValidationResult.Success;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            //if (MexWorkspace.LastOpened == null)
            //{
            //    return new ValidationResult("Workspace is not opened.");
            //}

            //if (!(value is string stringValue))
            //{
            //    return new ValidationResult("Value is not a valid string.");
            //}

            //if (string.IsNullOrEmpty(stringValue))
            //{
            //    if (CanBeNull)
            //        return ValidationResult.Success;
            //    else
            //        return new ValidationResult("File is required.");
            //}

            //string filePath = GetFullPath(MexWorkspace.LastOpened, stringValue);

            //if (!MexWorkspace.LastOpened.FileManager.Exists(filePath))
            //{
            //    return new ValidationResult("File not found");
            //}

            return ValidationResult.Success;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MexFilePathValidatorCallback : Attribute
    {
        public string CallbackMethodName { get; }

        /// <summary>
        /// The callback method name should return MexFilePathError
        /// The input is the new path
        /// </summary>
        /// <param name="callbackMethodName"></param>
        public MexFilePathValidatorCallback(string callbackMethodName)
        {
            CallbackMethodName = callbackMethodName;
        }
    }

    public class MexFilePathError
    {
        public string Message { get; internal set; }

        public MexFilePathError(string message)
        {
            Message = message;
        }
    }
}
