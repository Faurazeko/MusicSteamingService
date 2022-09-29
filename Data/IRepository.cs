using MusicStreamingService.Models;

namespace MusicStreamingService.Data
{
    public interface IRepository
    {
        public bool SaveChanges();

        public User GetUser(string username);
        public List<User> GetUsers();
        public bool UserExists(string username);
        public void CreateUser(string username, string password);
        public void DeleteUser(string username);
        public void ChangeUserPassword(string username, string newPassword);

        public List<DbSong> GetSongs();
        public IEnumerable<DbSong> GetSongs(Func<DbSong, bool> predicate);
        public void AddSong(DbSong song);
        public void DeleteSong(string FileName);

        public List<DbUserSong> GetUserSongs();
        public IEnumerable<DbUserSong> GetUserSongs(Func<DbUserSong, bool> predicate);
        public IEnumerable<DbUserSong> GetUserSongs(string username);
        public void AddUserSong(DbUserSong song);
        public void DeleteUserSong(int userId, int dbSongId);
    }
}
