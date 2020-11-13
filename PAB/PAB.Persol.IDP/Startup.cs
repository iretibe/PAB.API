using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PAB.Persol.IDP.Configuration;
using PAB.Persol.IDP.Entities;
using PAB.Persol.IDP.Services;

namespace PAB.Persol.IDP
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        private readonly ILogger<string> _logger;
        
        public Startup(IConfiguration configuration, ILogger<string> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public X509Certificate2 LoadCertificateFromStore()
        {
            string thumbPrint = _configuration["ThumbPrint"];

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint,
                    thumbPrint, true);
                if (certCollection.Count == 0)
                {
                    throw new Exception("The specified certificate wasn't found.");
                }
                return certCollection[0];
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _configuration["ConnectionStrings:PersolConn"];
            var adminConnectionString = _configuration["ConnectionStrings:Admin"];
            var https = Convert.ToBoolean(_configuration["Https:RequireHttpsMetadata"]);

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });


            services.AddIdentityServer()
                .AddSigninCredentialFromConfig(_configuration.GetSection("SigninKeyCredentials"), _logger)
                .AddPABUserStore()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
               // this adds the operational data from DB (codes, tokens, consents)
               .AddOperationalStore(options =>
               {
                   options.ConfigureDbContext = builder =>
                       builder.UseSqlServer(connectionString,
                           sql => sql.MigrationsAssembly(migrationsAssembly));

                   // this enables automatic token cleanup. this is optional.
                   options.EnableTokenCleanup = true;
                   options.TokenCleanupInterval = 30;
               })
               .AddExtensionGrantValidator<OnBehalfOfGrantValidator>();

            services.AddDbContextPool<PABUserContext>(o =>
            {
                o.UseSqlServer(adminConnectionString);
            });

            services.AddScoped<IPABUserRepository, PABUserRepository>();

            
            services.AddMvc(options =>
            {
                //automatically apply ValidateAntiForgeryToken on all updates, puts and patch
                // For cross site request forgery
                //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                if (https)
                {
                  options.Filters.Add(new RequireHttpsAttribute());  
                }
                
            });

            //services.Configure<IISOptions>(iis =>
            //{
            //    iis.AuthenticationDisplayName = "Windows";
            //    iis.AutomaticAuthentication = false;
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, PABUserContext pabUserContext)
        {
            var https = Convert.ToBoolean(_configuration["Https:RequireHttpsMetadata"]);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseWebApiExceptionHandler();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


            //browser will force user to https even if user removes the (s).
            //HTTP Strict Transport Security (HSTS)
            //if (https)
            //{
            //    app.UseHsts(options => options.MaxAge(365).IncludeSubdomains());
            //}
            


            //X-Content-Type-Options
            app.UseXContentTypeOptions();

            app.UseReferrerPolicy(opts => opts.NoReferrer());

            /* Uncomment later */
            //redirects to https or secure port
            //var options = new RewriteOptions()
            //    .AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 63423);
            //app.UseRewriter(options);


            MigrateInMemoryDataToSqlServer(app);

            // for Content-Security-Policy
            //app.UseCsp(opts => opts
            //    .BlockAllMixedContent()
            //    .StyleSources(s => s.Self())
            //    .StyleSources(s => s.UnsafeInline())
            //    .FontSources(s => s.Self())
            //    .FormActions(s => s.Self())
            //    .FrameAncestors(s => s.Self())
            //    .ImageSources(s => s.Self())
            //    .ScriptSources(s => s.Self())
            //);

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.UseStatusCodePages();

            pabUserContext.Database.Migrate();

            app.UseIdentityServer();

            app.UseStaticFiles();

            //X-XSS-Protection
            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.UseXfo(xfo => xfo.Deny());

            app.UseMvcWithDefaultRoute();

        }

        public void MigrateInMemoryDataToSqlServer(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in InMemoryConfiguration.GetClients(_configuration))
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in InMemoryConfiguration.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var apiresource in InMemoryConfiguration.GetApiResources())
                    {
                        context.ApiResources.Add(apiresource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
