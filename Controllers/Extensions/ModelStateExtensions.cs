using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VontaGenta.Controllers
{
    public static class ModelStateExtensions
    {
        public static void AddValidationErrors(this ModelStateDictionary dict, IEnumerable<KeyValuePair<string, string>> errors)
        {
            foreach (var error in errors)
            {
                // Keys which are not in the form will not be displayed, so best to leave blank in that case.
                var key = dict.ContainsKey(error.Key) ? error.Key : "";

                dict.AddModelError(key, error.Value);
            }
        }

    }
}
