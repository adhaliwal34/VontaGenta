#nullable disable

namespace VontaGenta.Services
{
    public class RequestStatus : IRequestStatus
    {
        public bool IsSuccess { get { return string.IsNullOrEmpty(Error) || ErrorList.Any(); } }
        // An error summary.
        public string Error { get; set; }
        public string Status { get; set; }
        // For detailed validation errors. Always set Error when setting ErrorList.
        public IEnumerable<KeyValuePair<string, string>> ErrorList { get; set; }

        public RequestStatus()
        {
            ErrorList = new List<KeyValuePair<string, string>>();
        }
    }
}
