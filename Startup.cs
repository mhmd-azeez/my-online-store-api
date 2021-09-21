using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyOnlineStoreAPI", Version = "v1" });

                c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.OAuth2,
                    OpenIdConnectUrl = new Uri("https://my-online-store.eu.auth0.com/.well-known/openid-configuration"),
                    
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://my-online-store.eu.auth0.com/authorize"),
                            TokenUrl = new Uri("https://my-online-store.eu.auth0.com/oauth/token"),

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
                        options.Authority = "https://my-online-store.eu.auth0.com/";
                        options.Audience = "https://api.my-online-shop.com";
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyOnlineStoreAPI v1");
                    c.OAuthClientId("Iw5MdATlJXNBO6PUjNx6CpIflE0wJw8v");
                    c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                    {
                        { "audience", "https://api.my-online-shop.com" }
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
