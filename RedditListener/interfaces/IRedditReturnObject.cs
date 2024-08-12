
namespace RedditListener
{
    public interface IRedditReturnObject
    {
        Dictionary<string, int>? AuthorCounts { get; set; }
        int NumberOfAuthorsToTrack { get; set; }
        int NumberOfPostsToTrack { get; set; }
        List<RedditPostSummary>? PostSummaries { get; set; }
        int xRateLimitRemaining { get; set; }
        int xRateLimitReset { get; set; }
        int xRateLimitUsed { get; set; }
    }
}