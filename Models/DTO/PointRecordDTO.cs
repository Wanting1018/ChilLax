namespace ChilLaxFrontEnd.Models.DTO
{
    public class PointRecordDTO
    {
        public string ModifiedSource { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int ModifiedAmount { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
