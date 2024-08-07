﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener
{
    internal class Configuration
    {
        //What is our Reddit App ID? Used to get Token
        public readonly string authenticationString = "<APP ID GOES HERE>";
        //What is my custom user agent?
        public readonly string useragent = "kgromerov0.0.1";
        //What subreddits do I want to monitor?
        public readonly string[] SubRedditsToMonitor =
                {
                "askreddit",
                "gaming"
                //"funny",
                //"worldnews",
                //"todayilearned"
            };
        //How many posts do I want to show for each subreddit? (Top #)
        public readonly int NumberOfPostsToTrack = 5;
        //How many Authors do I want to show for each subreddit? (Top #)
        public readonly int NumberOfAuthorsToTrack = 5;
    }
}
