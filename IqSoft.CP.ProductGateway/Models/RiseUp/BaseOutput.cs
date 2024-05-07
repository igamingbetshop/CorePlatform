namespace IqSoft.CP.ProductGateway.Models.RiseUp
{
	public class BaseOutput
	{
		public bool status { get; set; } = false;
		public decimal balance { get; set; } = 0;
		public string error { get; set; } = "";
		public string referenceTID { get; set; } = "";
	}
}