# nip-lab-2018

# Prerequisites
- Either one of those:
  - [Visual Studio 2017 version 15.7](https://visualstudio.microsoft.com/downloads/) or later with:
    - ASP.NET and web development
    - .NET Core cross-platform development
  - [Visual Studio Code](https://code.visualstudio.com/)
    - Plugin: C# for Visual Studio Code
    - IF ERRORS then use C# for Visual Studio Code [version 1.15.2](https://github.com/OmniSharp/omnisharp-vscode/releases/download/v1.15.2/csharp-1.15.2.vsix) since there is a bug when older version of VS is installed alongside. 
      1. Download that vsix file, go to VS Code > Extensions > ... > Install from VSIX...
      2. VS Code > Extensions > ... > Disable Auto Updating Extensions
    - [.NET Core 2.1 SDK or later](https://www.microsoft.com/net/download/all)
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
	1. Select the *ASP.NET Core Web Application* template. Name the project Posts.Api and click OK.
	1. In the *New ASP.NET Core Web Application* dialog, choose 2.1 or later as the ASP.NET Core version. Select the API template and click OK. Do not select Enable Docker Support.
	1. *(optinal) Rename the default namespace (everywhere) from `Posts.API` to `Nip.Blog.Services.Posts.API` for consistency (`<company>.<product>.<>`)*
	1. Right click the `ValuesController.cs` and rename it to `BlogPostsController.cs`
	1. At the toolbar switch the debug target from `IIS Express` to `Nip.Blog.Services.Posts.API` and run it. *( the reason behind this is that we want to use .net core runtime to run the server for us and not the IIS )*
	1. Before the debug starts VS should ask you for permission to add "fake" SSL certificate to the cert store.
- *(optional)* Using Visual Studio Code
	1. Create `Services/Posts` subfolder, navigate there and run commands:
		```
		dotnet new webapi -o Posts.API
		dotnet dev-certs https --trust
		code Posts.API
		```
	1. Accept any popups that appear (like `Restore`)
	1. Right click the `ValuesController.cs` and rename it to `BlogPostsController.cs`
	1. Start the server by either 
		- Running the Debug (F5)
		- Running `dotnet run` from VS Code Terminal
		- Running `dotnet run` when in project folder
		- Running `dotnet Posts.API.dll` when in /bin/Debug folder
- Two app URLs should be available at this point: secure `https://localhost:5001` if SSL is enabled; unsecure `http://localhost:5000` that redirects to secured URL if SSL is enabled, otherwise just works fine. *Note: If someone did choose to use IIS instead, the ports should be different but the app should behave the same.*
	1. Open web browser and navigate to `https://localhost:5001/api/blogposts`
- Using Postman
	1. Navigate to Settings and turn off "SSL certificate verification"
	1. Create new collection
	1. Add two new requests and validate they work (Send & Save): `GET http://localhost:5000/api/blogposts` and `GET https://localhost:5001/api/blogposts`

### Exercise set #1 - in-memory store & basic CRUD
- API versioning is important for consumers, thus change routing for our endpoint from `/api/blogposts` to `/api/v1/blogposts`. Validate using Postman that endpoints still work.
- Using Visual Studio
	1. Add new model BlogPost under Models folder (`./Models/BlogPost.cs`): right-click the project, select Add > New Folder and name it Models, then right-click the Models folder and select Add > Class (or New File when using VS Code). Name the class BlogPost and click Add. Update the model with the following properties/fields: 
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
	1. Then we can use it in our BlogPostsController:
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
	`GET https://localhost:5001/api/v1/blogposts/923829`
	1. Send and notice the 400 HTTP status code:\
	`GET https://localhost:5001/api/v1/blogposts/' or 1=1`
	1. Add "POST" request and validate it work (don't forget to hit Save!):\
	`POST https://localhost:5001/api/v1/blogposts { "title": "New Post", "description": "Lorem ipsum..." }`
	1. Notice the 201 status code and generated route for the new blog post (Headers -> Location). Validate the URI is accessible.
	1. Add "PUT" request and validate it work:\
	`PUT https://localhost:5001/api/v1/blogposts/1 { "title": "New Post With Fixed Title", "description": "Lorem ipsum..." }`
	1. Add "DELETE" request and validate it work:\
	`DELETE https://localhost:5001/api/v1/blogposts/1`
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
### Exercise set #4 - Global exception handler
- It is a good practice to always return consumable JSON error representation when building RESTful WebAPI.
- Using Visual Studio:
	1. Update `Startup.cs` and configure the HTTP request pipeline to redirect to `/api/v1/Error` controller whenever unhandled exception happens. This should work only if not in development mode, since in development we should get dev exception page with all sort of details (especially when queried with "?throw=true").
		```csharp
		app.UseExceptionHandler("/api/Error");
		```
	1. Create new controller named `ErrorController`. Setup `Index()` method that return status 500 and display friendly message in JSON format. Hint:
		```json
		{ "error": "Unhandled exception" }
		```
		```csharp
		 return StatusCode(..., new {...})
		```
	1. Fake one of the methods in `BlogPostsController` and throw exception from within, i.e:
		```csharp
		public async Task<ActionResult<IEnumerable<BlogPost>>> Get()
		{
			throw new BlogPostsDomainException("No posts atm");
		}
		```
	1. Query it with Postman and confirm you get 500 status code with JSON body.
	1. *(optional) Try returning exception details and URI path from where it was issued. Google `HttpContext.Features.Get<IExceptionHandlerPathFeature>()`...*
	1. Remove the exception from faked method.
### Exercise set #5 - Logging
- Few hints about logging
	- Never log any sensitive data (i.: usernames) at any level that goes to production. Trace/Debug level is less restrictive as long as it is only for development environment and does not involve "live" data.
	- **Trace** - debugging only, allow developer to track program execution, like begin/end of a method, bigger steps in algorithm, loops.
	- **Debug** - strictly for development & debugging purposes, like reading variable/model values, list sizes, consitions.
	- **Info** - usually contains information available in production so it is visible by OPS, used to track calls between "systems", like querying another api, processing a request, bigger steps in algorithms.
	- **Warn** - errors or unexpected behaviors which can be handled by application, like handled exceptions, invalid arguments.
	- **Error** - scenarios that are unrecoverable but can perform another work, like generic error handler.
	- **Critical/Fatal** - you should terminate right after this one... like no disk space, no space on the heap, lack of network connection.
- Using Visual Studio:
	1. Update `Program.cs` and append to the builder pipeline those lines:
		```csharp
		.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
		```
	1. Using DI feed `Startup`, `BlogPostsController` and `ErrorController` classes with either `ILogger<T>` or `ILoggerFactory`.
	1. Apply appropriate logging to each of those classes with appropriate levels. Example:
		```csharp
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
			...
		```
	1. *(optional) Try adding a provider that support writing logs to a file. Google `logging.AddFile(...);`*

### Exercise set #6 - persistent store & initialization
- ORM frameworks like EntityFramework Core not only allow you to easly map database objects to models, but easily swap database providers when needed, i.e: from SQLite > MsSQL > PostgreSQL > MySQL. It also handle database consistency and upgrades (aka migrations).
- Using Visual Studio:
	1. Update `Startup.cs` and switch from in-memory database to MsSQL provider.
		```
		var connection = @"Server=(localdb)\mssqllocaldb;Database=BlogPostsDb;Trusted_Connection=True;ConnectRetryCount=0";
		services.AddDbContext<BlogPostContext>(options => options.UseSqlServer(connection));
		```
	1. Ensure that project builds.
	1. We are going to use Code First workflow and generate database schema from our code (DBContext and models). Add initial database migration and update local instance of MsSQL database (you should have it) accordingly.
		```
		dotnet ef migrations add InitialCreate
		dotnet ef database update
		```
	1. Check the autogenerated migration files under `Migrations` folder. Those are the instructions how to change the databases with each version. There shoul dbe information like: tables and their columns, primary keys, indexes... That is being read by the database provider and translated to database instructions.
	1. Later in the code, remember that whenever you update dbcontext files, models or their relations then you should create new EF migrations.
	1. Run the server and check if everything works. Now whenever you query the web API, you should be able to see generated SQL commands in the console window.
	1. Add around 5 new posts using `POST https://localhost:5001/api/v1/blogposts`
	1. Restart the server and confirm all the posts were stored in the databsae even though the server was stopped. Run `GET https://localhost:5001/api/v1/blogposts`
	1. Now let us switch to SQLite. Update `Startup.cs` and switch from MsSQL provider database to SQLite provider.
		```csharp
		var connection = @"Data Source=Data/Posts.db";
		services.AddDbContextPool<BlogPostContext>(opt => opt.UseSqlite(connection))
		```
	1. For this to work you need to install `Microsoft.EntityFrameworkCore.Sqlite` nuget package, either by installing via Nuget Package Manager or using command line: `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`
	1. Ensure that project builds.
	1. We need to update selected SQLIte database file with the migration instructions generated before.
		```
		dotnet ef database update
		```
	1. Run the server and check if everything works. Now whenever you query the web API, you should be able to see generated SQLLite commands in the console window.
	1. Add X new posts using `POST https://localhost:5001/api/v1/blogposts`
	1. Download portable version of [SQLiteStudio](https://sqlitestudio.pl/files/sqlitestudio3/complete/win32/SQLiteStudio-3.2.1.zip) and open the `Data/Posts.db` file we used as persistent store. Confirm blog posts are in the `BlogPost` table.
	1. When developing the web API, it is a good idea to initialize (aka seed) empty database with test data. Do so either in `Program.cs` (harder but recommended) or in `Startup.cs` (easier):
		```csharp
		if (!env.IsDevelopment())
		{
			context.Database.EnsureCreated();
			if(!context.BlogPosts.Any())
			{
				var posts = new List<BlogPost>
				{
				...
				};
				context.BlogPosts.AddRange(posts);
				context.SaveChanges();
			}
		}
		```
	1. Delete `Data/Posts.db` file and run the server. Database should be recreated using migration instructions and should also be populated with initial data. Run `GET https://localhost:5001/api/v1/blogposts` to confirm.
### Exercise set #7 - repository pattern
- *For larger RESTful web API we could use `Controllers – Services – Repositories – Database` architecture, which provides maximum decoupling of application layers and makes it easy to develop and test the application. For the sake of this exercise we will skip `Services` layer.*
- Repository pattern adds fine abstraction level between the database and the controller. Repositories should encapsulate all the logic needed for accessing the database. If no `Services` laer present, they shoudl return collections as IEnumerable<T> or IAsyncEnumerable<T> (IQueryable<T> is used otherwise).
- Using Visual Studio:
	1. Somewhere under `/Repositories` folder create `IBlogPostRepository` interface and `BlogPostRepository` class that implements it. The `IBlogPostRepository.cs` can look like this:
		```csharp
			public interface IBlogPostRepository
			{
				Task<BlogPost> GetAsync(long id);
				IAsyncEnumerable<BlogPost> GetAllAsync();
				Task AddAsync(BlogPost post);
				Task UpdateAsync(BlogPost post);
				Task DeleteAsync(long id);
			}
		```
	1. Extract out from `BlogPostsController.cs` everything that is connected with accessing databsae under `BlogPostRepository.cs`, long story short: now the only class that should use `BlogPostContext` is `BlogPostRepository`
		```csharp
			private readonly BlogPostContext _context;
			public BlogPostRepository(BlogPostContext context)
			{
				_context = context;
			}
			...
		```
	1. The `BlogPostsController` shoul dnow expect `IBlogPostRepository` to be injected by the build-in Dependency Injection Container, hence its constructor should look like this:
		```csharp
			private readonly ILogger<BlogPostsController> _logger;
			private readonly IBlogPostRepository _postsRepo;
			public BlogPostsController(ILogger<BlogPostsController> logger, IBlogPostRepository repo)
			{
				_logger = logger;
				_postsRepo = repo;
			}
		```
	1. Remember that in order for the Dependency Injection Container to inject proper interface implementation to the constructors, you need to register them under `Startup` > `ConfigureServices`:
		```csharp
		services.AddScoped<IBlogPostRepository, BlogPostRepository>();
		```
	1. Check if everything works now. Everything should behave the same as before. We soon will extend the repository with few more methods.

### Updated requirements
- API v2

| API | Description | Request body | Response body | HTTP status code |
|-|:-|:-:|:-:|:-:|
| GET /api/v2/blogposts[?pageIndex=\{idx\}&pageSize=\{size\}] | Get blog posts page | - | \{ one page of posts \} | 200 OK |

- When requesting a page, there should be a link to the next page in the response body.

### Exercise set #8 - new web API version - v2
- We talked about API versioning and how much we care about backward portability to make consumers/clients of our RESTful APIs happy.
- We are going o add v2 controller that will alter behaviour for retrieving collection of blog posts and there we will use paging instead.
- Using Visual Studio:
	1. Before we proceed we need to install two nuget packages: `Microsoft.AspNetCore.Mvc.Versioning` and `Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer` using either NuGet Package Manager or using cmd:
		```
		dotnet add package Microsoft.AspNetCore.Mvc.Versioning
		dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
		```
	1. Update `Startup` > `ConfigureServices` and "teach" Swagger generator how to create separate documentation for each version of the controller endpoints:
		```csharp
		services.AddMvcCore().AddVersionedApiExplorer(
			options => {
			    options.GroupNameFormat = "'v'VVV";
			    options.SubstituteApiVersionInUrl = true;
			});
            	services.AddApiVersioning(options => options.ReportApiVersions = true);
		services.AddSwaggerGen(
			options => {
			    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
			    foreach (var description in provider.ApiVersionDescriptions)
			    {
				options.SwaggerDoc(description.GroupName,  new Info{ ... Version = description.ApiVersion.ToString(), ... } );
			    }
			});
		```
	1. Update `Startup` > `Configure` and tell Swagger UI where to find documentations:
		```csharp
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider apiVersionDescProvider) {
			app.UseSwaggerUI(c => {
				foreach (var description in apiVersionDescProvider.ApiVersionDescriptions) {
				    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
				}
				c.RoutePrefix = string.Empty; // serve the Swagger UI at the app's root
			});
		...
		```
	1. Modify `BlogPostsController` and replace `[Route("api/v1/[controller]")]` with this:
		```csharp
		[ApiVersion("1")]
		[Route("api/v{version:apiVersion}/BlogPosts")]
		```
	1. Build & run the server. Everything should work as before. Confirm Swagger UI generated same page under `https://localhost:5001/`.
	1. Clone `BlogPostsController.cs` and rename it to `BlogPostsV2Controller.cs`. Open it and change everything that was connected with v1 to v2. Examples:
		- `[ApiVersion("2")]` 
		- `[HttpGet("{id}", Name = "GetBlogPostV2")]`
		- `return CreatedAtRoute("GetBlogPostV2", new { id = post.Id }, post);`
	1. Build & run the server. Everything should work as before with exception that in Swagger UI there should be a dropdown at the top right corner where you can see specification for either V1 or V2 of the API.
- Using Postman:
	1. Duplicate collection of all your previous requests and rename it to `<collection> v2`
	1. Update all the hyperlinks under that collection to target v2 version of the API now. 
	1. Confirm `GET POST PUT DELETE` requests work and that `POST` redirects you to the newly created resource under v2 endpoint, i.e: `Location: https://localhost:5001/api/v2/BlogPosts/6`
