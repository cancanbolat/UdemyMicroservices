using FreeCourse.Gateway.DelegateHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace FreeCourse.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<TokenExchangeDelegateHandler>();

            services.AddAuthentication().AddJwtBearer("GatewayAuthenticationScheme", op =>
            {
                op.Authority = Configuration["IdentityServerURL"];
                op.Audience = "resource_gateway";
                op.RequireHttpsMetadata = false;
            });

            services.AddOcelot().AddDelegatingHandler<TokenExchangeDelegateHandler>();
        }

        async public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            await app.UseOcelot();
        }
    }
}
