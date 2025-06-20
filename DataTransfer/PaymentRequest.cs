namespace api_details.DataTransfer
{
    public class PaymentRequest
    {
        public string DeliveryAddress { get; set; }
        public List<OrderItemRequest> OrderItems { get; set; }
    }
}
