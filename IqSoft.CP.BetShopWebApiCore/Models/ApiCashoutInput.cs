public class ApiCashoutInput
{
	public string Token { get; set; }

	public long BetId { get; set; }

	public long BetDocumentId { get; set; }

	public bool IsFullAmount { get; set; }

	public decimal Amount { get; set; }
}