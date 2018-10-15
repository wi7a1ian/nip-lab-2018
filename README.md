# nip-lab-2018

# Prerequisites
- [Visual Studio 2017 version 15.7](https://visualstudio.microsoft.com/downloads/) or later with:
  - ASP.NET and web development
  - .NET Core cross-platform development
    - .NET Core 2.1 SDK or later
- [Postman](https://www.getpostman.com/)

# API Requirements (RESTful)
- API:

| API | Description | Request body | Response body | HTTP status code |
|-|:-|:-:|:-:|:-:|
| GET /api/v1/blogposts | Get all blog posts | - | \{ array of posts \} | 200 OK |
| GET /api/v1/blogposts/\{id\} | Get a post by id | - | \{ post \} | 200 OK |
| POST /api/v1/blogposts | Add a new blog post | { body } | \{ post \} | 201 Created |
| PUT /api/v1/blogposts/\{id\} | Update an existing blog post  | \{ post \} | - | 200 OK |
| DELETE /api/v1/blogposts/\{id\} | Delete a blog post | - | - | 204 No Content |

- Anonymous, but secured access ( use SSL for any publicly exposed API )
- Document your API
- Version API via the URL
- Default to JSON for both request and response 
- API should always return sensible HTTP status codes. API errors typically break down into 2 types: 400 series status codes for client issues & 500 series status codes for server issues. At a minimum, the API should standardize that all 400 series errors come with consumable JSON error representation

# Laboratories
### Setup steps
- Using Visual Studio
	1. From the File menu, select New > Project > Other Project Types > Blank Solution.
	1. Name it Nip.Blog, this follows the common patter `<company>.<product>.*`
	1. Right click on solution name and add folder path `Services/Posts` 
	1. Right click on `Services/Posts`, select Add > New project...
	1. Select the ASP.NET Core Web Application template. Name the project Posts.Api and click OK.
	1. In the New ASP.NET Core Web Application - NIP.Backend.BlogApi dialog, choose the ASP.NET Core version. Select the API template and click OK. Do not select Enable Docker Support.
	1. *(optinal) Rename the default namespace (everywhere) from `Posts.API` to `Nip.Blog.Services.Posts.API` for consistency (`<company>.<product>.<>`)*
	1. Right click the `ValuesController.cs` and rename it to `BlogPostsController.cs`
	1. At the toolbar switch the debug target from `IIS Express` to `Nip.Blog.Services.Posts.API` and run it. *( the reason behind this is that we want to use .net core runtime to run the server for us and not the IIS )*
	1. Before the debug starts VS should ask you for permission to add "fake" SSL certificate to the cert store.
	1. *(optional) stop the debugging, you can also start the server by either running `dotnet run` when in project folder, or `dotnet <name>.dll` when in /bin/Debug folder*
	1. Two app URLs should be available at this point: secure `https://localhost:5001` if SSL is enabled; unsecure `http://localhost:5000` that redirects to secured URL if SSL is enabled, otherwise just works fine. Note: If someone did choose to use IIS instead, the ports should be different but the app should behave the same.
	1. Open web browser and navigate to `https://localhost:5001/api/blogposts`
- Using Postman
	1. Navigate to Settings and turn off "SSL certificate verification"
	1. Create new collection
	1. Add two new requests and validate they work (Send & Save): `GET http://localhost:5000/api/blogposts` and `GET https://localhost:5001/api/blogposts`

### Exercise set #1 - in-memory store & basic CRUD
- API versioning is important for consumers, thus change routing for our endpoint from `/api/blogposts` to `/api/v1/blogposts`. Validate using Postman that endpoints still work.
- Using Visual Studio
	1. Add new model BlogPost under Models folder (`./Models/BlogPost.cs`): right-click the project, select Add > New Folder and name it Models, then right-click the Models folder and select Add > Class. Name the class BlogPost and click Add. Update the model with the following properties/fields: 
		```csharp
		namespace Nip.Blog.Services.Posts.API.Models
		{
			public class BlogPost
			{
				public long Id { get; set; }
				public string Title { get; set; }
				public string Description { get; set; }
			}
		}
		```
	1. The easiest way to create an in-memory store is to create EntityFramework database context for BlogPost data model. Create `Data/BlogPostContext.cs` class that derive from `Microsoft.EntityFrameworkCore.DbContext`. 
		```csharp
		using Microsoft.EntityFrameworkCore;
		using Nip.Blog.Services.Posts.API.Models;

		namespace Nip.Blog.Services.Posts.API.Data
		{
			public class BlogPostContext : DbContext
			{
				public BlogPostContext(DbContextOptions<BlogPostContext> options)
					: base(options)
				{
					// nop
				}

				public DbSet<BlogPost> BlogPosts { get; set; }
			}
		}
		```
	1. In ASP.NET Core dependencies are driven via built-in Dependency Injection (DI) Container. We have to register our DB context in the `IServiceCollection` service container. Update `Startup.cs`:
		```csharp
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<BlogPostContext>(opt => opt.UseInMemoryDatabase("BlogPosts"));
			...
		}
		```
	1. Then we can use it in our BlogPostsController now:
		```csharp
		public class BlogPostsController : ControllerBase
		{
			private readonly BlogPostContext _postsDbContext;

			public BlogPostsController(BlogPostContext postsDbContext)
			{
				_postsDbContext = postsDbContext;
			}
			...
		```
		*Note: The controller's constructor uses Dependency Injection to inject the database context (BlogPostsContext) into the controller.*
	1. Allow retrieval of all the blog posts. Update `BlogPostsController.cs`
		```csharp
		// GET api/blogposts
		[HttpGet]
		public ActionResult<IEnumerable<BlogPost>> Get()
		{
			return Ok(_postsDbContext.BlogPosts.ToList());
		}
		```
	1. Allow rerieval of specific blog post.
		```csharp
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
		```
	1. Allow creation of a new blog post.
		```csharp
		// POST api/blogposts
		[HttpPost]
		public IActionResult Post([FromBody] BlogPost post)
		{
			_postsDbContext.BlogPosts.Add(post);
			_postsDbContext.SaveChanges();

			return CreatedAtRoute("GetBlogPost", new { id = post.Id }, post);
		}
		```
	1. Allow modification of an existing blog post.
		```csharp
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
		```
	1. Allow removal of a blog post
		```csharp
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
		```
- Using Postman:
	1. Send and notice the 404 HTTP status code:\
	`GET http://localhost:5000/api/blogposts/923829`
	1. Send and notice the 400 HTTP status code:\
	`GET http://localhost:5000/api/blogposts/' or 1=1`
	1. Add "POST" request and validate it work (don't forget to hit Save!):\
	`POST http://localhost:5000/api/blogposts { "title": "New Post", "description": "Lorem ipsum..." }`
	1. Notice the 201 status code and generated route for the new blog post (Headers -> Location). Validate the URI is accessible.
	1. Add "PUT" request and validate it work:\
	`PUT http://localhost:5000/api/blogposts/1 { "title": "New Post With Fixed Title", "description": "Lorem ipsum..." }`
	1. Add "PUT" request and validate it work:\
	`DELETE http://localhost:5000/api/blogposts/1`
	1. Try sending invalid models, like: empty one; with changed field names; with changed field value types, i.e: array [] or object {}.

### Exercise set #2 - asynchronous IO access & asynchronous API
- *Forget for a moment that we currently have in-memory database...*
- Database context can be accessed asynchronously ( `async`/`await` ) since most of its operations are waiting for commends being executed on an actual database. They are usually not CPU intensitive, thus asynchronous operations allow the caller to do whatever he wants while waiting for the database query result.
- Usually there is a limited number of threads servicing requests. The benefit of using async over sync is that instead of blocking the thread while it is waiting for the database call to complete in sync implementation, the async will free the thread to handle more requests or assign it what ever process needs a thread. Once IO (database) call completes, another thread will take it from there and continue with the implementation. Async will also make your api run faster if your IO operations take longer to complete.
- ***One of prominent best practices in async programming is Async all the way i.e. you shouldn’t mix synchronous and asynchronous code without carefully considering the consequences.***
- Using Visual Studio:
	1. Change all the calls to database context (`_postsDbContext`) to async representatives, for example:
		```csharp
		public async Task<ActionResult<IEnumerable<BlogPost>>> Get()
		{
			return Ok(await _postsDbContext.BlogPosts.ToAsyncEnumerable().ToList());
		}
		
		public async Task<IActionResult> Post([FromBody] BlogPost post)
		{
			await _postsDbContext.BlogPosts.AddAsync(post);
			await _postsDbContext.SaveChangesAsync();
			return CreatedAtRoute("GetBlogPost", new { id = post.Id }, post);
		}
		```
	1. All `GET/POST/PUT/DELETE` action methods should be `async`.
- Using Postman:
	1. Confirm nothing is broken by rerunning previous `POST/GET/PUT/DELETE` (preferably in that order) requests.
	
### Exercise set #3 - document API
- One of the most widely used API specifications is OpenAPI. Swagger UI is an open source project to visually render documentation for an API defined with the OpenAPI (Swagger) Specification.
- The usual operation flow for usig Swagger looks like this: 
	1. We register the Swagger generator which scans our APIs (controllers) and defines 1 or more Swagger documents.
	1. Special middleware does serve generated Swagger as a JSON endpoint.
	1. Special middleware does serve swagger-ui (HTML, JS, CSS, etc.) specifying the Swagger JSON endpoint.
- Using Visual Studio:
	1. Install `Swashbuckle.AspNetCore` nuget package by right-click on the project in Solution Explorer > Manage NuGet Packages
	1. *(optional) you can avhieve the same via View > Other Windows > Package Manager Console and typing in `Install-Package Swashbuckle.AspNetCore`*
	1. Register Swagger generator in the services collection (`Startup.cs` > `ConfigureServices()`)
		```csharp
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new Info { Title = "Blog Posts API", Version = "v1" });
		});
		```
	1. Enable the middleware for serving the generated JSON document and the Swagger UI (`Startup.cs` > `Configure()`)
		```csharp
		app.UseSwagger();
		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog Posts API v1");
		});
		```
	1. Run the server.
- Using browser:
	1. Navigate to API documentation in JSON format\
	`https://localhost:5001/swagger/v1/swagger.json`
	1. Navigate to Swagger UI documentation\
	`https://localhost:5001/swagger/`
	1. Since the latter one is interacive, you can send the same GET/POST/PUT/DELETE requests.
- Using Visual Studio:
	1. Extend SwaggerDoc(...) with more info like description, contact & license.
	1. Add additional data annotations on the BlogPost model (`Models/BlogpPost.cs`) to inform about the constraints. Be careful because this does influence build-in model validation logic.
		```csharp
		[Required]
		[StringLength(32, MinimumLength = 3)]
		[RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$", ErrorMessage = "Should start from capital letter and consist only of basic characters.")]
		public string Title { get; set; }

		[StringLength(4096)]
		public string Description { get; set; }
		```
- Using browser:
	1. Confirm the documentation for the BlogPost model was updated *(bottom of the page)*
- One of the API requirements is to provide valid HTTP response codes and document them. We should at lest describe response types for each action.
- Use the HTTP Status Codes
	- **200 OK** - Response to a successful GET, PUT, PATCH or DELETE. Can also be used for a POST that doesn't result in a creation.
	- **201 Created** - Response to a POST that results in a creation. Should be combined with a Location header pointing to the location of the new resource.
	- **204 No Content** - Response to a successful request that won't be returning a body (like a DELETE request).
	- **400 Bad Request** - The request is malformed, such as if the body does not parse.
	- **401 Unauthorized** - When no or invalid authentication details are provided. Also useful to trigger an auth popup if the API is used from a browser.
	- **403 Forbidden** - When authentication succeeded but authenticated user doesn't have access to the resource.
	- **404 Not Found** - When a non-existent resource is requested.
- Using Visual Studio:
	1. Decorate/annotate each action method with appropriate `[ProducesResponseType]` attribute. Example:
		```csharp
		[HttpPost]
		[ProducesResponseType(201, Type = typeof(BlogPost))]
		[ProducesResponseType(400)]
		public async Task<IActionResult> Post([FromBody] BlogPost post)
		{ ...
		```
- Using browser:
	1. Open Swagger UI and check if HTTP response codes are now listed.

# TODO
- Part 1
	- Logging
	- Custom response/error page
- Part 2
	- Model validation using FluentValidation
	- Automapper (DTO and Entity be different, i.e: ModificationTime, ModificationAuthor?)
	- EF Core
		- Code first
		- Migrations
	- Advanced
		- filtering / sorting / paging
		- authentication and authorization using Identity (separate web API) either Access tokens (JWT) or reference tokens
		- separate helper web API with its own NoSQL database for audits
- [HttpGet("products")] [HttpGet("{id}/comments")] [HttpGet("posts/{id}")] 
- default HTTP 400 response is disabled if:
	```
	services.Configure<ApiBehaviorOptions>(options =>
	{
		options.SuppressModelStateInvalidFilter = true;
	});
	```
- PaginatedItemsModel<TEntity>
- IEnumerable<TEntity> vs IQueryable<TEntity>
- IExceptionFilter
- Cors
- Test using older VS 2017 ( < v15.3)
- Try the same using VS Code
