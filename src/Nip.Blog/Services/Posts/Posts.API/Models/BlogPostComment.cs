using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nip.Blog.Services.Posts.API.Models
{
    public class BlogPostComment : Entity
    {
        public string Author { get; set; }
        public string Content { get; set; }
    }
}
