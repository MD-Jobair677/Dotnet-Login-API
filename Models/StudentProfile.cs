namespace LoginSystem.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        public int StudentId { get; set; } // FK
        public string? ProfileImage{get;set;}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public  Student? Student { get; set; } // navigation

    }
}