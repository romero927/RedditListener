using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener
{
    internal class RedditPostSummary
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? author { get; set; }
        public int? upvotes { get; set; }
        public long createdutc { get; set; }
        public string? url { get; set; }
    }
}