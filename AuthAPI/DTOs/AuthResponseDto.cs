using MongoDB.Bson.Serialization.IdGenerators;

namespace AuthAPI.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
