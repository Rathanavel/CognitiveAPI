
using Microsoft.Owin;
using Owin;

using Microsoft.Owin.Security.ActiveDirectory;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens;


[assembly: OwinStartup(typeof(CognitiveAPI.App_Start.Startup))]
namespace CognitiveAPI.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            ConfigureAuth(app);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var azureADBearerAuthOptions = new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            {
                Tenant = ConfigurationManager.AppSettings["ida:Tenant"]
            };

            azureADBearerAuthOptions.TokenValidationParameters =
                new TokenValidationParameters()
                {
                    ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
                };

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(azureADBearerAuthOptions);
        }
    }
}