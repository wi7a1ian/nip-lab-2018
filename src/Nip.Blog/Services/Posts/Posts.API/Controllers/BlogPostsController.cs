using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nip.Blog.Services.Posts.API.Data;
using Nip.Blog.Services.Posts.API.Exceptions;
using Nip.Blog.Services.Posts.API.Models;
using Nip.Blog.Services.Posts.API.Repositories;

namespace Nip.Blog.Services.Posts.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly ILogger<BlogPostsController> _logger;
        private readonly IBlogPostRepository _postsRepo;

        public BlogPostsController(ILogger<BlogPostsController> logger, IBlogPostRepository repo)
        {
            _logger = logger;
            _postsRepo = repo;
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
            var posts = await _postsRepo.GetAllAsync().ToList();
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

            var item = await _postsRepo.GetAsync(id);
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

            await _postsRepo.AddAsync(post);

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

            var post = await _postsRepo.GetAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post {0} not found", id);
                return NotFound();
            }
            else
            {
                updatedPost.Id = id;
                await _postsRepo.UpdateAsync(updatedPost);

                _logger.LogInformation("Updating post {0} succeeded", post.Id);
                return NoContent(); 
            }
        }

        // DELETE api/blogposts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Removing post {id}", id);

            var post = await _postsRepo.GetAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post {id} not found", id);
                return NotFound();
            }
            else
            {
                await _postsRepo.DeleteAsync(id);

                _logger.LogInformation("Removing post {id} succeeded", id);
                return NoContent();
            }
        }
    }
}
