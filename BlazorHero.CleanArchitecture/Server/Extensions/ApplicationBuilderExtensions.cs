﻿using System.Globalization;
using System.Linq;
using BlazorHero.CleanArchitecture.Application.Constants.Application;
using BlazorHero.CleanArchitecture.Application.Constants.Localization;
using BlazorHero.CleanArchitecture.DataAccess.Interfaces;
using BlazorHero.CleanArchitecture.Server.Hubs;
using BlazorHero.CleanArchitecture.Server.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorHero.CleanArchitecture.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(
            this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            return app;
        }

        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", typeof(Program).Assembly.GetName().Name);
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
            });
        }

        public static IApplicationBuilder UseEndpoints(this IApplicationBuilder app)
            => app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<SignalRHub>(ApplicationConstants.SignalR.HubUrl);
            });

        public static IApplicationBuilder UseRequestLocalizationByCulture(this IApplicationBuilder app)
        {
            var supportedCultures = LocalizationConstants.SupportedLanguages.Select(l => new CultureInfo(l.Code)).ToArray();
            app.UseRequestLocalization(options =>
            {
                options.SupportedUICultures = supportedCultures;
                options.SupportedCultures = supportedCultures;
                options.DefaultRequestCulture = new RequestCulture(supportedCultures.First());
                options.ApplyCurrentCultureToResponseHeaders = true;
            });

            app.UseMiddleware<RequestCultureMiddleware>();

            return app;
        }

        public static IApplicationBuilder Initialize(this IApplicationBuilder app, Microsoft.Extensions.Configuration.IConfiguration _configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var initializers = serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>();

            foreach (var initializer in initializers)
            {
                initializer.Initialize();
            }

            return app;
        }
    }
}