namespace MusicStreamingService.Models
{
    public class Song
    {
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public uint Year { get; set; }

        private string _path;
        public string Path 
        {
            get => _path;
            set 
            {
                _path = $"/FileStorage/Music/{value}";
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration { 
            get 
            {
                return _duration;
            } 
            set 
            {
                _duration = value;

                var durationString = "";

                if (value > TimeSpan.FromHours(1))
                    durationString = value.ToString("hh\\:mm\\:ss");
                else
                    durationString = value.ToString("mm\\:ss");

                DurationString = durationString;
            } 
        }
        public string DurationString { get; private set; } = "";
        public DateTime CreatedUtcDateTime { get; set; }
        public int BitRate { get; set; }
        public int UserIndex { get; set; }
        public DateTime AddedUtcDateTime { get; set; }
    }
}
