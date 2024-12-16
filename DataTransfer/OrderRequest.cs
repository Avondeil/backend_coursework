namespace api_details.DataTransfer
{
    // DTO для заказа
    public class OrderRequest
    {
        public string DeliveryAddress { get; set; }
        public List<OrderItemRequest> OrderItems { get; set; }
    }
}
