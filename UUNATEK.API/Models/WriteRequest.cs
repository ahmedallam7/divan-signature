namespace UUNATEK.API.Models
{
    public class WriteRequest
    {
        public string FilePath { get; set; } = null!;
        public string Width { get; set; } = "210mm";
        public string Height { get; set; } = "297mm";
        public string XPosition { get; set; } = "100mm";
        public string YPosition { get; set; } = "100mm";
        public int Scale { get; set; } = 1;
        public int Rotation { get; set; } = 0;
        public int Clear { get; set; } = 1;
        public int Start { get; set; } = 1;
    }
}
