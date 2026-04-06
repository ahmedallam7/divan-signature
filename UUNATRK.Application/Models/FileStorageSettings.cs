namespace UUNATRK.Application.Models;

public class FileStorageSettings
{
    public string UploadPath { get; set; } = "wwwroot/uploads";
    public int MaxImageSizeMB { get; set; } = 10;
    public int MaxSvgSizeMB { get; set; } = 5;
}
