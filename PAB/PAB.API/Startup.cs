using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AspNetCoreRateLimit;
using CoreFlogger;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PAB.API.Helpers;
using PAB.Entity;
using PAB.API.Model;
using PAB.RepositoryInterface;
using PAB.Repository;

namespace PAB.API
{
    public class Startup
    {

        public static IConfiguration _config;
        
        public Startup(IConfiguration config) 
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var clientId = _config["IdentityServerSettings:ApiName"];

            //services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddResponseCaching();


            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new Info
                {
                    Version = ".Net Core 2.0 v1",
                    Title = "Persol Personal Address Book Core Service",
                    Description = "An ASP.NET Core Web API For PAB Service",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "SWD API Team",
                        Email = "eric.boateng@persol.net;somad.yessoufou@persol.net;emmanuel.asaber@persol.net",
                        Url = "http://www.persol.net/"
                    },
                    License = new License { Name = "Persol Systems Ltd", Url = "http://www.persol.net/" }
                });

                //opt.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                opt.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    AuthorizationUrl = _config["IdentityServerSettings:AuthorityURL"],
                    Flow = "implicit",
                    TokenUrl = _config["IdentityServerSettings:TokenUrl"],
                    Scopes = new Dictionary<string, string>
                    {
                        { "pabapi", "The Scope needed to Access PAB API" }
                    }

                });

                ////// Assign scope requirements to operations based on AuthorizeAttribute
                opt.OperationFilter<AuthorizeCheckOperationFilter>();

                //opt.OperationFilter<SwaggerSecurityOperationFilter>("pabapi");

            });

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = _config["IdentityServerSettings:Authority"];
                    options.RequireHttpsMetadata = false;
                    options.ApiName = _config["IdentityServerSettings:ApiId"];
                    options.ApiSecret = _config["IdentityServerSettings:Secret"];
                    options.SaveToken = true;
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default

                });

            services.AddMvc(options =>
            {

                options.Filters.Add(new TrackPerformanceFilter("Persol Personal Address Book", "Core API"));
                options.Filters.Add(new TrackUsageFilter("Persol Personal Address Book", "Core API"));

                // automatically apply ValidateAntiForgeryToken on all updates, puts and patch
                // For cross site request forgery
                //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                //this requires the app runs only on https
                //options.Filters.Add(new RequireHttpsAttribute());

                options.ReturnHttpNotAcceptable = true;
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                options.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

                var jsonOutputFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();

                jsonOutputFormatter?.SupportedMediaTypes.Add("application/vnd.persol.hateoas+json");

            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

            //services.AddRaygun(_config, new RaygunMiddlewareSettings
            //{
            //    ClientProvider = new PABRaygunAspNetCoreClientProvider()
            //});

            List<string> urlList = _config.GetSection("WebClients:Links").Get<List<string>>();
            
            string[] clientUrls = urlList.Select(i => i.ToString()).ToArray();


            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(clientUrls)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

  
            var strCon = _config["connectionStrings:DefaultConnection"];

            services.AddDbContextPool<PABContext>(o => o.UseSqlServer(strCon).EnableSensitiveDataLogging());

            services.AddScoped<IContactNameRepo, ContactNameRepo>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddTransient<ITypeHelperService, TypeHelperService>();

            services.AddHttpCacheHeaders(
                expirationModelOptions =>
                {
                    expirationModelOptions.MaxAge = 600;
                },
                validationModelOptions =>
                {
                    validationModelOptions.AddMustRevalidate = true;
                });

            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Limit = 1000,
                        Period = "5m"
                    },
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Limit = 200,
                        Period = "10s"
                    }
                };
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var clientSecret = _config["IdentityServerSettings:Secret"];
            var clientId = _config["IdentityServerSettings:ApiName"];

            //var clientFileSecret = _config.GetConnectionString("PABAPI");

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseWebApiExceptionHandler();

            //}

            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});

            //X-Content-Type-Options
            //app.UseXContentTypeOptions();

            //app.UseReferrerPolicy(opts => opts.NoReferrer());

            app.UseStaticFiles();

            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");

            //    await next();
            //});

            app.UseStatusCodePages();

            //X-XSS-Protection
            //app.UseXXssProtection(options => options.EnabledWithBlockMode());

            //app.UseXfo(xfo => xfo.Deny());

            app.UseRedirectValidation();

            AutoMapperSettings();

            app.UseIpRateLimiting();

            app.UseResponseCaching();

            app.UseHttpCacheHeaders();

            app.UseSwagger();

#if DEBUG
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Address Book API");

                //c.ConfigureOAuth2(clientId, clientSecret, "", "");

                c.OAuthClientId(clientId);  //Default clientId. MUST be a string
                c.OAuthClientSecret(clientSecret); //	Default clientSecret. MUST be a string
                c.OAuthRealm(""); //realm query parameter (for oauth1) added to authorizationUrl and tokenUrl . MUST be a string
                c.OAuthAppName("PAB API"); //application name, displayed in authorization popup. MUST be a string
                c.OAuthScopeSeparator(" "); //scope separator for passing scopes, encoded before calling, default value is a space (encoded value % 20).MUST be a string
                //c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                
            });
#else
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(_config["AppSettings:Folder"] + "/swagger/v1/swagger.json", "Personal Address Book API");
                //c.ConfigureOAuth2(clientId, clientSecret, "", "");

                c.OAuthClientId(clientId);  //Default clientId. MUST be a string
                c.OAuthClientSecret(clientSecret); //	Default clientSecret. MUST be a string
                c.OAuthRealm("swagger-ui-realm"); //realm query parameter (for oauth1) added to authorizationUrl and tokenUrl . MUST be a string
                c.OAuthAppName("PAB API"); //application name, displayed in authorization popup. MUST be a string
                c.OAuthScopeSeparator(" "); //scope separator for passing scopes, encoded before calling, default value is a space (encoded value % 20).MUST be a string
                //c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });
#endif

            app.UseCors("default");

            app.UseAuthentication();

            //app.UseRaygun();

            //X-XSS-Protection
            //app.UseXXssProtection(options => options.EnabledWithBlockMode());

            //app.UseXfo(xfo => xfo.Deny());

            app.UseExceptionHandler(eApp =>
            {
                eApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var errorCtx = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorCtx != null)
                    {
                        var ex = errorCtx.Error;
                        WebHelper.LogWebError("Persol Personal Address Book", "Core API", ex, context);

                        var errorId = Activity.Current?.Id ?? context.TraceIdentifier;
                        var jsonResponse = JsonConvert.SerializeObject(new CustomErrorResponse
                        {
                            ErrorId = errorId,
                            Message = "Some kind of error happened in the API."
                        });
                        await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);
                    }
                });
            });

            app.UseMvc();
        }

        private static void AutoMapperSettings()
        {
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<psPARContactName, ContactDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.SzFirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.SzLastName))
                    .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.SzMiddleName))
                    .ForMember(dest => dest.NickName, opt => opt.MapFrom(src => src.SzNickName))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.SzTitle))
                    .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.SzSuffix))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.IUserId))
                    .ForMember(dest => dest.Createdate, opt => opt.MapFrom(src => src.DCreatedate));

                cfg.CreateMap<ContactForCreationDto, psPARContactName>()
                    .ForMember(dest => dest.SzFirstName, opt => opt.MapFrom(src => src.FirstName))
                    .ForMember(dest => dest.SzLastName, opt => opt.MapFrom(src => src.LastName))
                    .ForMember(dest => dest.SzMiddleName, opt => opt.MapFrom(src => src.MiddleName))
                    .ForMember(dest => dest.SzNickName, opt => opt.MapFrom(src => src.NickName))
                    .ForMember(dest => dest.SzTitle, opt => opt.MapFrom(src => src.Title))
                    .ForMember(dest => dest.SzSuffix, opt => opt.MapFrom(src => src.Suffix))
                    .ForMember(dest => dest.IUserId, opt => opt.MapFrom(src => src.UserId));

                cfg.CreateMap<AddressForCreationDto, psPARContactAddress>()
                    //.ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzHomeAddress, opt => opt.MapFrom(src => src.HomeAddress))
                    .ForMember(dest => dest.SzBusinessAddress, opt => opt.MapFrom(src => src.BusinessAddress))
                    .ForMember(dest => dest.SzOther, opt => opt.MapFrom(src => src.Other));

                cfg.CreateMap<EmailForCreationDto, psPARContactEmail>()
                    //.ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzEmailAddress1, opt => opt.MapFrom(src => src.EmailAddress1))
                    .ForMember(dest => dest.SzEmailAddress2, opt => opt.MapFrom(src => src.EmailAddress2));

                cfg.CreateMap<OtherForCreationDto, psPARContactOther>()
                    //.ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzPersonalWebPage, opt => opt.MapFrom(src => src.PersonalWebPage))
                    .ForMember(dest => dest.SzSignificantOther, opt => opt.MapFrom(src => src.SignificantOther))
                    .ForMember(dest => dest.DBirthday, opt => opt.MapFrom(src => src.Birthday))
                    .ForMember(dest => dest.DAnniversary, opt => opt.MapFrom(src => src.Anniversary));

                cfg.CreateMap<PhoneForCreationDto, psPARContactPhone>()
                    //.ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzMobile1, opt => opt.MapFrom(src => src.Mobile1))
                    .ForMember(dest => dest.SzMobile2, opt => opt.MapFrom(src => src.Mobile2))
                    .ForMember(dest => dest.SzHome, opt => opt.MapFrom(src => src.Home))
                    .ForMember(dest => dest.SzBusiness, opt => opt.MapFrom(src => src.Business))
                    .ForMember(dest => dest.SzBusinessFax, opt => opt.MapFrom(src => src.BusinessFax))
                    .ForMember(dest => dest.SzHomeFax, opt => opt.MapFrom(src => src.HomeFax));

                cfg.CreateMap<WorkForCreationDto, psPARContactWork>()
                    //.ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzJobTitle, opt => opt.MapFrom(src => src.JobTitle))
                    .ForMember(dest => dest.SzCompany, opt => opt.MapFrom(src => src.Company));

                cfg.CreateMap<ContactForUpdateDto, psPARContactName>()
                    .ForMember(dest => dest.pkId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.SzFirstName, opt => opt.MapFrom(src => src.FirstName))
                    .ForMember(dest => dest.SzLastName, opt => opt.MapFrom(src => src.LastName))
                    .ForMember(dest => dest.SzMiddleName, opt => opt.MapFrom(src => src.MiddleName))
                    .ForMember(dest => dest.SzNickName, opt => opt.MapFrom(src => src.NickName))
                    .ForMember(dest => dest.SzTitle, opt => opt.MapFrom(src => src.Title))
                    .ForMember(dest => dest.SzSuffix, opt => opt.MapFrom(src => src.Suffix))
                    .ForMember(dest => dest.IUserId, opt => opt.MapFrom(src => src.UserId)).ReverseMap();

                cfg.CreateMap<AddressForUpdateDto, psPARContactAddress>()
                    //.ForMember(dest => dest.pkId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzHomeAddress, opt => opt.MapFrom(src => src.HomeAddress))
                    .ForMember(dest => dest.SzBusinessAddress, opt => opt.MapFrom(src => src.BusinessAddress))
                    .ForMember(dest => dest.SzOther, opt => opt.MapFrom(src => src.Other)).ReverseMap();

                cfg.CreateMap<EmailForUpdateDto, psPARContactEmail>()
                    //.ForMember(dest => dest.pkId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzEmailAddress1, opt => opt.MapFrom(src => src.EmailAddress1))
                    .ForMember(dest => dest.SzEmailAddress2, opt => opt.MapFrom(src => src.EmailAddress2)).ReverseMap();

                cfg.CreateMap<OtherForUpdateDto, psPARContactOther>()
                    //.ForMember(dest => dest.pkId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzPersonalWebPage, opt => opt.MapFrom(src => src.PersonalWebPage))
                    .ForMember(dest => dest.SzSignificantOther, opt => opt.MapFrom(src => src.SignificantOther))
                    .ForMember(dest => dest.DBirthday, opt => opt.MapFrom(src => src.Birthday))
                    .ForMember(dest => dest.DAnniversary, opt => opt.MapFrom(src => src.Anniversary)).ReverseMap();

                cfg.CreateMap<PhoneForUpdateDto, psPARContactPhone>()
                    //.ForMember(dest => dest.pkId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzMobile1, opt => opt.MapFrom(src => src.Mobile1))
                    .ForMember(dest => dest.SzMobile2, opt => opt.MapFrom(src => src.Mobile2))
                    .ForMember(dest => dest.SzHome, opt => opt.MapFrom(src => src.Home))
                    .ForMember(dest => dest.SzBusiness, opt => opt.MapFrom(src => src.Business))
                    .ForMember(dest => dest.SzBusinessFax, opt => opt.MapFrom(src => src.BusinessFax))
                    .ForMember(dest => dest.SzHomeFax, opt => opt.MapFrom(src => src.HomeFax)).ReverseMap();

                cfg.CreateMap<WorkForUpdateDto, psPARContactWork>()
                    //.ForMember(dest => dest.pkId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.IContactNameId, opt => opt.MapFrom(src => src.ContactNameId))
                    .ForMember(dest => dest.SzJobTitle, opt => opt.MapFrom(src => src.JobTitle))
                    .ForMember(dest => dest.SzCompany, opt => opt.MapFrom(src => src.Company)).ReverseMap();

                //cfg.CreateMap<psPARContactAddress, AddressForUpdateDto>()
                //    .ForMember(dest => dest.ContactNameId, opt => opt.MapFrom(src => src.IContactNameId))
                //    .ForMember(dest => dest.HomeAddress, opt => opt.MapFrom(src => src.SzHomeAddress))
                //    .ForMember(dest => dest.BusinessAddress, opt => opt.MapFrom(src => src.SzBusinessAddress))
                //    .ForMember(dest => dest.Other, opt => opt.MapFrom(src => src.SzOther));

                //cfg.CreateMap<psPARContactName, ContactForUpdateDto>()
                //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                //    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.SzFirstName))
                //    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.SzLastName))
                //    .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.SzMiddleName))
                //    .ForMember(dest => dest.NickName, opt => opt.MapFrom(src => src.SzNickName))
                //    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.SzTitle))
                //    .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.SzSuffix))
                //    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.IUserId));

                //cfg.CreateMap<psPARContactEmail, EmailForUpdateDto>()
                //    .ForMember(dest => dest.ContactNameId, opt => opt.MapFrom(src => src.IContactNameId))
                //    .ForMember(dest => dest.EmailAddress1, opt => opt.MapFrom(src => src.SzEmailAddress1))
                //    .ForMember(dest => dest.EmailAddress2, opt => opt.MapFrom(src => src.SzEmailAddress2));


                //cfg.CreateMap<psPARContactOther, OtherForUpdateDto>()
                //    .ForMember(dest => dest.ContactNameId, opt => opt.MapFrom(src => src.IContactNameId))
                //    .ForMember(dest => dest.PersonalWebPage, opt => opt.MapFrom(src => src.SzPersonalWebPage))
                //    .ForMember(dest => dest.SignificantOther, opt => opt.MapFrom(src => src.SzSignificantOther))
                //    .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.DBirthday))
                //    .ForMember(dest => dest.Anniversary, opt => opt.MapFrom(src => src.DAnniversary));

                //cfg.CreateMap<psPARContactPhone, PhoneForUpdateDto>()
                //    .ForMember(dest => dest.ContactNameId, opt => opt.MapFrom(src => src.IContactNameId))
                //    .ForMember(dest => dest.Mobile1, opt => opt.MapFrom(src => src.SzMobile1))
                //    .ForMember(dest => dest.Mobile2, opt => opt.MapFrom(src => src.SzMobile2))
                //    .ForMember(dest => dest.Home, opt => opt.MapFrom(src => src.SzHome))
                //    .ForMember(dest => dest.Business, opt => opt.MapFrom(src => src.SzBusiness))
                //    .ForMember(dest => dest.BusinessFax, opt => opt.MapFrom(src => src.SzBusinessFax))
                //    .ForMember(dest => dest.HomeFax, opt => opt.MapFrom(src => src.SzHomeFax));

                //cfg.CreateMap<psPARContactWork, WorkForUpdateDto>()
                //    .ForMember(dest => dest.ContactNameId, opt => opt.MapFrom(src => src.IContactNameId))
                //    .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.SzJobTitle))
                //    .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.SzCompany));

                cfg.CreateMap<psPARContactName, GetAllContactDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.SzFirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.SzLastName))
                    .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.SzMiddleName))
                    .ForMember(dest => dest.NickName, opt => opt.MapFrom(src => src.SzNickName))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.SzTitle))
                    .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.SzSuffix))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.IUserId));

                cfg.CreateMap<psPARContactAddress, GetAllAddressDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.HomeAddress, opt => opt.MapFrom(src => src.SzHomeAddress))
                    .ForMember(dest => dest.BusinessAddress, opt => opt.MapFrom(src => src.SzBusinessAddress))
                    .ForMember(dest => dest.Other, opt => opt.MapFrom(src => src.SzOther));

                cfg.CreateMap<psPARContactEmail, GetAllEmailDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.EmailAddress1, opt => opt.MapFrom(src => src.SzEmailAddress1))
                    .ForMember(dest => dest.EmailAddress2, opt => opt.MapFrom(src => src.SzEmailAddress2));

                cfg.CreateMap<psPARContactOther, GetAllOtherDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.PersonalWebPage, opt => opt.MapFrom(src => src.SzPersonalWebPage))
                    .ForMember(dest => dest.SignificantOther, opt => opt.MapFrom(src => src.SzSignificantOther))
                    .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.DBirthday))
                    .ForMember(dest => dest.Anniversary, opt => opt.MapFrom(src => src.DAnniversary));

                cfg.CreateMap<psPARContactPhone, GetAllPhoneDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.Mobile1, opt => opt.MapFrom(src => src.SzMobile1))
                    .ForMember(dest => dest.Mobile2, opt => opt.MapFrom(src => src.SzMobile2))
                    .ForMember(dest => dest.Home, opt => opt.MapFrom(src => src.SzHome))
                    .ForMember(dest => dest.Business, opt => opt.MapFrom(src => src.SzBusiness))
                    .ForMember(dest => dest.BusinessFax, opt => opt.MapFrom(src => src.SzBusinessFax))
                    .ForMember(dest => dest.HomeFax, opt => opt.MapFrom(src => src.SzHomeFax));

                cfg.CreateMap<psPARContactWork, GetAllWorkDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pkId))
                    .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.SzJobTitle))
                    .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.SzCompany));

            });
        }
    }
}
