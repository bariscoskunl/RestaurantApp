using Microsoft.Data.SqlClient.DataClassification;
using RestaurantApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Services.Interfaces
{
    public interface ITableRepository
    {
        Task<IEnumerable<Table>> GetAllTablesAsync();
        Task<Table?> GetTableByIdAsync(int id);
        Task AddTableAsync(Table table);
        Task UpdateTableStatusAsync(int tableId, RestaurantApp.Common.Enums.TableStatus newStatus);
    }
}
