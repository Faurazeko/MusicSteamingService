namespace MusicStreamingService.Models
{
	public class Playlist
	{
		public string OwnerUsername { get; set; }
		public bool IsPlaylistPublic { get; set; } = false;
        public double DurationSec
        {
            get
            {
                double duration = 0;
                foreach (var item in Songs)
                    duration += item.Duration.TotalSeconds;

                return duration;
            }
        }
        public string DurationString
        {
            get
            {
                var secondsTotal = (int)Math.Ceiling(DurationSec);

                var sec = secondsTotal % 60;
                var min = (secondsTotal / 60) % 60;
                var hr = (secondsTotal / 60) / 60;

                return $"{hr} hr, {min} min, {sec} sec";
            }
        }
        public List<Song> Songs { get; set; }
    }
}
