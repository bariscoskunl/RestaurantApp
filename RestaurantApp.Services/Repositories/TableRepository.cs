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
           return await _context.Tables
               .Include(t => t.Orders.Where(o => o.Status != RestaurantApp.Common.Enums.OrderStatus.Completed && o.Status != RestaurantApp.Common.Enums.OrderStatus.Cancelled))
                   .ThenInclude(o => o.OrderItems)
                       .ThenInclude(oi => oi.Product)
               .ToListAsync();
        }
        public async Task<IEnumerable<Table>> GetEmptyTableAsync()
        {
            return await _context.Tables
                .Where(t => t.Status == TableStatus.Empty)
                .ToListAsync();
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
        public async Task DeleteTableAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table != null)
            {
                _context.Tables.Remove(table);
                await _context.SaveChangesAsync();
            }
        }       
    }
}
