using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nip.Blog.Services.Posts.API.Data;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly BlogPostContext _postsDbContext;

        public BlogPostsController(BlogPostContext postsDbContext)
        {
            _postsDbContext = postsDbContext;
        }

        // GET api/blogposts
        [HttpGet]
        public ActionResult<IEnumerable<BlogPost>> Get()
        {
            return Ok(_postsDbContext.BlogPosts.ToList());
        }

        // GET api/blogposts/5
        [HttpGet("{id}", Name = "GetBlogPost")]
        public ActionResult<BlogPost> Get(long id)
        {
            var item = _postsDbContext.BlogPosts.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(item);
            }
        }

        // POST api/blogposts
        [HttpPost]
        public IActionResult Post([FromBody] BlogPost post)
        {
            _postsDbContext.BlogPosts.Add(post);
            _postsDbContext.SaveChanges();

            return CreatedAtRoute("GetBlogPost", new { id = post.Id }, post);
        }

        // PUT api/blogposts/5
        [HttpPut("{id}")]
        public IActionResult Put(long id, [FromBody] BlogPost updatedPost)
        {
            var post = _postsDbContext.BlogPosts.Find(id);
            if (post == null)
            {
                return NotFound();
            }
            else
            {
                post.Title = updatedPost.Title;
                post.Description = updatedPost.Description;

                _postsDbContext.BlogPosts.Update(post);
                _postsDbContext.SaveChanges();

                return NoContent();
            }
        }

        // DELETE api/blogposts/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var post = _postsDbContext.BlogPosts.Find(id);
            if (post == null)
            {
                return NotFound();
            }
            else
            {
                _postsDbContext.BlogPosts.Remove(post);
                _postsDbContext.SaveChanges();

                return NoContent();
            }
        }
    }
}
