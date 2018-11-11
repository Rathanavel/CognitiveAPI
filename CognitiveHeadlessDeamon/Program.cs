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
        //App Client ID & Secret
        private readonly static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private readonly static string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];

        //App Native Client ID
        private readonly static string NativeClientId = ConfigurationManager.AppSettings["ida:NativeClientId"];

        //Azure ActiveDirectory details
        private readonly static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private readonly static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        //Source service endpoint
        private readonly static string serviceResourceId = ConfigurationManager.AppSettings["ida:ServiceResourceId"];
        private readonly static string serviceBaseAddress = ConfigurationManager.AppSettings["endpoint:ServiceBaseAddress"];

        //Token Issuer authority
        private static readonly string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //Global Variables
        private static AuthenticationContext authContext = new AuthenticationContext(authority);
        private static ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);
        private static AuthenticationResult authResult = null;

        static void Main(string[] args)
        {
            int retryCount = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    #region Using Client Credentials

                    //synchronous way to aquire token.
                    authResult = authContext.AcquireToken(serviceResourceId, clientCredential);

                    //asynchronous way to aquire token. 
                    //Authenticate(serviceResourceId, clientCredential).Wait();

                    #endregion

                    #region Using User Credentials
                    
                    //Provide different username/password to run this application from same/any Network (or) domain.
                    var userName = ConfigurationManager.AppSettings["ida:UserName"];
                    var password = ConfigurationManager.AppSettings["ida:Password"];
                    UserCredential userCredential = new UserCredential(userName, password);

                    //Use this to run under current LoggedIn user identity, This works for scenario where Desktop/Laptop/VM in connected to same the same domain.
                    //UserCredential userCredential = new UserCredential();

                    authResult = authContext.AcquireToken(serviceResourceId, NativeClientId, userCredential);
                    
                    #endregion
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        Console.WriteLine(ex.ErrorCode + Environment.NewLine + ex.Message);
                        retry = true;
                    }
                    else
                    {
                        retry = true;
                        Console.WriteLine(ex.ErrorCode + Environment.NewLine + ex.Message);
                    }

                    Console.WriteLine(string.Format("Retrying ({0}) of 3 attempt(s)", (retryCount + 1).ToString()));

                    retryCount++;
                    Thread.Sleep(3000);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                    Console.ReadLine();
                    return;
                }
            } while ((retry == true) && (retryCount < 3));

            if (authResult == null)
            {
                Console.WriteLine("Cancelling attempt ..");
                Thread.Sleep(2000);
                return;
            }

            Console.WriteLine("Authenticated succesfully.." + Environment.NewLine + "Accessing Enpoint.." + Environment.NewLine);

            AccessEnpoint(authResult, "api/videos").Wait();

            AccessEnpoint(authResult, "api/video/1").Wait();

            AccessEnpoint(authResult, "api/video/1/frames").Wait();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Completed!");
            Console.ReadLine();
            return;
        }

        /// <summary>
        /// Helper method to acquire authentication result in async way.
        /// </summary>
        /// <param name="resource">Resource id</param>
        /// <param name="cred">Credentials</param>
        /// <returns></returns>
        private static async Task Authenticate(string resource, ClientCredential cred)
        {
            authResult = await authContext.AcquireTokenAsync(serviceResourceId, clientCredential);
        }

        /// <summary>
        /// Access secure enpoint using authenticated context Bearer token.
        /// </summary>
        /// <param name="authResult">AuthenticationResult: Authenticated token object</param>
        /// <param name="enpoint">Resource enpoint</param>
        /// <returns></returns>
        private static async Task AccessEnpoint(AuthenticationResult authResult, string enpoint)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

            HttpResponseMessage response = await httpClient.GetAsync(serviceBaseAddress + enpoint);

            if (response.IsSuccessStatusCode)
            {
                string output = await response.Content.ReadAsStringAsync();
                Console.WriteLine(output);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Access Denied!");
                    authContext.TokenCache.Clear();
                }
                else
                {
                    Console.WriteLine(response.ReasonPhrase);
                }
            }

            Console.WriteLine(Environment.NewLine + "*******" + Environment.NewLine);
        }
    }
}
