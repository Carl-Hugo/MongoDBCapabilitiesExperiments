using System;
using System.Linq;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoDbCapabilities
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<MongoOptions>(_configuration.GetSection("Mongo"))
                .AddSingleton<IValidateOptions<MongoOptions>, MongoOptionsValidator>()
                .AddSingleton(sp => sp.GetService<IOptionsMonitor<MongoOptions>>().CurrentValue)
            ;
            services
                .AddSingleton<IValidateOptions<MongoDocumentOptions>, MongoDocumentOptionsValidator>()
                .AddSingleton(sp => sp.GetService<IOptionsMonitor<MongoDocumentOptions>>().CurrentValue)
                .AddOptions<MongoDocumentOptions>().Configure(options =>
                {
                    options.CollectionName = "docs";
                    options.DatabaseName = "db";
                })
            ;
            var assembly = GetType().Assembly;
            services
                .AddTransient<IMongoClient, MyMongoClient>()

                .AddMediatR(assembly)
                .AddAutoMapper(assembly)
                .AddValidatorsFromAssembly(assembly)

                .Scan(s => s
                    .FromAssemblyOf<Startup>()
                    .AddClasses(classes => classes.AssignableTo(typeof(ExceptionBehavior<,>)))
                    .As(typeof(IPipelineBehavior<,>))
                    .WithTransientLifetime()
                )


                .AddSwaggerDocument()

                .AddControllers()
            ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseOpenApi();
            app.UseSwaggerUi3();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
