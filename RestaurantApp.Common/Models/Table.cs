using RestaurantApp.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Common.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string TableNumber { get; set; }= string.Empty;
        public TableStatus Status { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
