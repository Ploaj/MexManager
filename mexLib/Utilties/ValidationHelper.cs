using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mexLib.Utilties
{
    public static class ValidationHelper
    {
        public static bool AreAllPropertiesValid(object obj)
        {
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();

            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                // Skip properties without validation attributes
                var attributes = property.GetCustomAttributes(typeof(ValidationAttribute), true);
                if (attributes.Length == 0) continue;

                // Get the property value
                var value = property.GetValue(obj);

                // Validate the property
                bool isValid = Validator.TryValidateProperty(value, new ValidationContext(obj) { MemberName = property.Name }, results);

                if (!isValid)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
