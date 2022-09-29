namespace MusicStreamingService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsPlaylistPublic { get; set; } = false;
        public DateTime ForcedLogoutTime { get; set; } = DateTime.MinValue;
    }
}
