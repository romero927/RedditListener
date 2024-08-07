using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener
{
    internal class Configuration
    {
        //What is our Reddit App ID? Used to get Token
        public readonly string AuthenticationString = "<APP ID GOES HERE>";
        //What is my custom user agent?
        public readonly string UserAgent = "kgromerov0.0.1";
        //What subreddits do I want to monitor?
        public readonly string[] SubRedditsToMonitor =
                {
                "askreddit",
                "news",
                "gaming",
                "programming",
                "todayilearned"
            };
        //How many posts do I want to show for each subreddit? (Top #)
        public readonly int NumberOfPostsToTrack = 5;
        //How many Authors do I want to show for each subreddit? (Top #)
        public readonly int NumberOfAuthorsToTrack = 5;

        // new = Look at all posts made from the point that the app was started and calculate the top NumberOfPostsToTrack and NumberOfAuthorsToTrack from those posts.
        //      This uses the new.json listing and will page backwards through the listing data slices until it finds the first post that was created after start.
        //      Time between requests will slow down over time as the number of pages you have to go through increases, as each page uses a request.
        // top = Look at the top posts listing and calculate the top NumberOfPostsToTrack and NumberOfAuthorsToTrack from those posts. Looks at current top 100.
        public string Mode = "top";
        //How many parallel threads can run at once?
        public int MaxDegreeOfParallelism = 5;

    }
}
