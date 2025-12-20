namespace UUNATEK.API.Models
{
    public class MachineResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }

        public static MachineResponse CreateSuccess(string message = "Operation completed successfully.")
        {
            return new MachineResponse
            {
                Success = true,
                Message = message,
                Error = 0
            };
        }

        public static MachineResponse CreateFailure(string message, int errorCode)
        {
            return new MachineResponse
            {
                Success = false,
                Message = message,
                Error = errorCode
            };
        }
    }
}
