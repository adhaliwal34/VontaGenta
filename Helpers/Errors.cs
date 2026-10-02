
namespace VontaGenta.Models
{
    public class Errors
    {
        public Dictionary<string, string> ErrorList { get; set; }

        public void AddError(string property, string errorMessage)
        {
            if (ErrorList.TryGetValue(property, out var existing))
            {
                ErrorList[property] = existing + ". " + errorMessage;
            }
            else
            {
                ErrorList.Add(property, errorMessage);
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetErrors()
        {
            var errors = new List<KeyValuePair<string, string>>();

            errors.AddRange(ErrorList.Select(e => new KeyValuePair<string, string>(e.Key, e.Value)));

            return errors;
        }

        public bool Any()
        {
            return ErrorList.Count > 0;
        }

        public Errors()
        {
            ErrorList = new Dictionary<string, string>();
        }
    }
}
