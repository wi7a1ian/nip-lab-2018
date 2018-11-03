using Nip.Blog.Services.Posts.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public interface IBlogPostRepository
    {
        Task<BlogPost> GetAsync(long id);
        IAsyncEnumerable<BlogPost> GetAllAsync();
        Task AddAsync(BlogPost post);
        Task UpdateAsync(BlogPost post);
        Task DeleteAsync(long id);
    }
}
