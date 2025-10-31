namespace TinyUrl.Api.Models
{
    public class TinyUrls
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ShortURL { get; set; } = string.Empty;
        public string OriginalURL { get; set; } = string.Empty;
        public int TotalClicks { get; set; } = 0;
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}