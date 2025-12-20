namespace UUNATEK.API.Models
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }

        public static Response CreateSuccess(string message = "Operation completed successfully.")
        {
            return new Response
            {
                Success = true,
                Message = message,
                Error = 0
            };
        }

        public static Response CreateFailure(string message, int errorCode)
        {
            return new Response
            {
                Success = false,
                Message = message,
                Error = errorCode
            };
        }
    }

    public record MachineReposne(int Error);
}
