using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nip.Blog.Services.Posts.API.Data;
using Nip.Blog.Services.Posts.API.Repositories;
using Swashbuckle.AspNetCore.Swagger;

namespace Nip.Blog.Services.Posts.API
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDatabaseProviders(services);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            ConfigureVersioningAndDocProviders(services);

            services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        }

        private void ConfigureDatabaseProviders(IServiceCollection services)
        {
            // CMD> dotnet add package Microsoft.EntityFrameworkCore.Sqlite

            var dbType = Configuration.GetValue<string>("SelectedDbType", "in-memory");
            switch (dbType)
            {
                case "MsSQL":
                    {
                        _logger.LogInformation("Adding MsSQL-backed BlogPosts database");
                        services.AddDbContextPool<BlogPostContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("MsSQLBlogPostsDatabase"))
                                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));
                        break;
                    }
                case "SQLite":
                    {
                        _logger.LogInformation("Adding SQLite-backed BlogPosts database");
                        services.AddDbContextPool<BlogPostContext>(opt => opt.UseSqlite(Configuration.GetConnectionString("SQLiteBlogPostsDatabase"))
                                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));
                        break;
                    }
                default:
                    {
                        _logger.LogInformation("Adding in-memory BlogPosts database");
                        services.AddDbContext<BlogPostContext>(opt => opt.UseInMemoryDatabase("BlogPosts"));

                        break;
                    }
            }

            // CMD> dotnet ef migrations add InitialCreate
            // CMD> dotnet ef database update
        }

        private void ConfigureVersioningAndDocProviders(IServiceCollection services)
        {
            _logger.LogInformation("Adding API versioning provider");
            services.AddMvcCore().AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddApiVersioning(options => options.ReportApiVersions = true);

            _logger.LogInformation("Adding Swagger documentation generator");
            services.AddSwaggerGen(
                options =>
                {
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                    }
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider apiVersionDescProvider)
        {
            _logger.LogInformation("Adding Swagger UI");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var description in apiVersionDescProvider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
                c.RoutePrefix = string.Empty; // serve the Swagger UI at the app's root
            });

            if (env.IsDevelopment())
            {
                _logger.LogInformation("Running in Development environment");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/api/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        static Info CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info()
            {
                Title = $"Blog Posts API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = "RESTful repository allowing basic CRUD operations on blog post resource.",
                TermsOfService = "None",
                Contact = new Contact
                {
                    Name = "Rick Roll",
                    Email = string.Empty,
                    Url = "https://github.com/wi7a1ian"
                },
                License = new License
                {
                    Name = "Use under MIT license",
                    Url = "https://github.com/wi7a1ian/nip-lab-2018/blob/master/LICENSE.md"
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
