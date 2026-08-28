namespace VontaGenta.Services
{
    public interface IHcaptchaService
    {
        Task<HcaptchaDTO> VerifyResponse(string hcaptchaResponse);
    }
}