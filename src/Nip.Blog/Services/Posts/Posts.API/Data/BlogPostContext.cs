using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Data
{
    public class BlogPostContext : DbContext
    {
        public BlogPostContext(DbContextOptions<BlogPostContext> options)
            : base(options)
        {
            // nop
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
    }
}
