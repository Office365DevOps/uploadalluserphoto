using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;
using CommandLine;
using System.Linq;

namespace updatealluserphoto
{
    class Program
    {
        public class Options
        {
            [Option('i', "applicationId")]
            public string ApplicationId { get; set; }
            [Option('s', "clientSecret")]
            public string ClientSecret { get; set; }
            [Option('t', "tenantId")]
            public string TenantId { get; set; }
            [Option('d', "directory")]
            public string Directory { get; set; }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                var appId = o.ApplicationId;
                var secret = o.ClientSecret;
                var tenantId = o.TenantId;
                var directory = o.Directory;

                if (!System.IO.Directory.Exists(directory))
                {
                    Console.WriteLine("Directory is not exist, please check and try again");
                    return;
                }

                var app = ConfidentialClientApplicationBuilder.Create(appId)
                    .WithClientSecret(secret)
                    .WithTenantId(tenantId)
                    .Build();
                var authProvider = new ClientCredentialProvider(app);
                var client = new GraphServiceClient(authProvider);

                var files = new System.IO.DirectoryInfo(directory).GetFiles("*.jpg").Where(x => x.Length <= 4 * 1024 *1024);

                foreach (var file in files)
                {

                    var user = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    using (var stream = System.IO.File.OpenRead(file.FullName))
                    {
                        client.Users[user].Photo.Content.Request().PutAsync(stream).Wait();
                        stream.Close();
                    }

                    Console.WriteLine("Done - {0}", user);
                }
            })
            .WithNotParsed(Error =>
            {
                Console.WriteLine("Please check the args...");
            });
        }
    }
}
