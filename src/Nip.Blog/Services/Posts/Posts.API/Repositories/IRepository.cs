using Nip.Blog.Services.Posts.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public interface IRepository<T> where T : Entity, new()
    {
        Task<T> GetAsync(long id);
        IAsyncEnumerable<T> GetAllAsync();
        Task<PaginatedItems<T>> GetAllPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> filter = null);
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(long id);
    }
}
