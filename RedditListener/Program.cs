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

    //Setup Auth Headers for HttpClient
    using HttpClient Client = new();
    Client.DefaultRequestHeaders.Accept.Clear();
    
    //Convert our Auth settings into Base64 and add a user agent
    var Base64String = Convert.ToBase64String(
       System.Text.Encoding.ASCII.GetBytes(Config.AuthenticationString));
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64String);
    Client.DefaultRequestHeaders.Add("User-Agent", Config.UserAgent);

    //Get UNIX UTC Time at start of app
    DateTime StartTime = DateTime.UtcNow;
    double StartTimeUTC = ((DateTimeOffset)StartTime).ToUnixTimeSeconds();

    //Get the Reddit Token
    string TokenJSON = await PostForRedditToken(Client);
    RedditToken? Token = JsonSerializer.Deserialize<RedditToken>(TokenJSON);
    if(Token is not null)
        Client.DefaultRequestHeaders.Add("access_token", Token.Access_Token);

    do
    {
        //Let's loop until Escape is hit
        while (!Console.KeyAvailable)
        {
            //Inform of exit button
            Console.WriteLine("Process Started: " + DateTimeOffset.FromUnixTimeSeconds((long)StartTimeUTC));
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
                    ReturnedData = await ProcessRedditPostsAsync(Client, StartTimeUTC, Subreddit, Config);

                    //Figure out how long we should pause thread based on rate limits and number of threads that will also count against limits
                    ThreadSeep = ((ReturnedData.xRateLimitReset / ReturnedData.xRateLimitRemaining) * 1000) * NumberOfThreads;

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

static async Task<string> PostForRedditToken(HttpClient client)
{
    //Get a access_token from reddit. Might actually not be needed for this but could be useful later
    var Values = new Dictionary<string, string>
      {
          { "grant_type", "https://oauth.reddit.com/grants/installed_client" },
          { "device_id", "b1589af8-9eb4-4922-8011-f697fc1b93a5" }
      };
    var Content = new FormUrlEncodedContent(Values);
    var Response = await client.PostAsync("https://www.reddit.com/api/v1/access_token", Content);
    var ResponseString = await Response.Content.ReadAsStringAsync();
    return ResponseString;
}

static async Task<RedditReturnObject> ProcessRedditPostsAsync(HttpClient Client, double StartTimeUTC, string Subreddit, Configuration Config)
{
    //GET the new.json for the selected subreddit.
    HttpResponseMessage Response = await Client.GetAsync(
        "https://www.reddit.com/r/" + Subreddit + "/"+Config.Mode+".json?limit=100");

    Response.EnsureSuccessStatusCode();

    //What are the rate limits currently at?
    int xRateLimitUsed = Convert.ToInt32(Convert.ToDouble((Response.Headers.GetValues("x-ratelimit-used").First())));
    int xRateLimitRemaining = Convert.ToInt32(Convert.ToDouble((Response.Headers.GetValues("x-ratelimit-remaining").First())));
    int xRateLimitReset = Convert.ToInt32(Convert.ToDouble((Response.Headers.GetValues("x-ratelimit-reset").First())));

    //Pull out the response JSON
    string JSON = await Response.Content.ReadAsStringAsync();

    //Deserialize JSON into Object to contain posts
    Root? PostsCollection = JsonSerializer.Deserialize<Root>(JSON);

    //New List that we will use to truncate any data we dont need for posts and sort
    List<RedditPostSummary> PostSummaries = new List<RedditPostSummary>();

    //Dictionary we will use to track # of posts by author
    Dictionary<string, int> AuthorCounts = new Dictionary<string, int>();

    //ID if we need to go to next page
    bool EndPaging = false;

    //Loop through posts
    foreach (RedditPost Post in PostsCollection.data.children)
    {
        if (Config.Mode == "new")
        {
            //If post was created after the app started, it is in play
            if (Post.data.created_utc is not null && Post.data.created_utc >= StartTimeUTC)
            {
                RedditPostSummary PostSummary = new RedditPostSummary();
                PostSummary.Title = Post.data.title;
                PostSummary.Author = Post.data.author;
                PostSummary.Upvotes = Post.data.ups;
                PostSummary.CreatedUTC = (long)(Post.data.created_utc);
                PostSummary.ID = Post.data.id;
                PostSummary.PermaLink = Post.data.permalink;

                PostSummaries.Add(PostSummary);

                if (AuthorCounts is not null && Post.data.author is not null)
                {
                    if (AuthorCounts.ContainsKey(Post.data.author))
                        AuthorCounts[Post.data.author]++;
                    else AuthorCounts.Add(Post.data.author, 1);
                }
            }
            else if (Post.data.created_utc is not null && Post.data.created_utc < StartTimeUTC)
            {
                //Current Post happened before app start, this will be our end condition
                EndPaging = true;
            }
        }
        else
        {
            //If post was created after the app started, it is in play
            if (Post.data.created_utc is not null)
            {
                RedditPostSummary PostSummary = new RedditPostSummary();
                PostSummary.Title = Post.data.title;
                PostSummary.Author = Post.data.author;
                PostSummary.Upvotes = Post.data.ups;
                PostSummary.CreatedUTC = (long)(Post.data.created_utc);
                PostSummary.ID = Post.data.id;
                PostSummary.PermaLink = Post.data.permalink;

                PostSummaries.Add(PostSummary);

                if (AuthorCounts is not null && Post.data.author is not null)
                {
                    if (AuthorCounts.ContainsKey(Post.data.author))
                        AuthorCounts[Post.data.author]++;
                    else AuthorCounts.Add(Post.data.author, 1);
                }
            }
        }
    }

    //If we are in new mode and didnt find end condition, we need to loop back to next page of data slice
    while (Config.Mode == "new" && !EndPaging)
    {
        //Get the next slice using after
        Response = await Client.GetAsync(
        "https://www.reddit.com/r/" + Subreddit + "/"+Config.Mode+".json?limit=100&after="+PostsCollection.data.after);

        Response.EnsureSuccessStatusCode();

        //What are the rate limits currently at?
        xRateLimitUsed = Convert.ToInt32(Convert.ToDouble((Response.Headers.GetValues("x-ratelimit-used").First())));
        xRateLimitRemaining = Convert.ToInt32(Convert.ToDouble((Response.Headers.GetValues("x-ratelimit-remaining").First())));
        xRateLimitReset = Convert.ToInt32(Convert.ToDouble((Response.Headers.GetValues("x-ratelimit-reset").First())));

        //Pull out the response JSON
        JSON = await Response.Content.ReadAsStringAsync();

        //Deserialize JSON into Object to contain posts
        PostsCollection = JsonSerializer.Deserialize<Root>(JSON);

        //Loop through posts
        foreach (RedditPost Post in PostsCollection.data.children)
        {
            //If post was created after the app started, it is in play
            if (Post.data.created_utc is not null && Post.data.created_utc >= StartTimeUTC)
            {
                RedditPostSummary PostSummary = new RedditPostSummary();
                PostSummary.Title = Post.data.title;
                PostSummary.Author = Post.data.author;
                PostSummary.Upvotes = Post.data.ups;
                PostSummary.CreatedUTC = (long)(Post.data.created_utc);
                PostSummary.ID = Post.data.id;
                PostSummary.PermaLink = Post.data.permalink;

                PostSummaries.Add(PostSummary);

                if (AuthorCounts is not null && Post.data.author is not null)
                {
                    if (AuthorCounts.ContainsKey(Post.data.author))
                        AuthorCounts[Post.data.author]++;
                    else AuthorCounts.Add(Post.data.author, 1);
                }
            }
            else if (Post.data.created_utc is not null && Post.data.created_utc < StartTimeUTC)
            {
                //Found our end condition
                EndPaging = true;
            }
        }
    }

    //Order post summaries descending, and take configured top number
    PostSummaries = PostSummaries.OrderByDescending(p => p.Upvotes).Take(Config.NumberOfPostsToTrack).ToList();

    //Order author counts descending, and take configured top number
    Dictionary<string, int> SortedAuthors = new Dictionary<string, int>();
    
    if(AuthorCounts is not null)
        SortedAuthors = AuthorCounts.OrderByDescending(pair => pair.Value).Take(Config.NumberOfAuthorsToTrack)
               .ToDictionary(pair => pair.Key, pair => pair.Value);

    //Build our return object from this task and return it.
    RedditReturnObject DataToReturn = new RedditReturnObject();
    DataToReturn.NumberOfPostsToTrack = Config.NumberOfPostsToTrack;
    DataToReturn.NumberOfAuthorsToTrack = Config.NumberOfAuthorsToTrack;
    DataToReturn.PostSummaries = PostSummaries;
    DataToReturn.AuthorCounts = SortedAuthors;
    DataToReturn.xRateLimitRemaining = xRateLimitRemaining;
    DataToReturn.xRateLimitReset = xRateLimitReset;
    DataToReturn.xRateLimitUsed = xRateLimitUsed;
    return DataToReturn;
}
