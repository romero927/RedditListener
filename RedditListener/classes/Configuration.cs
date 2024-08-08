using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedditListener
{
    internal class Configuration
    {
        //Constructor: Load values from config.json. Fallback to sane defaults if there is an error.
        public Configuration()
        {
            try
            {
                //Load our JSON config file and deserialize into an object
                JSONDeserializedConfig ConfigFile = JsonFileReader.Read<JSONDeserializedConfig>(@"./config.json");
                //Construct our Config object from the deserialized settings
                AuthenticationString = ConfigFile.authenticationString;
                UserAgent = ConfigFile.userAgent;
                SubRedditsToMonitor = ConfigFile.subRedditsToMonitor;
                NumberOfPostsToTrack = ConfigFile.numberOfPostsToTrack;
                NumberOfAuthorsToTrack = ConfigFile.numberOfAuthorsToTrack;
                Mode = ConfigFile.mode;
                MaxDegreeOfParallelism = ConfigFile.maxDegreeOfParallelism;
            }
            catch (Exception ex)
            {
                //Alert user to config.json file problem
                Console.WriteLine("ERROR LOADING CONFIG FILE, FALLING BACK TO DEFAULTS");
                //Fallback to Sane Defaults
                AuthenticationString = "<APP ID GOES HERE>";
                UserAgent = "kgromerov0.0.1";
                SubRedditsToMonitor = new string[]
                {
                    "askreddit"
                    //"news",
                    //"gaming",
                    //"programming",
                    //"todayilearned"
                };
                NumberOfPostsToTrack = 5;
                NumberOfAuthorsToTrack = 5;
                Mode = "top";
                MaxDegreeOfParallelism = 5;
            }

        }

        //Private Data Model to Deserialize our Config File into during construciton
        private class JSONDeserializedConfig
        {
            public string authenticationString { get; set; }
            public string userAgent { get; set; }
            public string[] subRedditsToMonitor { get; set; }
            public int numberOfPostsToTrack { get; set; }
            public int numberOfAuthorsToTrack { get; set; }
            public string mode { get; set; }
            public int maxDegreeOfParallelism { get; set; }
        }

        //Logic to read Our JSON Config File
        private static class JsonFileReader
        {
            public static T Read<T>(string filePath)
            {
                string text = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(text);
            }
        }

        //-----------------------
        //PARAMETERS BELOW
        //-----------------------
        //What is our Reddit App ID? Used to get Token
        public string AuthenticationString { get; }

        //What is my custom user agent?
        public string UserAgent { get; }

        //What subreddits do I want to monitor?
        public string[] SubRedditsToMonitor { get; }
        //How many posts do I want to show for each subreddit? (Top #)
        public int NumberOfPostsToTrack { get; }
        //How many Authors do I want to show for each subreddit? (Top #)
        public int NumberOfAuthorsToTrack { get; }

        // new = Look at all posts made from the point that the app was started and calculate the top NumberOfPostsToTrack and NumberOfAuthorsToTrack from those posts.
        //      This uses the new.json listing and will page backwards through the listing data slices until it finds the first post that was created after start.
        //      Time between requests will slow down over time as the number of pages you have to go through increases, as each page uses a request.
        // top = Look at the top posts listing and calculate the top NumberOfPostsToTrack and NumberOfAuthorsToTrack from those posts. Looks at current top 100.
        public string Mode { get; }
        //How many parallel threads can run at once?
        public int MaxDegreeOfParallelism { get; }

    }

}

