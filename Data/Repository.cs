using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MusicStreamingService.Models;
using System.Linq;

namespace MusicStreamingService.Data
{
    public class Repository : IRepository
    {
        private readonly AppDbContext _dbContext;

        public Repository(IWebHostEnvironment webHostEnvironment, AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool SaveChanges() => _dbContext.SaveChanges() >= 0;

        public User GetUser(string username)
        {
            var usernameLower = username.ToLower();
            return _dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == usernameLower);
        }

        public bool UserExists(string username) => GetUser(username) != null;

        public void CreateUser(string username, string password)
        {
            var existingUser = GetUser(username);

            if(existingUser != null)
            {
                existingUser.Password = password;
                _dbContext.Entry(existingUser).State = EntityState.Modified;
                return;
            }

            var user = new User() { Username = username, Password = password };

            _dbContext.Users.Add(user);
        }

        public void DeleteUser(string username)
        {
            var user = GetUser(username);

            if (user != null)
            {
                _dbContext.Entry(user).State = EntityState.Deleted;
                return;
            }
        }

        public void ChangeUserPassword(string username, string newPassword)
        {
            var user = GetUser(username);

            if (user != null)
            {
                user.Password = newPassword;
                _dbContext.Entry(user).State = EntityState.Modified;
            }
        }

        public List<User> GetUsers() => _dbContext.Users.ToList();



        public List<DbSong> GetSongs() => _dbContext.Songs.ToList();

        public IEnumerable<DbSong> GetSongs(Func<DbSong, bool> predicate) => _dbContext.Songs.Where(predicate);
        public void AddSong(DbSong song)
        {
            var isAlreadyExists = _dbContext.Songs.Where(e => e.Sha1Hash == song.Sha1Hash).Any();

            if (isAlreadyExists)
                return;

            _dbContext.Songs.Add(song);
        }

        public void DeleteSong(string FileName)
        {
            var songs = GetSongs(e => e.FileName == FileName);

            foreach (var item in songs)
                _dbContext.Entry(item).State = EntityState.Deleted;
        }



        private IIncludableQueryable<DbUserSong, DbSong> GetUserSongsWithInclude() => _dbContext.UserSongs.Include(e => e.User).Include(e => e.DbSong);

        public List<DbUserSong> GetUserSongs() => GetUserSongsWithInclude().ToList();

        public IEnumerable<DbUserSong> GetUserSongs(Func<DbUserSong, bool> predicate) => GetUserSongsWithInclude().Where(predicate);

        public IEnumerable<DbUserSong> GetUserSongs(string username) => GetUserSongs(e =>  e.User.Username == username);

        public void AddUserSong(DbUserSong song)
        {
            var isAlreadyExists = _dbContext.UserSongs.Where(e => e.DbSongId == song.DbSongId && e.UserId == song.UserId).Any();

            if (isAlreadyExists)
                return;

            _dbContext.UserSongs.Add(song);
        }

        public void DeleteUserSong(int userId, int dbSongId)
        {
            var songs = GetUserSongs(e => e.UserId == userId && e.DbSongId == dbSongId);

            foreach (var item in songs)
                _dbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}
