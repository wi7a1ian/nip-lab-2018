using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nip.Blog.Services.Posts.API.Models
{
    public class BlogPost : Entity
    {
        [Required]
        [StringLength(32, MinimumLength = 3)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$", ErrorMessage = "Should start from capital letter and consist only of basic characters.")]
        public string Title { get; set; }

        [StringLength(4096)]
        public string Description { get; set; }
    }
}
