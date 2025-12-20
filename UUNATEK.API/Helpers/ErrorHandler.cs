using UUNATEK.API.Models;

namespace UUNATEK.API.Helpers
{
    public class ErrorHandler
    {
        public static Response GetErrorResponse(int errorCode)
        {
            return errorCode switch
            {
                -1 => ParameterError(),
                -2 => MachineNotAvailable(),
                -3 => ServiceBusy(),
                1 => FailedToGenerate(),
                2 => SoftwareUnAuthorized(),
                _ => new Response
                {
                    Success = false,
                    Message = "Unknown error.",
                    Error = errorCode
                },
            };
        }
        public static Response MachineNotAvailable() =>
            new()
            {
                Success = false,
                Message = "The requested machine is not available.",
                Error = -2
            };

        public static Response ParameterError() => 
            new()
            {
                Success = false,
                Message = "Parameter error",
                Error = -1
            };

        public static Response ServiceBusy() => 
            new()
            {
                Success = false,
                Message = "Services busy.",
                Error = -3
            };

        public static Response FailedToGenerate() =>
            new()
            {
                Success = false,
                Message = "Failed to generate writing file, may need to retry.",
                Error = 1
            };

        public static Response SoftwareUnAuthorized() =>
            new()
            {
                Success = false,
                Message = "Software unauthorized, Need to login to the software again.",
                Error = 2
            };
        public static Response FileNotExist() =>
            new()
            {
                Success = false,
                Message = "File not exist",
                Error = -4 // custom error code for file not exist
            };
    }
}
