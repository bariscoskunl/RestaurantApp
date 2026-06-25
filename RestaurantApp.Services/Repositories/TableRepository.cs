using Microsoft.EntityFrameworkCore;
using RestaurantApp.Common.Enums;
using RestaurantApp.Common.Models;
using RestaurantApp.Data;
using RestaurantApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Services.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly ApplicationDbContext _context;

        public TableRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Table>> GetAllTablesAsync()
        {
           return await _context.Tables.ToListAsync();
        }
        public async Task<Table?> GetTableByIdAsync(int id)
        {
            return await _context.Tables.FindAsync(id);
        }
        public async Task AddTableAsync(Table table)
        {
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateTableStatusAsync(int tableId, TableStatus newStatus)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table != null)
            {
                table.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }
    }
}
