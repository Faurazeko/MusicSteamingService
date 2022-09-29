using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamingService.Models
{
    public class DbUserSong
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? Genre { get; set; }
        public uint? Year { get; set; }

        public int DbSongId { get; set; }
        public DbSong DbSong { get; set; }

        public int UserId { get; set; }
        public User User { get; set;}

        public int UserIndex { get; set; }

        [DataType("datetime2")]
        public DateTime AddedUtcDateTime { get; set; } = DateTime.UtcNow;

    }
}
