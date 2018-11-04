using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nip.Blog.Services.Posts.API.Models
{
    public class PaginatedItems<T> where T: Entity 
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public long TotalItems { get; set; }

        public IEnumerable<T> Items { get; set; }

        public string NextPage { get; set; }
    }
}
