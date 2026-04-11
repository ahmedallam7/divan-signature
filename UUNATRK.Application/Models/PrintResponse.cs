namespace UUNATRK.Application.Models
{
    public class PrintResponse
    {
        public string Message { get; set; } = "";
        public int CommandsSent { get; set; }
        public int Copies { get; set; }
        public int TotalCommandsSent { get; set; }
        
        // Pen usage tracking
        public PenUsageMetrics? Usage { get; set; }
        public List<string>? Warnings { get; set; }
    }
}
