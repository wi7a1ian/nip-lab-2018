using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public interface IRepository<T> where T : class, new()
    {
        Task<T> GetAsync(long id);
        IAsyncEnumerable<T> GetAllAsync();
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(long id);
    }
}
