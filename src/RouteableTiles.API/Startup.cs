﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OsmSharp.Db.Tiled;
using RouteableTiles.API.Results;
using RouteableTiles.IO.JsonLD.Semantics;
using Serilog;

namespace RouteableTiles.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
                {
                    options.OutputFormatters.Insert(0, new JsonLDOutputFormatter());
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
            };
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            
            app.UseForwardedHeaders(options);
            app.Use((context, next) => 
            {
                if (context.Request.Headers.TryGetValue("X-Forwarded-PathBase", out var pathBases))
                {
                    context.Request.PathBase = pathBases.First();
                    if (context.Request.PathBase.Value.EndsWith("/"))
                    {
                        context.Request.PathBase =
                            context.Request.PathBase.Value.Substring(0, context.Request.PathBase.Value.Length - 1);
                    }
                    if (context.Request.Path.Value.StartsWith(context.Request.PathBase.Value))
                    {
                        var before = context.Request.Path.Value;
                        var after = context.Request.Path.Value.Substring(
                            context.Request.PathBase.Value.Length,
                            context.Request.Path.Value.Length - context.Request.PathBase.Value.Length);
                        context.Request.Path = after;
                    }
                }
                return next();
            });
            
            if (!OsmDb.TryLoad(this.Configuration["db"], out var osmDb)) throw new Exception("Osm DB not found!");
            DatabaseInstance.Default = osmDb;
            
            JsonLDOutputFormatter.Mapping = TagMapperConfigParser.Parse(this.Configuration["mapping"]);
            JsonLDOutputFormatter.MappingKeys = TagMapperConfigParser.ParseKeys(this.Configuration["mapping_keys"]);

            app.UseMvc();
        }
    }
}