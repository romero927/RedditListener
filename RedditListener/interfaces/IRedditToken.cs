namespace RedditListener
{
    public interface IRedditToken
    {
        string Access_Token { get; set; }
        string device_id { get; set; }
        int expires_in { get; set; }
        string scope { get; set; }
        string token_type { get; set; }
    }
}