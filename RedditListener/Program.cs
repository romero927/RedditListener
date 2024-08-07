//System Includes
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using System.Text.Json;
using System.Collections;
using System.Threading.RateLimiting;
using System;
using System.Net.Http.Json;
using RedditListener;

//Kick Off App
await MainApp();

static async Task MainApp()
{
    //Instantiate config object
    Configuration config = new Configuration();

    //Setup Auth Headers for HttpClient
    using HttpClient client = new();
    client.DefaultRequestHeaders.Accept.Clear();

    var base64String = Convert.ToBase64String(
       System.Text.Encoding.ASCII.GetBytes(config.authenticationString));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
    client.DefaultRequestHeaders.Add("User-Agent", config.useragent);

    //Get UNIX UTC Time at start of app
    DateTime startTime = DateTime.UtcNow;
    double startTimeUTC = ((DateTimeOffset)startTime).ToUnixTimeSeconds();

    //Get the Reddit Token
    string TokenJSON = await PostForRedditToken(client);
    RedditToken? Token = JsonSerializer.Deserialize<RedditToken>(TokenJSON);
    if(Token is not null)
        client.DefaultRequestHeaders.Add("access_token", Token.access_token);

    do
    {
        //Let's loop until Escape is hit
        while (!Console.KeyAvailable)
        {
            //Inform of exit button
            Console.WriteLine("Process Started: " + DateTimeOffset.FromUnixTimeSeconds((long)startTimeUTC));
            Console.WriteLine("Press ESC to stop");

            //Setup Parallel Async HTTP Requests
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 5
            };
            
            //1 thread per subreddit we will monitor
            int numberofthreads = config.SubRedditsToMonitor.Length;
            
            //Start the parallel threads
            await Parallel.ForEachAsync(config.SubRedditsToMonitor, parallelOptions, async (subreddit, ct) =>
            {
                //Instantiate Return Object
                RedditNewReturnObject ReturnedData = new RedditNewReturnObject();
                int threadsleep = 1000;
                try
                {
                    //Call reddit and get the new posts data
                    ReturnedData = await ProcessRedditNewAsync(client, startTimeUTC, subreddit);

                    //Figure out how long we should pause thread based on rate limits and number of threads that will also count against limits
                    threadsleep = ((ReturnedData.xratelimitreset / ReturnedData.xratelimitremaining) * 1000) * numberofthreads;

                    //Output the thread and rate limit data to console in a readable format
                    Console.WriteLine("");
                    Console.WriteLine("#######################");
                    Console.WriteLine("Subreddit: " + subreddit);
                    Console.WriteLine("-----------------------");
                    Console.WriteLine("Number of Threads: " + numberofthreads);
                    Console.WriteLine("Delay Between Requests: " + (threadsleep / 1000) + " seconds");
                    Console.WriteLine("Request Limit Used: " + ReturnedData.xratelimitused);
                    Console.WriteLine("Request Limit Remaining: " + ReturnedData.xratelimitremaining);
                    Console.WriteLine("Request Limit Reset: " + ReturnedData.xratelimitreset + " seconds");
                    Console.WriteLine("-----------------------");
                    Console.WriteLine("Top " + ReturnedData.NumberOfPostsToTrack + " Posts:");
                    Console.WriteLine("-----------------------");


                    //Output the Top Posts data to console in a readable format
                    if (ReturnedData is not null && ReturnedData.PostSummaries.Count > 0)
                    {
                        foreach (RedditPostSummary PostSummary in ReturnedData.PostSummaries)
                        {

                            Console.WriteLine("Title: " + PostSummary.title);
                            Console.WriteLine("Upvotes: " + PostSummary.upvotes);
                            Console.WriteLine("Author: " + PostSummary.author);
                            Console.WriteLine("Created UTC: " + DateTimeOffset.FromUnixTimeSeconds(PostSummary.createdutc));
                            Console.WriteLine("Post ID: " + PostSummary.id);
                            Console.WriteLine("Permalink: " + PostSummary.url);
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
                        foreach (KeyValuePair<string, int> entry in ReturnedData.AuthorCounts)
                        {
                            Console.WriteLine("Author: " + entry.Key);
                            Console.WriteLine("# Posts: " + entry.Value);
                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("NO AUTHORS YET, PLEASE STAND BY");
                    }
                    Console.WriteLine("-----------------------");

                    //Sleep thread for calculated time to maximize usage of rate limit spread across threads
                    Thread.Sleep(threadsleep);
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

static async Task<string> PostForRedditToken(HttpClient client)
{
    //Get a access_token from reddit. Might actually not be needed for this but could be useful later
    var values = new Dictionary<string, string>
      {
          { "grant_type", "https://oauth.reddit.com/grants/installed_client" },
          { "device_id", "b1589af8-9eb4-4922-8011-f697fc1b93a5" }
      };
    var content = new FormUrlEncodedContent(values);
    var response = await client.PostAsync("https://www.reddit.com/api/v1/access_token", content);
    var responseString = await response.Content.ReadAsStringAsync();
    return responseString;
}

static async Task<RedditNewReturnObject> ProcessRedditNewAsync(HttpClient client, double startTimeUTC, string subreddit)
{
    //Instantiate config object
    Configuration config = new Configuration();

    //GET the new.json for the selected subreddit.
    HttpResponseMessage response = await client.GetAsync(
        "https://www.reddit.com/r/" + subreddit + "/new.json?limit=100");

    response.EnsureSuccessStatusCode();

    //What are the rate limits currently at?
    int xratelimitused = Convert.ToInt32(Convert.ToDouble((response.Headers.GetValues("x-ratelimit-used").First())));
    int xratelimitremaining = Convert.ToInt32(Convert.ToDouble((response.Headers.GetValues("x-ratelimit-remaining").First())));
    int xratelimitreset = Convert.ToInt32(Convert.ToDouble((response.Headers.GetValues("x-ratelimit-reset").First())));

    //Pull out the response JSON
    string json = await response.Content.ReadAsStringAsync();

    //Deserialize JSON into Object to contain posts
    Root? NewPostsCollection = JsonSerializer.Deserialize<Root>(json);

    //New List that we will use to truncate any data we dont need for posts and sort
    List<RedditPostSummary> PostSummaries = new List<RedditPostSummary>();

    //Dictionary we will use to track # of posts by author
    Dictionary<string, int> AuthorCounts = new Dictionary<string, int>();

    //Loop through posts
    foreach (RedditPost Post in NewPostsCollection.data.children)
    {
        //If post was created after the app started, it is in play
        if (Post.data.created_utc is not null && Post.data.created_utc >= startTimeUTC)
        {
            RedditPostSummary PostSummary = new RedditPostSummary();
            PostSummary.title = Post.data.title;
            PostSummary.author = Post.data.author;
            PostSummary.upvotes = Post.data.ups;
            PostSummary.createdutc = (long)(Post.data.created_utc);
            PostSummary.id = Post.data.id;
            PostSummary.url = Post.data.permalink;

            PostSummaries.Add(PostSummary);

            if (AuthorCounts is not null && Post.data.author is not null)
            {
                if (AuthorCounts.ContainsKey(Post.data.author))
                    AuthorCounts[Post.data.author]++;
                else AuthorCounts.Add(Post.data.author, 1);
            }
        }
    }

    //Order post summaries descending, and take configured top number
    PostSummaries = PostSummaries.OrderByDescending(p => p.upvotes).Take(config.NumberOfPostsToTrack).ToList();

    //Order author counts descending, and take configured top number
    Dictionary<string, int> sortedAuthors = new Dictionary<string, int>();
    
    if(AuthorCounts is not null)
        sortedAuthors = AuthorCounts.OrderByDescending(pair => pair.Value).Take(config.NumberOfAuthorsToTrack)
               .ToDictionary(pair => pair.Key, pair => pair.Value);

    //Build our return object from this task and return it.
    RedditNewReturnObject DataToReturn = new RedditNewReturnObject();
    DataToReturn.NumberOfPostsToTrack = config.NumberOfPostsToTrack;
    DataToReturn.NumberOfAuthorsToTrack = config.NumberOfAuthorsToTrack;
    DataToReturn.PostSummaries = PostSummaries;
    DataToReturn.AuthorCounts = sortedAuthors;
    DataToReturn.xratelimitremaining = xratelimitremaining;
    DataToReturn.xratelimitreset = xratelimitreset;
    DataToReturn.xratelimitused = xratelimitused;
    return DataToReturn;
}