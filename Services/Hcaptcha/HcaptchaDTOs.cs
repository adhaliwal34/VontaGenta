#nullable disable

namespace VontaGenta.Services
{
    public class HcaptchaDTO : RequestStatus
    {
        public HttpResponseMessage Response { get; set; }

        public string ResponseString { get; set; }
    }

}
