﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nip.Blog.Services.Posts.API.Models
{
    public class BlogPost
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
