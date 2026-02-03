using Common.Enums;

namespace Repository.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public List<Zimmer> Zimmers { get; set; } = new();

    }
}
