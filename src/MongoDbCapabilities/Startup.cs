using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using ForEvolve.ExceptionMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
                    .AddClasses(classes => classes.AssignableTo(typeof(ValidationBehavior<,>)))
                    .As(typeof(IPipelineBehavior<,>))
                    .WithTransientLifetime()
                )

                .AddExceptionMapper(builder => builder
                    .MapCommonHttpExceptions()
                    .Map<ValidationException>(map => map.ToStatusCode(400))
                    .Map<Exception>(map => map.To(async ctx =>
                    {
                        // TODO: Move this into a class that implements the
                        // ForEvolve.ExceptionMapper.IExceptionHandler interface.
                        // Move that class to the ForEvolve.ExceptionMapper solution.
                        var env = ctx.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
                        var problemDetails = ctx.HttpContext.RequestServices
                            .GetService<ProblemDetailsFactory>()
                            .CreateProblemDetails(
                                ctx.HttpContext,
                                title: ctx.Error.Message,
                                statusCode: ctx.HttpContext.Response.StatusCode
                            );

                        if (ctx.Error is ValidationException validationException)
                        {
                            problemDetails.Title = "One or more validation errors occurred.";
                            var errors = validationException.Errors
                                .GroupBy(x => x.PropertyName)
                                .Select(e => new KeyValuePair<string, string[]>(
                                    e.Key,
                                    e.Select(x => x.ErrorMessage).ToArray()
                                ));
                            var dict = new Dictionary<string, string[]>(errors);
                            problemDetails.Extensions.Add("errors", dict);
                        }
                        else if(env.IsDevelopment())
                        {
                            problemDetails.Extensions.Add(
                                "debug",
                                new
                                {
                                    type = ctx.Error.GetType().Name,
                                    stackTrace = ctx.Error.StackTrace,
                                }
                            );
                        }

                        ctx.HttpContext.Response.ContentType = "application/problem+json";
                        await JsonSerializer.SerializeAsync(
                            ctx.HttpContext.Response.Body,
                            problemDetails
                        );
                    }))
                )

                .AddSwaggerDocument()

                .AddControllers(options => options.Filters.Add<ModelStateValidationFilter>());
            ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionMapper();
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
