namespace ECommerce.API.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
