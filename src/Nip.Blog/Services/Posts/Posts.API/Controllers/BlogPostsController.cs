using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nip.Blog.Services.Posts.API.Data;
using Nip.Blog.Services.Posts.API.Exceptions;
using Nip.Blog.Services.Posts.API.Models;

namespace Nip.Blog.Services.Posts.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly ILogger<BlogPostsController> _logger;
        private readonly BlogPostContext _postsDbContext;

        public BlogPostsController(ILogger<BlogPostsController> logger, BlogPostContext postsDbContext)
        {
            _logger = logger;
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
            _logger.LogInformation("Obtaining all the blog posts");
            var posts = await _postsDbContext.BlogPosts.ToAsyncEnumerable().ToList();
            _logger.LogDebug("Retrieved {0} posts total", posts.Count());

            return Ok(posts);
        }

        // GET api/blogposts/5
        [HttpGet("{id}", Name = "GetBlogPost")]
        [ProducesResponseType(200, Type = typeof(BlogPost))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BlogPost>> Get(long id)
        {
            _logger.LogInformation("Obtaining post {Id}", id);

            var item = await _postsDbContext.BlogPosts.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Post {Id} not found", id);
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
            _logger.LogInformation("Adding new blog post");

            var isTitleAlreadyExisting = await _postsDbContext.BlogPosts
                .Where(x => x.Title.Equals(post.Title, StringComparison.InvariantCultureIgnoreCase))
                .ToAsyncEnumerable().Any();

            // Note: below code exist solely to show how global exception handler works
            if (isTitleAlreadyExisting)
            {
                throw new BlogPostsDomainException($"Blog post with such title already exist: {post.Title}");
            }

            await _postsDbContext.BlogPosts.AddAsync(post);
            await _postsDbContext.SaveChangesAsync();

            _logger.LogInformation("Post {0} has been added", post.Id);
            return CreatedAtRoute("GetBlogPost", new { id = post.Id }, post);
        }

        // PUT api/blogposts/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(long id, [FromBody] BlogPost updatedPost)
        {
            _logger.LogInformation("Updating post {0}", id);
            _logger.LogDebug("Received post id {0} with new title: {1}'", id, updatedPost.Title);

            var post = await _postsDbContext.BlogPosts.FindAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post {0} not found", id);
                return NotFound();
            }
            else
            {
                var isSuchTitleAlreadyExisting = await _postsDbContext.BlogPosts
                    .Where(x => x.Title.Equals(updatedPost.Title, StringComparison.InvariantCultureIgnoreCase) && x.Id != id)
                    .ToAsyncEnumerable().Any();

                if (isSuchTitleAlreadyExisting)
                {
                    _logger.LogWarning("Post {0} was not updated, blog post with same title already exist", id);
                    return BadRequest(new { Title = new string[] { "Blog post with same title already exist" } });
                }
                else
                {
                    post.Title = updatedPost.Title;
                    post.Description = updatedPost.Description;

                    _postsDbContext.BlogPosts.Update(post);
                    await _postsDbContext.SaveChangesAsync();

                    _logger.LogInformation("Updating post {0} succeeded", post.Id);

                    return NoContent();
                }
            }
        }

        // DELETE api/blogposts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Removing post {id}", id);
            var post = await _postsDbContext.BlogPosts.FindAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post {id} not found", id);
                return NotFound();
            }
            else
            {
                _postsDbContext.BlogPosts.Remove(post);
                await _postsDbContext.SaveChangesAsync();

                _logger.LogInformation("Removing post {id} succeeded", id);

                return NoContent();
            }
        }
    }
}
