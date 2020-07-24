using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace Splicer
{
    public static class Splicer
    {
        [FunctionName("Splicer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string responseMessage = "";
            string urlString = "";
            List<String> urls = new List<string>();
            urlString += req.Query["sites"];
            if (urlString.Length > 0)
            {
                urls = urlString.Split(",").ToList();
                log.LogInformation(urls[0]);
                try
                {
                    string splicedboi = await RunDownloadParallelAsync(urls);
                    log.LogDebug(splicedboi);
                    responseMessage += splicedboi;
                }
                catch(Exception e)
                {
                    log.LogError(e.Message);
                    responseMessage += "Please enter valid URLs";
                }

                watch.Stop();

            }
            else
                responseMessage += "Please enter at least one URL separated by commas";

            var elapsedMs = watch.ElapsedMilliseconds;
            responseMessage += $"\nTotal execution time: { elapsedMs }";

            return new OkObjectResult(responseMessage);
        }

        /*private void executeSync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            RunDownloadSync();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Total execution time: { elapsedMs }";
        }

        private async void executeAsync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            await RunDownloadParallelAsync();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Total execution time: { elapsedMs }";
        }*/

        private static async Task<string> RunDownloadAsync(List<string> websites)
        {
            string splicedWebsite = "";
            foreach (string site in websites)
            {
                splicedWebsite += await Task.Run(() => DownloadWebsite(site));
            }
            return splicedWebsite;
        }

        private static async Task<string> RunDownloadParallelAsync(List<string> websites)
        {
            List<Task<string>> tasks = new List<Task<string>>();

            foreach (string site in websites)
            {
                tasks.Add(DownloadWebsiteAsync(site));
            }

            var results = await Task.WhenAll(tasks);
            string splicedWebsite = "";

            foreach (var item in results)
            {
                splicedWebsite += item;
            }

            return splicedWebsite;
        }

        private static async Task<string> DownloadWebsiteAsync(string websiteURL)
        {
            string output = "";
            WebClient client = new WebClient();
            output = await client.DownloadStringTaskAsync(websiteURL);

            return output;
        }

        private static String DownloadWebsite(string websiteURL)
        {
            string output = "";
            WebClient client = new WebClient();

            output = client.DownloadString(websiteURL);
            return output;
        }
    }


}
