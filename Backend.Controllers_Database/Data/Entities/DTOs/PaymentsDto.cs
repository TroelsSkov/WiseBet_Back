namespace WiseBet.backend.DTOs;

public class PaymentDto : IDto
{
    public Guid ID { get; set; }
    public required Guid UserID { get; set; }
    public DateTime TimeOfPayment { get; set; }
    public int PaymentAmount { get; set; }
    public int PrePaymentBalance { get; set; }
}

