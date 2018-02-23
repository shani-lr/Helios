using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Helios
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddSigningCredential(new X509Certificate2(
                    @"/Users/Shani/Projects/Helios/Helios/Security/helios.pfx", "password"))
                .AddTestUsers(InMemoryConfiguration.Users().ToList())
                .AddInMemoryClients(InMemoryConfiguration.Clients())
                .AddInMemoryApiResources(InMemoryConfiguration.ApiResources());

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}