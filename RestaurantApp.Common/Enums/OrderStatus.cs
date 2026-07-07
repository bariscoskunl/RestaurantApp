using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Common.Enums
{
    public enum OrderStatus
    {
        Preparing = 0,       // Müşteri ödediğinde veya Waiter girdiğinde doğrudan Mutfakta
        Served = 1,          // Masaya servis edildi
        Completed = 2,       // Hesap ödendi/Kapandı
        Cancelled = 3        // İptal edildi
    }
}
