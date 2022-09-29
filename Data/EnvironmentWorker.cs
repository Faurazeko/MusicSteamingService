namespace MusicStreamingService.Data
{
	public class EnvironmentWorker : IEnvironmentWorker
	{
		private readonly IWebHostEnvironment _env;
        private readonly string _fileStoragePath;
        private readonly string _musicFolderPath;
		private readonly string _tempFolderPath;

        public EnvironmentWorker(IWebHostEnvironment webHostEnvironment)
		{
			_env = webHostEnvironment;

            _fileStoragePath = $"{_env.WebRootPath}/FileStorage";
            _musicFolderPath = $"{_fileStoragePath}/Music";
            _tempFolderPath = $"{_fileStoragePath}/Temp";
        }

        public string GetFileStoragePath() => _fileStoragePath;

        public string GetMusicFolderPath() => _musicFolderPath;

        public string GetTempFolderPath() => _tempFolderPath;

    }
}
