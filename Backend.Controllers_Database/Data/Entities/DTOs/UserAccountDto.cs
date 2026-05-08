namespace WiseBet.backend.DTOs
{
    public class UserAccountDto : IDto
    {
        public Guid ID { get; set; }
        public string Username { get; set; }
        public int Saldo { get; set; }
    }
}
