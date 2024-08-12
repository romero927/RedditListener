namespace RedditListener
{
    public interface IRoot
    {
        Data? data { get; set; }
        string? kind { get; set; }
    }
}