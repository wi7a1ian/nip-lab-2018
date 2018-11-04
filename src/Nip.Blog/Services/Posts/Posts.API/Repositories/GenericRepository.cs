using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : Entity, new()
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IAsyncEnumerable<T> GetAllAsync()
        {
            return _dbSet.ToAsyncEnumerable();
        }

        public virtual async Task<T> GetAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T item)
        {
            await _dbSet.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T item)
        {
            var existingItem = await _dbSet.FindAsync(item.Id);
            if (existingItem != null)
            {
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task DeleteAsync(long id)
        {
            var item = await _dbSet.FindAsync(id);
            if (item != null)
            {
                _dbSet.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
