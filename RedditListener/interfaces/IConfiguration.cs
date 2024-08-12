using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener.interfaces
{
    //Configuration Interface
    public interface IConfiguration
    {
        public string AuthenticationString { get; }
        public string UserAgent { get; }
        public string[] SubRedditsToMonitor { get; }
        public int NumberOfPostsToTrack { get; }
        public int NumberOfAuthorsToTrack { get; }
        public string Mode { get; }
        public int MaxDegreeOfParallelism { get; }
        public string SetFrom { get; }
    }
}
