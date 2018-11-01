using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveHeadlessDeamon
{
    class Program
    {
        //Client ID & Secret
        private readonly static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private readonly static string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];

        //Azure details & Registered AppKey Endpoint
        private readonly static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private readonly static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        //Source service endpoint
        private readonly static string serviceResourceId = ConfigurationManager.AppSettings["ida:ServicesBaseAddress"];

        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        private static AuthenticationContext authContext = new AuthenticationContext(authority);
        private static ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

        static void Main(string[] args)
        {
            AuthenticationResult authResult = null;
            int retryCount = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    //Making synchronous way to aquire token.
                    authResult = authContext.AcquireToken(serviceResourceId, clientCredential);

                    //asynchronous way - remember to handle the next steps/logics accordingly 
                    Task<AuthenticationResult> authenticationResult = authContext.AcquireTokenAsync(serviceResourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        Console.WriteLine(ex.ErrorCode + Environment.NewLine + ex.Message);
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            } while ((retry == true) && (retryCount < 3));

            if (authResult == null)
            {
                Console.WriteLine("Cancelling attempt ..");
                return;
            }

            Console.WriteLine("Authenticated succesfully.." + Environment.NewLine + "Accessing Enpoint.." + Environment.NewLine);

            AccessEnpoint(authResult, "api/videos").Wait();
            Console.WriteLine(Environment.NewLine + "*******" + Environment.NewLine);

            AccessEnpoint(authResult, "api/video/1").Wait();
            Console.WriteLine(Environment.NewLine + "*******" + Environment.NewLine);

            AccessEnpoint(authResult, "api/video/1/frames").Wait();

            Console.Read();
        }

        /// <summary>
        /// Access enpoint from the authenticated object.
        /// </summary>
        /// <param name="authContext">AuthenticationResult: Authenticated token object</param>
        /// <param name="enpoint">Resource enpoint</param>
        /// <returns></returns>
        private static async Task AccessEnpoint(AuthenticationResult authContext, string enpoint)
        {
            string serviceBaseAddress = "http://ratsubcognitiveapi.azurewebsites.net/";

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authContext.AccessToken);

            HttpResponseMessage response = await httpClient.GetAsync(serviceBaseAddress + enpoint);

            if (response.IsSuccessStatusCode)
            {
                string r = await response.Content.ReadAsStringAsync();
                Console.WriteLine(r);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Program.authContext.TokenCache.Clear();
                    Console.WriteLine("Access Denied!");
                }
                else
                {
                    Console.WriteLine(response.ReasonPhrase);
                }
            }

        }
    }
}
