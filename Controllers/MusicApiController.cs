using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Data;
using MusicStreamingService.Models;
using System.IO.Compression;
using System.IO;
using System;
using AutoMapper;
using NAudio;
using NAudio.Wave;
using NLayer;
using NLayer.NAudioSupport;
using NAudio.MediaFoundation;
using System.Reflection.Metadata;
using System.Net;

#pragma warning disable CS8602

namespace MusicSteamingService.Controllers
{
	[Route("api/music")]
	[ApiController]
	[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class MusicApiController : ControllerBase
	{
		private readonly IRepository _repository;
		private readonly IEnvironmentWorker _envWorker;
		private readonly IMapper _mapper;

		public MusicApiController(IRepository repository, IMapper mapper, IEnvironmentWorker environmentWorker)
		{
			_repository = repository;
			_envWorker = environmentWorker;
			_mapper = mapper;
		}

		[HttpGet(Name = "MusicList")]
		public IActionResult GetMusicList([FromQuery] string? playlist)
		{
            string username = GetUsername();

			if (!string.IsNullOrEmpty(playlist) && (playlist.ToLower() != username.ToLower()))
			{
				var user = _repository.GetUser(playlist);

				if (user == null)
					return NotFound("This playlist doesnt exist!");

				if (!user.IsPlaylistPublic)
					return StatusCode(403, "This playlist is private!");

				username = playlist;
            }

            var isPublic = _repository.GetUser(username).IsPlaylistPublic;

            username = username.ToLower();

            var usersongs = _repository.GetUserSongs(e => e.User.Username.ToLower() == username);
			var responseSongs = new List<Song>();

			foreach (var item in usersongs)
			{
				var song = _mapper.Map<Song>(item);

				responseSongs.Add(song);
			}

			responseSongs.OrderBy(e => e.UserIndex);

			var playlistObj = new Playlist() {OwnerUsername = username, Songs = responseSongs, IsPlaylistPublic = isPublic};

			return Ok(playlistObj);
		}

		[HttpPost]
		[RequestSizeLimit(100_000_000)]
		public IActionResult UploadMusic([FromForm] List<IFormFile> songFiles)
		{
			var username = GetUsername();
			var user = _repository.GetUser(username);

			songFiles = (from song in songFiles where song.FileName.Split('.').Last().ToLower() == "mp3" select song).ToList();

			if (songFiles.Count <= 0)
				return BadRequest("Request should include only mp3 files and more than 0 files.");

			var userSongsCount = _repository.GetUserSongs(username).Count();

			var responseSongs = new List<Song>();

			foreach (var item in songFiles)
			{
				var stream = new MemoryStream();
				item.CopyTo(stream);
				stream.Position = 0;

				if (!CheckIfMp3FileValid(stream))
					continue;

				var hash = Utils.GetFileHash(stream);

				var songsByHash = _repository.GetSongs(e => e.Sha1Hash == hash);
				var alreadyExists = songsByHash.Any();

				++userSongsCount;

				if (alreadyExists)
				{
					var alreadyInUserLibrary = _repository
						.GetUserSongs(e => e.User.Username == username && e.DbSong.Sha1Hash == hash)
						.Any();

					if (alreadyInUserLibrary)
					{
						--userSongsCount;
						continue;
					}

					var hashSong = songsByHash.FirstOrDefault()!;
					var dbUserSong = GetUserSongFromDbSong(hashSong, user, userSongsCount);
					var song = _mapper.Map<Song>(dbUserSong);

					responseSongs.Add(song);
					_repository.AddUserSong(dbUserSong);
				}
				else
				{
					var filePath = $"{_envWorker.GetMusicFolderPath()}/{item.FileName}";
					var changeTitle = false;

					if (System.IO.File.Exists(filePath))
					{
						filePath = $"{_envWorker.GetMusicFolderPath()}/{Guid.NewGuid()}.mp3";
						changeTitle = true;
                    }

					SaveSong(item, filePath);

					var dbSong = Utils.GenerateDbSong(filePath);

					if (dbSong == null)
					{
						System.IO.File.Delete(filePath);
						--userSongsCount;
						continue;
					}
					_repository.AddSong(dbSong);

					var userSong = GetUserSongFromDbSong(dbSong, user, userSongsCount);

					if (changeTitle && string.IsNullOrEmpty(userSong.Title))
						userSong.Title = item.FileName;

					_repository.AddUserSong(userSong);

					var song = _mapper.Map<Song>(userSong);
					responseSongs.Add(song);
				}
				_repository.SaveChanges();
			}

			if (responseSongs.Count <= 0)
				return BadRequest();

			return CreatedAtRoute("MusicList", null, responseSongs);
		}

		[NonAction]
		public bool CheckIfMp3FileValid(MemoryStream stream)
		{
			var array = stream.ToArray();

			var checkString = string.Join(' ', array.Take(3));
			const string mp3Signature = "73 68 51";

			if (checkString == mp3Signature)
				return true;

			return false;
		}

		[NonAction]
		public DbUserSong GetUserSongFromDbSong(DbSong dbSong, User user, int userIndex)
		{
			var userSong = _mapper.Map<DbUserSong>(dbSong);
			userSong.Id = 0;
			userSong.UserIndex = userIndex;
			userSong.User = user;
			userSong.DbSong = dbSong;

			return userSong;
		}

		[NonAction]
		public void SaveSong(IFormFile file, string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
				file.CopyTo(fs);
		}

		[NonAction]
		public string GetUsername() => User.FindFirst("username").Value;

		[HttpDelete("{fileName}")]
		public IActionResult DeleteSong(string fileName)
		{
			var username = GetUsername();

			var userSongs = _repository.GetUserSongs(e => e.DbSong.FileName == fileName);
			if (userSongs.Count() > 1)
			{
				var user = _repository.GetUser(username);

				_repository.DeleteUserSong(user.Id, userSongs.FirstOrDefault().DbSongId);
				_repository.SaveChanges();

				return Ok();
			}
			else if (!userSongs.Any())
				return BadRequest("No such file");


			_repository.DeleteSong(fileName);
			_repository.SaveChanges();

			ReIndexUserSongs(username);

			return Ok();
		}

		[NonAction]
		public void ReIndexUserSongs(string username)
		{
			var userSongs = _repository.GetUserSongs(e => e.User.Username == username).OrderBy(e => e.UserIndex).ToList();

			for (int i = 0; i < userSongs.Count(); i++)
			{
				var currentIndex = i + 1;
				userSongs[i].UserIndex = currentIndex;
			}

			_repository.SaveChanges();
		}

		[HttpGet("downloadAll")]
		public IActionResult DownloadAllSongs([FromQuery] string? playlistName)
		{
			var username = User.FindFirst("username").Value;

			if(playlistName != null)
			{
				var user = _repository.GetUser(playlistName);
				if(user == null)
					return BadRequest("No such user!");

				username = playlistName;
            }

			var zipFilePath = $"{_envWorker.GetTempFolderPath()}/{username}-AllTheSongs.zip";

			var songs = _repository.GetUserSongs(username);

			using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
			{
				foreach (var item in songs)
				{
					var filename = item.DbSong.FileName;
					archive.CreateEntryFromFile($"{_envWorker.GetMusicFolderPath()}/{filename}", filename);
				}
			}

			byte[] content = System.IO.File.ReadAllBytes(zipFilePath);
			System.IO.File.Delete(zipFilePath);

			return File(content, "APPLICATION/octet-stream", "AllTheSongs.zip");
		}

		[HttpGet("download/{filename}")]
		public IActionResult DownloadSong(string filename)
		{
			var songFilename = $"{_envWorker.GetMusicFolderPath()}/{filename}";

			byte[] content = System.IO.File.ReadAllBytes(songFilename);

			return File(content, "APPLICATION/octet-stream", filename);
		}

		[HttpPut("{fileName}")]
		public IActionResult EditSong(
			[FromForm] string? title, [FromForm] string? artist, [FromForm] string? album, 
			[FromForm] string? genre, [FromForm] string? year, [FromForm] string userIndex, 
			string fileName)
		{
			var username = GetUsername();
			var userSong = _repository.GetUserSongs(e => e.DbSong.FileName == fileName && e.User.Username == username).FirstOrDefault();

			if (userSong == null)
				return BadRequest("No such file!");

			var succ = uint.TryParse(year, out uint yearParsed);
			if (!succ)
				yearParsed = 0;

			succ = int.TryParse(userIndex, out int userIndexParsed);
			if (!succ)
				userIndexParsed = userSong.UserIndex;

			userSong.Title = title;
			userSong.Artist = artist;
			userSong.Album = album;
			userSong.Genre = genre;
			userSong.Year = yearParsed;
			userSong.UserIndex = userIndexParsed - 1;

			_repository.SaveChanges();

			userSong = null;

			ReIndexUserSongs(username);

			return Ok();
		}

		[HttpGet("{fileName}")]
		public IActionResult GetSong(string fileName, int bitRate)
		{

			var exists = _repository.GetSongs(e => e.FileName == fileName).Any();

			if (!exists)
				return BadRequest("File does not exists!");


			var SampleRates = new int[] 
			{
				1000, 8000, 11025, 16000, 22050, 32000, 44100, 47250, 48000, 50000, 50400, 64000, 88200,
				96000, 176400, 192000, 352800, 2822400, 5644800, 11289600, 22579200
			};

			var encodingBitrate = SampleRates[0];

			foreach (var item in SampleRates)
			{
				if (item <= bitRate)
					encodingBitrate = item;
				else
					break;
			}

			if(bitRate <= 1000)
                encodingBitrate = 8000;
            else
				bitRate = encodingBitrate;

			byte[] content;

			var sourcePath = $"{_envWorker.GetMusicFolderPath()}/{fileName}";

			if (!System.IO.File.Exists(sourcePath))
				return BadRequest("File doesnt exists!");


            using (var reader = new Mp3FileReader($"{_envWorker.GetMusicFolderPath()}/{fileName}"))
			{
				var isInvalid = bitRate <= 0;
				var isEqualOrGrater = bitRate >= reader.WaveFormat.SampleRate;

                var outputFileName = $"{_envWorker.GetMusicFolderPath()}/{bitRate}/{fileName}";

                if (isInvalid || isEqualOrGrater)
					return Redirect($"/filestorage/music/{fileName}");
				else if (System.IO.File.Exists(outputFileName)) { }
				else
				{
					Directory.CreateDirectory($"{_envWorker.GetMusicFolderPath()}/{bitRate}");

					var outFormat = new WaveFormat(bitRate, reader.WaveFormat.Channels);

					using (var resampler = new MediaFoundationResampler(reader, outFormat))
					{
						var mediaType = MediaFoundationEncoder.SelectMediaType(
							AudioSubtypes.MFAudioFormat_MP3,
							new WaveFormat(encodingBitrate, reader.WaveFormat.Channels),
							bitRate);

						using (var enc = new MediaFoundationEncoder(mediaType))
							enc.Encode(outputFileName, resampler);
					}
				}

			}
            return Redirect($"/filestorage/music/{bitRate}/{fileName}");
        }

		[HttpPost("existing")]
		public IActionResult AddExistingSongToPlaylist([FromQuery] string filename)
		{
            var username = GetUsername();
            var user = _repository.GetUser(username);

			var song = _repository.GetSongs( e => e.FileName == filename ).FirstOrDefault();

			if (song == null)
				return BadRequest("No such song!");

			var songsCount = _repository.GetUserSongs(username).Count();

            var userSong = GetUserSongFromDbSong(song, user, songsCount);

			_repository.AddUserSong(userSong);
			_repository.SaveChanges();

			return Ok();
        }

		[HttpDelete]
		public IActionResult EmptyPlaylist()
		{
            var username = GetUsername();
            var user = _repository.GetUser(username);

			return Ok();
        }
	}
}
