using System.ComponentModel.DataAnnotations;

namespace MusicStreamingService.Models
{
    public class DbSong
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Sha1Hash { get; set; }
        public bool IsConfirmed { get; set; } = false;


        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? Genre { get; set; }
        public uint Year { get; set; }
        public TimeSpan Duration { get; set; }
        [DataType("datetime2")]
        public DateTime CreatedUtcDateTime { get; set; }
        public int BitRate { get; set; }
    }
}
