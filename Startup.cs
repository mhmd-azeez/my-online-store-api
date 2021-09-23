using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using MyOnlineStoreAPI.Data;
using MyOnlineStoreAPI.Helpers;

namespace MyOnlineStoreAPI
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
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Database=my_store;Username=postgres;Password=root");
            });
            
            services.Configure<CurrencyScoopOptions>(Configuration.GetSection("CurrencyScoop"));
            services.AddScoped<CurrencyService>();

            services.AddHttpClient<CurrencyService>((serviceProvider, client) => 
            {
                var options = serviceProvider.GetRequiredService<IOptions<CurrencyScoopOptions>>().Value;
                client.BaseAddress = new System.Uri(options.BaseUrl);
            });

            services.Configure<AuthOptions>(Configuration.GetSection("Auth"));
            var authOptions = Configuration.GetOptions<AuthOptions>("Auth");

            services.AddHttpClient("IdPClient", (serviceProvider, httpClient) =>
            {
               httpClient.BaseAddress = new Uri(authOptions.Authority);
            });

            services.AddHttpClient("Auth0Client", httpClient =>
            {
               httpClient.BaseAddress = new Uri(authOptions.ManagementApiUrl);
            });
            
            services.AddScoped<Auth0Service>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyOnlineStoreAPI", Version = "v1" });

                c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.OAuth2,

                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{authOptions.Authority}/authorize"),
                            TokenUrl = new Uri($"{authOptions.Authority}/oauth/token"),

                            // https://auth0.com/docs/configure/apis/scopes
                            // https://auth0.com/docs/configure/apis/scopes/openid-connect-scopes
                            Scopes = new Dictionary<string, string>
                            {
                                { OpenIdConnectScope.OpenId, "User ID" },
                                { OpenIdConnectScope.OpenIdProfile, "User name(s), picture, and updated_at" },
                                { OpenIdConnectScope.Email, "User email" },
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme 
                        { 
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "OAuth2" }
                        },
                        new List<string> { "OAuth2" }
                    }
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authOptions.Authority;
                        options.Audience = authOptions.Audience;
                    });

            services.AddSingleton<IAuthorizationHandler, PermissionRequirementHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPermissionPolicy(Permissions.CurrencyGet);

                options.AddPermissionPolicy(Permissions.ProductsList);
                options.AddPermissionPolicy(Permissions.ProductsGet);
                options.AddPermissionPolicy(Permissions.ProductsCreate);
                options.AddPermissionPolicy(Permissions.ProductsUpdate);
                options.AddPermissionPolicy(Permissions.ProductsDelete);

                options.AddPermissionPolicy(Permissions.UsersSearch);
                options.AddPermissionPolicy(Permissions.UsersOnboard);
            });

            services.AddScoped<IClaimsTransformation, UserRoleClaimsTransformation>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<AuthOptions> authOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyOnlineStoreAPI v1");
                    c.OAuthClientId(authOptions.Value.SwaggerClientId);
                    c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                    {
                        { "audience", authOptions.Value.Audience }
                    });
                    c.OAuthUsePkce();
                    c.OAuthAppName("Test Name");
                    c.ShowCommonExtensions();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
