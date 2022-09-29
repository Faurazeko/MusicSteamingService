namespace MusicStreamingService.Data
{
	public interface IEnvironmentWorker
	{
        public string GetFileStoragePath();
        public string GetMusicFolderPath();
        public string GetTempFolderPath();
    }
}
