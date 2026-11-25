#nullable disable
using System.Text;
using Newtonsoft.Json.Linq;

namespace VontaGenta.Services
{
    public class HcaptchaService : IHcaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string verificationEndPoint = "https://hcaptcha.com/siteverify";

        public HcaptchaService(HttpClient httpClient,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<HcaptchaDTO> VerifyResponse(string hcaptchaResponse)
        {
            var response = new HcaptchaDTO();

            if (string.IsNullOrEmpty(hcaptchaResponse))
            {
                response.AppError = "Invalid hCaptcha response.";
                return response;
            }

            try
            {
                var data = $"response={hcaptchaResponse}&secret={_config["Hcaptcha:Secret"]}";

                StringContent content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                // using SendAsync to set the authentication header on a get request.
                // See https://stackoverflow.com/a/40707446
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, verificationEndPoint))
                {
                    if (content != null)
                    {
                        requestMessage.Content = content;
                    }
                    response.Response = await _httpClient.SendAsync(requestMessage);
                }
            }
            catch
            {
                // TODO Log exception
                response.AppError = "Error on sending request to the hCaptcha end point.";
                return response;
            }

            try
            {
                response.ResponseString = await response.Response.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(response.ResponseString);
                bool success = (bool)responseJson["success"];

                if (success)
                {
                    return response;
                }
                else
                {
                    response.AppError = "Validation failed";
                    return response;
                }
            }
            catch
            {
                // TODO Log exception
                response.AppError = "Error on reading the response.";
                return response;
            }
        }

    }
}
