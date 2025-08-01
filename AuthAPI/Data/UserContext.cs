using AuthAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AuthAPI.Data
{
    public class UserContext
    {
        private readonly IMongoDatabase _database;

        public UserContext(IOptions<MongoDbSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.DatabaseName);
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("Users");
    }
}
