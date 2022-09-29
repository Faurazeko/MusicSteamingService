using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MusicStreamingService.Models;

namespace MusicSteamingService
{
	public static class Utils
	{
		public static string GetFileHash(Stream inputStream)
		{
			string hash = BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(inputStream));

			return hash;
		}

		public static DbSong GenerateDbSong(string filePath)
		{
			try
			{
				var file = TagLib.File.Create(filePath);
				var fileTag = file.Tag;
				var fileName = file.Name.Split("/").Last();

				var song = new DbSong()
				{
					Album = fileTag.Album,
					FileName = fileName,
					Artist = String.Join(", ", fileTag.AlbumArtists),
					Title = fileTag.Title,
					Year = fileTag.Year,
					Duration = file.Properties.Duration,
					CreatedUtcDateTime = File.GetCreationTimeUtc(filePath),
					BitRate = file.Properties.AudioBitrate,
					Genre = String.Join(", ", fileTag.Genres),
				};

				using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
					song.Sha1Hash = GetFileHash(fs);

				return song;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
