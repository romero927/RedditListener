using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener
{
    public class RedditPostSummary : IRedditPostSummary
    {
        public string? ID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public int? Upvotes { get; set; }
        public long CreatedUTC { get; set; }
        public string? PermaLink { get; set; }
    }
}