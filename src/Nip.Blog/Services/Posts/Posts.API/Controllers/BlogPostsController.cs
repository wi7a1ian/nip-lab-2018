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
        // GET api/blogposts/all
        // GET api/blogposts/getall
        [HttpGet]
        [HttpGet("all")] // Note: multiple routing
        [HttpGet("GetAll")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BlogPost>))]
        public async Task<ActionResult<IEnumerable<BlogPost>>> Get()
        {
            return Ok(await _postsDbContext.BlogPosts.ToAsyncEnumerable().ToList());
        }

        // GET api/blogposts/5
        [HttpGet("{id}", Name = "GetBlogPost")]
        [ProducesResponseType(200, Type = typeof(BlogPost))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BlogPost>> Get(long id)
        {
            var item = await _postsDbContext.BlogPosts.FindAsync(id);
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
        [ProducesResponseType(201, Type = typeof(BlogPost))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Post([FromBody] BlogPost post)
        {
            await _postsDbContext.BlogPosts.AddAsync(post);
            await _postsDbContext.SaveChangesAsync();

            return CreatedAtRoute("GetBlogPost", new { id = post.Id }, post);
        }

        // PUT api/blogposts/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(long id, [FromBody] BlogPost updatedPost)
        {
            var post = await _postsDbContext.BlogPosts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            else
            {
                post.Title = updatedPost.Title;
                post.Description = updatedPost.Description;

                _postsDbContext.BlogPosts.Update(post);
                await _postsDbContext.SaveChangesAsync();

                return NoContent();
            }
        }

        // DELETE api/blogposts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            var post = await _postsDbContext.BlogPosts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            else
            {
                _postsDbContext.BlogPosts.Remove(post);
                await _postsDbContext.SaveChangesAsync();

                return NoContent();
            }
        }
    }
}
