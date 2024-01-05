namespace IqSoft.CP.Integration.Products.Models.GlobalSlots
{
    public class TransferInput
    {
        public string key { get; set; }

        public string @event { get; set; }

        public int ghouse_id { get; set; }

        public decimal credit { get; set; }

        public string termid { get; set; }
    }
}
