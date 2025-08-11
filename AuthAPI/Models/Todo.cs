namespace AuthAPI.Models
{
    public class Todo
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IsCompleted { get; set; } = string.Empty;
    }
}
