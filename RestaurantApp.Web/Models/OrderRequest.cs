using RestaurantApp.Common.Models;
using System.Collections.Generic;

namespace RestaurantApp.Web.Models
{
    public class OrderRequest
    {
        public List<SaleProducts> Cart { get; set; }
        public int? TableNumber { get; set; }
    }
}
