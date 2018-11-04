using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public interface IBlogPostRepository : IRepository<BlogPost>
    {
        Task<IAsyncEnumerable<BlogPostComment>> GetCommentsAsync(long blogPostId);
        Task AddCommentAsync(long blogPostId, BlogPostComment comment);
    }
}
