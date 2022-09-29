using MusicStreamingService.Data;
using System.Runtime.InteropServices;

namespace MusicStreamingService
{
    public class SongDeleterWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private  IEnvironmentWorker _environmentWorker;
        private IRepository _repository;
        private Thread _checkingThread;
        private CancellationToken _cancellationToken;

        int _numberOfDeleteTries = 5;
        int _timeIntervalBetweenDeleteTries = 3000;

        public SongDeleterWorker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        private string[] GetFilesInDirectory(string directory)
        {
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

            return files;
        }

        private bool TryDeleteFile(string filePath)
        {
            var tries = 0;
            while (true)
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (IOException e)
                {
                    if (!IsFileLocked(e))
                        return false;
                    if (++tries > _numberOfDeleteTries)
                        return false;

                    Thread.Sleep(_timeIntervalBetweenDeleteTries);
                }
            }
        }

        private static bool IsFileLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _cancellationToken = stoppingToken;
            _checkingThread = new Thread(() =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    var scope = _serviceProvider.CreateScope();
                    _repository = scope.ServiceProvider.GetService<IRepository>()!;
                    _environmentWorker = scope.ServiceProvider.GetService<IEnvironmentWorker>()!;

                    var filePathes = GetFilesInDirectory(_environmentWorker.GetMusicFolderPath());

                    foreach (var path in filePathes)
                    {
                        var filename = path.Split('\\').Last();

                        var shouldBeDeleted = !_repository.GetSongs(e => e.FileName == filename).Any();

                        if (shouldBeDeleted)
                        {
                            var succ = TryDeleteFile(path);

                            if (!succ)
                                Console.WriteLine($"Failed to delete {filename}. We'll get em next time!");
                            else
                                Console.WriteLine($"{filename} successfully deleted!");
                        }
                    }
                    Thread.Sleep(900000); // 15 mins
                }
            });
            _checkingThread.Start();

            return Task.CompletedTask;
        }
    }
}
