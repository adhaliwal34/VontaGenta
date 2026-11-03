#nullable disable

namespace VontaGenta.Helpers
{

    public class ErrorHelpers
    {
        protected string GetExceptionError(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }
    }

}
