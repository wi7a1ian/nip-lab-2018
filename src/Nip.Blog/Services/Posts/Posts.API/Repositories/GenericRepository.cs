using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public virtual async Task<PaginatedItems<T>> GetAllPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> filter = null)
        {
            if(pageIndex < 0)
            {
                throw new ArgumentException("Cannot be negative", nameof(pageIndex));
            }

            if (pageSize < 0)
            {
                throw new ArgumentException("Cannot be negative", nameof(pageSize));
            }

            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalItems = await query.CountAsync();

            var posts = await query.OrderByDescending(c => c.Id).Skip(pageIndex * pageSize).Take(pageSize)
                .ToListAsync();

            var actPageSize = Math.Min(pageSize, totalItems - pageIndex * pageSize);

            var pagedPosts = new PaginatedItems<T>
            {
                PageIndex = pageIndex,
                PageSize = ((actPageSize < 0) ? 0 : actPageSize),
                TotalItems = totalItems,
                Items = posts.AsEnumerable()
            };

            return pagedPosts;
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
