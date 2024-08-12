namespace RedditListener
{
    public interface IRedditPost
    {
        Data? data { get; set; }
        string? kind { get; set; }
    }
}