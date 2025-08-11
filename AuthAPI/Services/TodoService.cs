using AuthAPI.Models;
using MongoDB.Driver;

namespace AuthAPI.Services
{
    public class TodoService
    {
        private readonly IMongoCollection<Todo> _todos;

        public TodoService(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase("AuthDb");

            _todos = database.GetCollection<Todo>("Todos");
        }

        public async Task<List<Todo>>GetTodosByUserIdAsync(string userId)
        {
            return await _todos.Find(t => t.UserId == userId).ToListAsync();
        }

        public async Task<Todo>CreateTodoAsync(Todo todo)
        {
            await _todos.InsertOneAsync(todo);
            return todo;
        }

        public async Task<bool>UpdateTodoAsync(string id, string userid, Todo updateTodo)
        {
            var result = await _todos.ReplaceOneAsync(t => t.Id == id && t.UserId == userid, updateTodo);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteTodoAsync(string id, string userId)
        {
            var result = await _todos.DeleteOneAsync(t => t.Id == id && t.UserId == userId);
            return result.DeletedCount > 0;
        }


    }
}
