namespace RestaurantApp.Web.DTOs
{
    public class PlaceOrderDto
    {
        public int TableId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
