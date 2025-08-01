using AuthAPI.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace AuthAPI.Services
{
    public interface IAuthService
    {
        
        Task<string> RegisterAsync(string username, string email, string password);
        Task<string> LoginAsync(string email, string password);
       
    }
}
