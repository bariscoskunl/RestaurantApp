using RestaurantApp.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Common.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; } = null!;

        public string? WaiterId { get; set; }
        public ApplicationUser Waiter { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Preparing;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
