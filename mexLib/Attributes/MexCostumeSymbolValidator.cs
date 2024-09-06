using HSDRaw;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace mexLib.Attributes
{
    public class MexCostumeSymbolValidatorAttribute : ValidationAttribute
    {
        public MexCostumeSymbolValidatorAttribute()
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // get file path
            //if (MexWorkspace.LastOpened == null)
            //    return ValidationResult.Success;

            //// Get the value of the other property
            //var costume = value as MexCostumeFile;
            //if (costume == null)
            //{
            //    return ValidationResult.Success;
            //}

            //// check if file exists
            //var fullPath = MexWorkspace.LastOpened.GetFilePath(costume.FileName);
            //if (!MexWorkspace.LastOpened.FileManager.Exists(fullPath))
            //    return new ValidationResult("File not found!");

            //// check if symbol is in file
            //if (!costume.GetSymbolFromFile(MexWorkspace.LastOpened))
            //{
            //    return new ValidationResult("Symbols not found; costume may be corrupted");
            //}

            return ValidationResult.Success;
        }
    }
}
