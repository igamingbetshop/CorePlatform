namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
    public class TransferInput
    {
        /// <summary>
        /// The required action. In the second step,  this is 'transfer'
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// Session identifier returned in  response to the prepare request.
        /// </summary>
        public string sid { get; set; }
    }
}