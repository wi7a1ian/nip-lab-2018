﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nip.Blog.Services.Posts.API.Data;
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
            _logger.LogInformation("Adding in-memory BlogPosts database");
            services.AddDbContext<BlogPostContext>(opt => opt.UseInMemoryDatabase("BlogPosts"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            _logger.LogInformation("Adding Swagger documentation generator");
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Blog Posts API",
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
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _logger.LogInformation("Adding Swagger UI");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog Posts API v1");
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
    }
}
