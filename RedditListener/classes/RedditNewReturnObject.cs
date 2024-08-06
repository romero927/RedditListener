using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class RedditNewReturnObject
{
   public List<RedditPostSummary>? PostSummaries { get; set; }
   public Dictionary<string, int>? AuthorCounts { get; set; }
   public int NumberOfPostsToTrack {  get; set; }
   public int NumberOfAuthorsToTrack {  get; set; }
   public int xratelimitused { get; set; }
   public int xratelimitremaining { get; set; }
   public int xratelimitreset { get; set; }
}
