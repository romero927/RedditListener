namespace RedditListener
{
    public interface IRedditPostSummary
    {
        string? Author { get; set; }
        long CreatedUTC { get; set; }
        string? ID { get; set; }
        string? PermaLink { get; set; }
        string? Title { get; set; }
        int? Upvotes { get; set; }
    }
}