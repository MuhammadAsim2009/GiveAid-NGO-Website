namespace Give_AID.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // stores the hash, not plain text
        public string Role { get; set; } = "User";           // Admin or User
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? Organization { get; set; }
    }
}