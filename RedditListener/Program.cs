//System Includes
using System.Net.Http.Headers;
using System.Text.Json;
using RedditListener;

//Kick Off App
await MainApp();

static async Task MainApp()
{
    //Instantiate config object
    Configuration Config = new Configuration();

    //Setup HTTP Client
    HttpClient Client = new HttpClient();
    Client = await RedditAPIUtility.SetupClient(Config, Client);

    //Get UNIX UTC Time at start of app
    DateTime StartTime = DateTime.UtcNow;
    double StartTimeUTC = ((DateTimeOffset)StartTime).ToUnixTimeSeconds();

    do
    {
        //Let's loop until Escape is hit
        while (!Console.KeyAvailable)
        {
            //Inform of exit button
            Console.WriteLine("Process Started: " + DateTimeOffset.FromUnixTimeSeconds((long)StartTimeUTC));
            Console.WriteLine("Config loaded from: " + Config.SetFrom);
            Console.WriteLine("Press ESC to stop");

            //Setup Parallel Async HTTP Requests
            ParallelOptions ParallelOptions = new()
            {
                MaxDegreeOfParallelism = Config.MaxDegreeOfParallelism
            };
            
            //1 thread per subreddit we will monitor
            int NumberOfThreads = Config.SubRedditsToMonitor.Length;
            
            //Start the parallel threads
            await Parallel.ForEachAsync(Config.SubRedditsToMonitor, ParallelOptions, async (Subreddit, ct) =>
            {
                //Instantiate Return Object
                RedditReturnObject ReturnedData = new RedditReturnObject();
                int ThreadSeep = 1000;
                try
                {
                    //Call reddit and get the new posts data
                    ReturnedData = await RedditAPIUtility.ProcessRedditPostsAsync(Client, StartTimeUTC, Subreddit, Config);

                    //Figure out how long we should pause thread based on rate limits and number of threads that will also count against limits
                    ThreadSeep = RedditAPIUtility.CalculateThreadSleep(ReturnedData.xRateLimitReset, ReturnedData.xRateLimitRemaining, NumberOfThreads);

                    //Output the thread and rate limit data to console in a readable format
                    Console.WriteLine("");
                    Console.WriteLine("#######################");
                    Console.WriteLine("Subreddit: " + Subreddit);
                    Console.WriteLine("-----------------------");
                    Console.WriteLine("Number of Threads: " + NumberOfThreads);
                    Console.WriteLine("Delay Between Requests: " + (ThreadSeep / 1000) + " seconds");
                    Console.WriteLine("Request Limit Used: " + ReturnedData.xRateLimitUsed);
                    Console.WriteLine("Request Limit Remaining: " + ReturnedData.xRateLimitRemaining);
                    Console.WriteLine("Request Limit Reset: " + ReturnedData.xRateLimitReset + " seconds");
                    Console.WriteLine("");
                    Console.WriteLine("-----------------------");
                    Console.WriteLine("Top " + ReturnedData.NumberOfPostsToTrack + " Posts:");
                    Console.WriteLine("-----------------------");


                    //Output the Top Posts data to console in a readable format
                    if (ReturnedData is not null && ReturnedData.PostSummaries.Count > 0)
                    {
                        foreach (RedditPostSummary PostSummary in ReturnedData.PostSummaries)
                        {

                            Console.WriteLine("Title: " + PostSummary.Title);
                            Console.WriteLine("Upvotes: " + PostSummary.Upvotes);
                            Console.WriteLine("Author: " + PostSummary.Author);
                            Console.WriteLine("Created UTC: " + DateTimeOffset.FromUnixTimeSeconds(PostSummary.CreatedUTC));
                            Console.WriteLine("Post ID: " + PostSummary.ID);
                            Console.WriteLine("Permalink: " + PostSummary.PermaLink);
                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("NO POSTS YET, PLEASE STAND BY");
                    }
                    
                    //Output the Top Authors data to console in a readable format
                    Console.WriteLine("-----------------------");
                    Console.WriteLine("Top " + ReturnedData.NumberOfAuthorsToTrack + " Authors with Most Posts:");
                    Console.WriteLine("-----------------------");

                    if (ReturnedData.AuthorCounts.Count > 0)
                    {
                        foreach (KeyValuePair<string, int> Entry in ReturnedData.AuthorCounts)
                        {
                            Console.WriteLine("Author: " + Entry.Key);
                            Console.WriteLine("# Posts: " + Entry.Value);
                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("NO AUTHORS YET, PLEASE STAND BY");
                    }
                    Console.WriteLine("-----------------------");

                    //Sleep thread for calculated time to maximize usage of rate limit spread across threads
                    Thread.Sleep(ThreadSeep);
                }
                catch (Exception ex)
                {
                    //There was an error, display output
                    Console.WriteLine("#######################");
                    Console.WriteLine("ERROR: " + ex.Message);
                    Console.WriteLine("#######################");
                }
            });

            //Clear console for next display, and add a tiny delay to make sure there are no weird display artifacts
            Console.Clear();
            Thread.Sleep(1000);
        }
        //Continue until escape key is pressed
    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
}


