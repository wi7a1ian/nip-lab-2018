using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nip.Blog.Services.Posts.API.Data;
using Nip.Blog.Services.Posts.API.Exceptions;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Repositories
{
    public class BlogPostRepository : GenericRepository<BlogPost>, IBlogPostRepository
    {
        private readonly BlogPostContext _bpContext;

        public BlogPostRepository(BlogPostContext context) : base(context)
        {
            _bpContext = context;
        }

        public override async Task AddAsync(BlogPost post)
        {
            var isTitleAlreadyExisting = await _bpContext.BlogPosts
                .Where(x => x.Title.Equals(post.Title))
                .ToAsyncEnumerable().Any();

            // Note: below code exist solely to show how global exception handler works
            if (isTitleAlreadyExisting)
            {
                throw new BlogPostsDomainException($"Blog post with such title already exist: {post.Title}");
            }
            else
            {
                await base.AddAsync(post);
            }
        }

        public override async Task UpdateAsync(BlogPost post)
        {
            var isSuchTitleAlreadyExisting = await _bpContext.BlogPosts
                    .Where(x => x.Title.Equals(post.Title) && x.Id != post.Id)
                    .ToAsyncEnumerable().Any();

            if (isSuchTitleAlreadyExisting)
            {
                throw new BlogPostsDomainException($"Blog post with such title already exist: {post.Title}");
            }
            else
            {
                await base.UpdateAsync(post);
            }
        }
    }
}
