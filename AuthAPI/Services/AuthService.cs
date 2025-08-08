using AuthAPI.DTOs;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthAPI.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthService(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase("AuthDb");
            _users = database.GetCollection<User>("Users");
           
            _jwtKey = configuration["Jwt:Key"];
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];
        }

         public async Task<ApiResponse<object>>RegisterAsync(RegisterDto dto)
        {
            var response = new ApiResponse<object>();
            try
            {
                var existingUser = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
               
                if(existingUser!= null)
                {
                    response.Status = false;
                    response.Message = "User already exists";
                    return response;  
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    HashedPassword = hashedPassword
                };

                await _users.InsertOneAsync(user);
                response.Status = true;
                response.Message = "User registered successfully";
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Message = "Error:" + ex.Message;
            }

            return response;
        }

        public async Task<ApiResponse<string>>LoginAsync(LoginDto dto)
        {
            var response = new ApiResponse<string>();

            try
            {
                var user = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
                if(user==null|| string.IsNullOrEmpty(user.HashedPassword)|| !BCrypt.Net.BCrypt.Verify(dto.Password,user.HashedPassword))
                {
                    response.Status = false;
                    response.Message = "Invalid email or password";
                    return response;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.Username)

                    }),

                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _jwtIssuer,
                    Audience = _jwtAudience,

                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                        )

                   
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);

                response.Status = true;
                response.Message = "Login successful";
                response.Result = jwtToken;


            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Message = "Error:" + ex.Message;
                response.Result = null;
            }

            return response;
        }
    }

   
}
