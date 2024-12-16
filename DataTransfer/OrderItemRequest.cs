namespace api_details.DataTransfer
{
    public class OrderItemRequest
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
