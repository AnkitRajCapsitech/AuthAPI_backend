using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserContext _context;
        private readonly IConfiguration _config;

         public AuthService(UserContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> RegisterAsync(string username, string email, string password)
        {
            var existingUser = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (existingUser != null)
                return "User already exists";

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword
            };

            await _context.Users.InsertOneAsync(user);
            return "User registered successfully";
        }
       

        public async Task<string> LoginAsync(string email, string password)
        {

            var user = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return "Invalid credentials";

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                
                Expires = DateTime.UtcNow.AddHours(1),
               
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> ForgetPasswordAsync(string email)
        {
            var user = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return "User not found";

            // Generate token
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            var expiry = DateTime.UtcNow.AddHours(1);

            var update = Builders<User>.Update
                .Set(u => u.ResetToken, token)
                .Set(u => u.ResetTokenExpiry, expiry);

            await _context.Users.UpdateOneAsync(u => u.Id == user.Id, update);

            // In real app: send token via email
            return $"Reset token (for testing): {token}";
        }

        public async Task<string> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.Find(u =>
                u.ResetToken == token &&
                u.ResetTokenExpiry > DateTime.UtcNow
            ).FirstOrDefaultAsync();

            if (user == null)
                return "Invalid or expired token";

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, hashedPassword)
                .Unset(u => u.ResetToken)
                .Unset(u => u.ResetTokenExpiry);

            await _context.Users.UpdateOneAsync(u => u.Id == user.Id, update);

            return "Password reset successfully";
        }

    }
}
