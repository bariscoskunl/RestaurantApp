using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Common.Enums
{
    public enum TableStatus
    {
        Empty = 0, // Boş
        Occupied = 1, // Dolu
        WaitingForBill = 2, // Hesap Bekliyor
        WaiterRequested = 3, // Garson Çağrıldı
    }
}
