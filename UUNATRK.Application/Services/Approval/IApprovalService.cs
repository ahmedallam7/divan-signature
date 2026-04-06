namespace UUNATRK.Application.Services.Approval;

public interface IApprovalService
{
    Task<ApprovalResponse> RequestApprovalAsync(string imagePath, Guid requestId);
}

public record ApprovalResponse(
    bool IsApproved,
    string Message,
    string? RejectionReason = null
);
