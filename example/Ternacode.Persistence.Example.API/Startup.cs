using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ternacode.Persistence.EntityFrameworkCore.Extensions;
using Ternacode.Persistence.Example.API.Contracts.Errors;
using Ternacode.Persistence.Example.Database;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Interfaces;
using Ternacode.Persistence.Example.Domain.Processes.Users;
using Ternacode.Persistence.Example.Domain.Processes.Users.Interfaces;

namespace Ternacode.Persistence.Example.API
{
    public class Startup
    {
        private readonly IConfigurationRoot _config;

        public Startup()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var dbConnectionString = _config.GetSection("ConnectionStrings:SqlServer").Value;

            // Add the persistence services to the IServiceCollection:
            // Define the DbContext factory and what entities are used.
            services.AddPersistence(() =>
                    {
                        var factory = new BlogContextFactory(dbConnectionString);
                        return factory.CreateDbContext();
                    })
                    .AddEntity(c => c.Users)
                    .AddEntity(c => c.Posts);

            services.AddTransient<IBlogPostsProcess, BlogPostsProcess>()
                    .AddTransient<IUsersProcess, UsersProcess>();

            services.AddMvc(o => o.EnableEndpointRouting = false);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Ternacode.Persistence Example API",
                    Description = "A small API application showcasing Ternacode.Persistence"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Add a simple exception handler
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/plain";

                    var exception = context.Features?.Get<IExceptionHandlerFeature>()?.Error;
                    var errorResponse = JsonConvert.SerializeObject(
                        new ExceptionResponse(exception),
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });

                    await context.Response.WriteAsync(errorResponse);
                });
            });

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ternacode.Persistence Example API v1");
            });
        }
    }
}
