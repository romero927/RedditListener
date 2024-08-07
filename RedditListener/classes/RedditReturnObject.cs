using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener {
    internal class RedditReturnObject
    {
       public List<RedditPostSummary>? PostSummaries { get; set; }
       public Dictionary<string, int>? AuthorCounts { get; set; }
       public int NumberOfPostsToTrack {  get; set; }
       public int NumberOfAuthorsToTrack {  get; set; }
       public int xRateLimitUsed { get; set; }
       public int xRateLimitRemaining { get; set; }
       public int xRateLimitReset { get; set; }
    }
}
