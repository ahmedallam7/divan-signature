using UUNATEK.Domain.Enums;

namespace UUNATEK.Domain.Entities;

public class RequestLog
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public RequestStatus Status { get; set; }
    public string? ApprovalResponse { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Pen usage tracking
    public double? DrawingDistanceMm { get; set; }
    public int? StrokeCount { get; set; }
    public TimeSpan? DrawingDuration { get; set; }
    public Guid? PenUsageLogId { get; set; }
    
    // Navigation property
    public PenUsageLog? PenUsageLog { get; set; }
}
