using Microsoft.AspNetCore.Hosting;
using Nip.Blog.Services.Posts.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nip.Blog.Services.Posts.API.Data
{
    public class BlogPostContextInitializer
    {
        public static void Initialize(BlogPostContext context, IHostingEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                // Seed only in development environment
                return;
            }

            context.Database.EnsureCreated();

            if (context.BlogPosts.Any())
            {
                // DB has been seeded
                return;
            }

            var posts = new List<BlogPost>
            {
                new BlogPost{Title="Test 1", Description = "Descrition 1"},
                new BlogPost{Title="Test 2", Description = "Descrition 2"},
                new BlogPost{Title="Test 3", Description = "Descrition 3"},
                new BlogPost{Title="Test 4", Description = "Descrition 4"},
                new BlogPost{Title="Test 5", Description = "Descrition 5"},
            };

            context.BlogPosts.AddRange(posts);
            context.SaveChanges();
        }
    }
}
