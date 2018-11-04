﻿using System;
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
    [ApiController]
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}/BlogPosts")]
    public class BlogPostsV2Controller : ControllerBase
    {
        private readonly ILogger<BlogPostsV2Controller> _logger;
        private readonly IBlogPostRepository _postsRepo;

        public BlogPostsV2Controller(ILogger<BlogPostsV2Controller> logger, IBlogPostRepository repo)
        {
            _logger = logger;
            _postsRepo = repo;
        }

        // GET api/v2/blogposts[?pageIndex=3&pageSize=10]
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BlogPost>))]
        [ProducesResponseType(200, Type = typeof(PaginatedItems<BlogPost>))]
        public async Task<IActionResult> Get([FromQuery]int pageIndex = -1, [FromQuery]int pageSize = 5)
        {
            if(pageIndex >= 0 && pageSize < 1)
            {
                _logger.LogWarning("Incorrect page size provided when retrieving paged blog posts");
                return BadRequest();
            }
            else if (pageIndex < 0)
            {
                _logger.LogInformation("Obtaining all the blog posts");
                var posts = await _postsRepo.GetAllAsync().ToList();

                _logger.LogDebug("Retrieved {0} posts total", posts.Count);
                return Ok(posts);
            }
            else
            {
                _logger.LogInformation("Obtaining blog posts: page = {0}, size = {1}", pageIndex, pageSize);

                var pagedPosts = await _postsRepo.GetAllPagedAsync(pageIndex, pageSize);
                var isLastPage = (pagedPosts.TotalItems <= pageIndex * pageSize + pagedPosts.PageSize);

                pagedPosts.NextPage = (!isLastPage ? Url.Link(null, new { pageIndex = pageIndex + 1, pageSize = pageSize }) : null);

                _logger.LogDebug("Retrieved {0} posts from {1} total", pagedPosts.PageSize, pagedPosts.TotalItems);

                return Ok(pagedPosts);
            }
        }

        // GET api/v2/blogposts/withtitle/sometitle[?pageIndex=3&pageSize=10]
        [HttpGet("withtitle/{title:minlength(1)}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(PaginatedItems<BlogPost>))]
        public async Task<IActionResult> Get(string title, [FromQuery]int pageIndex = 0, [FromQuery]int pageSize = 5)
        {
            if (pageIndex < 0 || pageSize < 1 || string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Incorrect parameters provided");
                return BadRequest();
            }
            else
            {
                _logger.LogInformation("Obtaining blog posts: page = {0}, size = {1}", pageIndex, pageSize);
                _logger.LogDebug("Title filter: '{0}'", title);

                var pagedPosts = await _postsRepo.GetAllPagedAsync(pageIndex, pageSize, x => x.Title.Contains(title));
                var isLastPage = (pagedPosts.TotalItems <= pageIndex * pageSize + pagedPosts.PageSize);

                pagedPosts.NextPage = (!isLastPage ? Url.Link(null, new { pageIndex = pageIndex + 1, pageSize = pageSize }) : null);

                _logger.LogDebug("Retrieved {0} posts from {1} total", pagedPosts.PageSize, pagedPosts.TotalItems);

                return Ok(pagedPosts);
            }
        }

        // GET api/v2/blogposts/5
        [HttpGet("{id}", Name = "GetBlogPostV2")]
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

        // POST api/v2/blogposts
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(BlogPost))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Post([FromBody] BlogPost post)
        {
            _logger.LogInformation("Adding new blog post");

            await _postsRepo.AddAsync(post);

            _logger.LogInformation("Post {0} has been added", post.Id);
            return CreatedAtRoute("GetBlogPostV2", new { id = post.Id }, post);
        }

        // PUT api/v2/blogposts/5
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

        // DELETE api/v2/blogposts/5
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
