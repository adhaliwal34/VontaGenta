
namespace VontaGenta.Models
{
    public class Errors
    {
        public Dictionary<string, string> ErrorList { get; set; }

        public void AddError(string property, string errorMessage)
        {
            if (ErrorList.ContainsKey(property))
            {
                ErrorList[property] = ErrorList[property] + ". " + errorMessage;
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
            return ErrorList.Any();
        }

        public Errors()
        {
            ErrorList = new Dictionary<string, string>();
        }
    }
}
