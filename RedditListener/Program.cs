
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
    RedditToken Token = JsonSerializer.Deserialize<RedditToken>(TokenJSON);
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
                    if (ReturnedData.PostSummaries.Count > 0)
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
            Thread.Sleep(500);
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
        "https://www.reddit.com/r/" + subreddit + "/new.json");

    response.EnsureSuccessStatusCode();

    //What are the rate limits currently at?
    int xratelimitused = Convert.ToInt32(Convert.ToDouble((response.Headers.GetValues("x-ratelimit-used").First())));
    int xratelimitremaining = Convert.ToInt32(Convert.ToDouble((response.Headers.GetValues("x-ratelimit-remaining").First())));
    int xratelimitreset = Convert.ToInt32(Convert.ToDouble((response.Headers.GetValues("x-ratelimit-reset").First())));

    //Pull out the response JSON
    string json = await response.Content.ReadAsStringAsync();

    //Deserialize JSON into Object to contain posts
    Root NewPostsCollection = JsonSerializer.Deserialize<Root>(json);

    //New List that we will use to truncate any data we dont need for posts and sort
    List<RedditPostSummary> PostSummaries = new List<RedditPostSummary>();

    //Dictionary we will use to track # of posts by author
    Dictionary<string, int> AuthorCounts = new Dictionary<string, int>();

    //Loop through posts
    foreach (RedditPost Post in NewPostsCollection.data.children)
    {
        //If post was created after the app started, it is in play
        if (Post.data.created_utc >= startTimeUTC)
        {
            RedditPostSummary PostSummary = new RedditPostSummary();
            PostSummary.title = Post.data.title;
            PostSummary.author = Post.data.author;
            PostSummary.upvotes = Post.data.ups;
            PostSummary.createdutc = (long)(Post.data.created_utc);
            PostSummary.id = Post.data.id;
            PostSummary.url = Post.data.permalink;

            PostSummaries.Add(PostSummary);
            if (AuthorCounts.ContainsKey(Post.data.author))
                AuthorCounts[Post.data.author]++;
            else AuthorCounts.Add(Post.data.author, 1);
        }
    }
    //Order post summaries descending, and take configured top number
    PostSummaries = PostSummaries.OrderByDescending(p => p.upvotes).Take(config.NumberOfPostsToTrack).ToList();

    //Order author counts descending, and take configured top number
    Dictionary<string, int> sortedAuthors = AuthorCounts.OrderByDescending(pair => pair.Value).Take(config.NumberOfAuthorsToTrack)
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

[TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Instantiate Return Object
            RedditNewReturnObject ReturnedData = new RedditNewReturnObject();

            ReturnedData = await ProcessRedditNewAsync(client, startTimeUTC, subreddit);

        }
    }


//https://github.com/reddit-archive/reddit/wiki/OAuth2
//https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient

//Start App
//Pull config values from file
//Get Auth Token
//Capture Current Time as Start Time
//Capture any posts created after Start Time ordered by upvotes and add to structure
//After each capture, loop through posts and ID count post per user
//Display current top posts and current user post counts

//https://support.reddithelp.com/hc/en-us/articles/16160319875092-Reddit-Data-API-Wiki
//https://www.reddit.com/dev/api/#GET_top


//Rules
//You can use the Reddit Data API, subject to our Developer Terms and Data API Terms. 
//To request commercial access, research approval, or to reach out to the team, please contact us here. Please excuse delays due to a high volume of requests.
//Clients must authenticate with a registered OAuth token. We can and will freely throttle or block unidentified Data API users. 
//You must use a User-Agent where possible. Change your client's User-Agent string to something unique and descriptive, including the target platform, a unique application identifier, a version string, and your username as contact information, in the following format:
//<platform>:< app ID >:< version string> (by /u/<reddit username>)
//Example:
//User - Agent: android: com.example.myredditapp:v1.2.3 (by /u/kemitche)
//Many default User-Agents (like "Python/urllib" or "Java") are drastically limited to encourage unique and descriptive user-agent strings.
//Including the version number and updating it as you build your application allows us to safely block old buggy/broken versions of your app.
//NEVER lie about your User-Agent. 
//Our robots.txt is for search engines, not Data API users. 
//You must remove any user content in your possession that has been deleted from Reddit. 
//When posts and comments are deleted, you must delete all content related to the post and/or comment (e.g., title, body, embedded URLs, etc.).
//When a user account is deleted, you must delete all related user ID info (e.g., t2_*). You must also delete all references to the author-identifying information (i.e., the author ID, name, profile URL, avatar image URL, user flair, etc.) from posts and comments created by that account. 
//To best comply with this policy, we strongly recommend routinely deleting any stored user data and content within 48 hours.
//Note that retention of content and data that has been deleted–-even if disassociated, de-identified or anonymized–-is a violation of our terms and policies.

//Rate Limits
//Monitor the following response headers to ensure that you're not exceeding the limits:

//X-Ratelimit-Used: Approximate number of requests used in this period
//X-Ratelimit-Remaining: Approximate number of requests left to use
//X-Ratelimit-Reset: Approximate number of seconds to end of period
//We enforce rate limits for those eligible for free access usage of our Data API. The limit is:   

//100 queries per minute (QPM) per OAuth client id
//QPM limits will be an average over a time window (currently 10 minutes) to support bursting requests.

//Traffic not using OAuth or login credentials will be blocked, and the default rate limit will not apply.

//Important note: Historically, our rate limit response headers indicated counts by client id/user id combination. These headers will update to reflect this new policy based on client id only.