using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions
{
    public static class ModelStateExtension
    {
        // Extension method
        public static List<string> GetErrors(this ModelStateDictionary modelState)
        {
            var errors = new List<string>();

            foreach (var value in modelState.Values)
                errors.AddRange(value.Errors.Select(error => error.ErrorMessage)); // Mesmo que foreach (var error in value.Errors) errors.Add(error.ErrorMessage);

            return errors;
        }
    }
}
