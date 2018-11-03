using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nip.Blog.Services.Posts.API.Data;
using Nip.Blog.Services.Posts.API.Exceptions;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly BlogPostContext _context;

        public BlogPostRepository(BlogPostContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<BlogPost> GetAllAsync()
        {
            return _context.BlogPosts.ToAsyncEnumerable();
        }

        public async Task<BlogPost> GetAsync(long id)
        {
            return await _context.BlogPosts.FindAsync(id);
        }

        public async Task AddAsync(BlogPost post)
        {
            var isTitleAlreadyExisting = await _context.BlogPosts
                .Where(x => x.Title.Equals(post.Title))
                .ToAsyncEnumerable().Any();

            // Note: below code exist solely to show how global exception handler works
            if (isTitleAlreadyExisting)
            {
                throw new BlogPostsDomainException($"Blog post with such title already exist: {post.Title}");
            }
            else
            {
                await _context.BlogPosts.AddAsync(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(BlogPost post)
        {
            var isSuchTitleAlreadyExisting = await _context.BlogPosts
                    .Where(x => x.Title.Equals(post.Title) && x.Id != post.Id)
                    .ToAsyncEnumerable().Any();

            if (isSuchTitleAlreadyExisting)
            {
                throw new BlogPostsDomainException($"Blog post with such title already exist: {post.Title}");
            }
            else
            {
                var existingPost = await _context.BlogPosts.FindAsync(post.Id);
                _context.Entry(existingPost).CurrentValues.SetValues(post);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post != null)
            {
                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
