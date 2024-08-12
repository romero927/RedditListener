
namespace RedditListener
{
    public interface IData
    {
        string? after { get; set; }
        string? author { get; set; }
        List<RedditPost>? children { get; set; }
        double? created_utc { get; set; }
        string? id { get; set; }
        string? permalink { get; set; }
        string? title { get; set; }
        int? ups { get; set; }
    }
}