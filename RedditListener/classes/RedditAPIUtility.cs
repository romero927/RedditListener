using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Headers;

namespace RedditListener
{
    //All the logic for accessing Reddit API needed for the listener.
    public static class RedditAPIUtility
    {
        internal static async Task<HttpClient> SetupClient(Configuration Config, HttpClient Client)
        {
            Client = new();
            Client.DefaultRequestHeaders.Accept.Clear();

            //Convert our Auth settings into Base64 and add a user agent
            var Base64String = Convert.ToBase64String(
               System.Text.Encoding.ASCII.GetBytes(Config.AuthenticationString));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64String);
            Client.DefaultRequestHeaders.Add("User-Agent", Config.UserAgent);

            //Get the Reddit Token
            string TokenJSON = await RedditListener.RedditAPIUtility.PostForRedditToken(Client);
            RedditToken? Token = JsonSerializer.Deserialize<RedditToken>(TokenJSON);
            if (Token is not null && Token.Access_Token != string.Empty)
                Client.DefaultRequestHeaders.Add("access_token", Token.Access_Token);

            return Client;
        }
        internal static async Task<string> PostForRedditToken(HttpClient client)
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

        internal static async Task<RedditReturnObject> ProcessRedditPostsAsync(HttpClient Client, double StartTimeUTC, string Subreddit, Configuration Config)
        {
            //GET the new.json for the selected subreddit.
            HttpResponseMessage Response = await Client.GetAsync(
                "https://www.reddit.com/r/" + Subreddit + "/" + Config.Mode + ".json?limit=100");

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
                "https://www.reddit.com/r/" + Subreddit + "/" + Config.Mode + ".json?limit=100&after=" + PostsCollection.data.after);

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

            if (AuthorCounts is not null)
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

        public static int CalculateThreadSleep(int xRateLimitReset, int xRateLimitRemaining, int NumberOfThreads)
        {
            return ((xRateLimitReset / xRateLimitRemaining) * 1000) * NumberOfThreads;
        }
    }
}
