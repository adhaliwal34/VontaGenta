#nullable disable

namespace VontaGenta.Services
{
    public class RequestStatus : IRequestStatus
    {
        public bool IsSuccess { get { return string.IsNullOrEmpty(AppError) || AppErrorList.Any(); } }
        // An error summary.
        public string AppError { get; set; }
        public string Status { get; set; }
        // For detailed validation errors. Always set AppError when setting AppErrorList.
        public IEnumerable<KeyValuePair<string, string>> AppErrorList { get; set; }

        public RequestStatus()
        {
            AppErrorList = new List<KeyValuePair<string, string>>();
        }
    }
}
