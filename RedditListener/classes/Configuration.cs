using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedditListener
{

    //Configuration Class, Implements IConfiguration Interface
    internal class Configuration : RedditListener.interfaces.IConfiguration
    {
        //Constructor: Load values from config.json. Fallback to sane defaults if there is an error.
        public Configuration()
        {
            try
            {
                //Load our JSON config file and deserialize into an object
                JSONDeserializedConfig ConfigFile = JsonFileReader.Read<JSONDeserializedConfig>(@"./config.json");
                //Construct our Config object from the deserialized settings
                AuthenticationString = ConfigFile.AuthenticationString;
                UserAgent = ConfigFile.UserAgent;
                SubRedditsToMonitor = ConfigFile.SubRedditsToMonitor;
                NumberOfPostsToTrack = ConfigFile.NumberOfPostsToTrack;
                NumberOfAuthorsToTrack = ConfigFile.NumberOfAuthorsToTrack;
                Mode = ConfigFile.Mode;
                MaxDegreeOfParallelism = ConfigFile.MaxDegreeOfParallelism;
                SetFrom = "config.json";
            }
            catch (Exception ex)
            {
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
                SetFrom = "default";
            }

        }

        //Private Data Model to Deserialize our Config File into during construciton. Implements IConfiguration
        private struct JSONDeserializedConfig : RedditListener.interfaces.IConfiguration
        {
            public string AuthenticationString { get; set; }
            public string UserAgent { get; set; }
            public string[] SubRedditsToMonitor { get; set; }
            public int NumberOfPostsToTrack { get; set; }
            public int NumberOfAuthorsToTrack { get; set; }
            public string Mode { get; set; }
            public int MaxDegreeOfParallelism { get; set; }
            public string SetFrom { get; set; }
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
        //Config file or Default? Only uses default if there is a problem using the file.
        public string SetFrom { get;}

    }

}

