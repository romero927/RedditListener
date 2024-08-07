using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
namespace RedditListener
{
    internal class RedditPost
    {
        public string? kind { get; set; }
        public Data? data { get; set; }
    }

    internal class Data
    {
        public string? title { get; set; }
        public int? ups { get; set; }
        public string? id { get; set; }
        public string? author { get; set; }
        public string? permalink { get; set; }
        public double? created_utc { get; set; }
        public List<RedditPost>? children { get; set; }

        //Unused parameters commented out to save memory.
        //public string? after { get; set; }
        //public int? dist { get; set; }
        //public string? modhash { get; set; }
        //public string? geo_filter { get; set; }
        //public object? before { get; set; }
        //public object? approved_at_utc { get; set; }
        //public string? subreddit { get; set; }
        //public string? selftext { get; set; }
        //public string? author_fullname { get; set; }
        //public bool  saved { get; set; }
        //public object? mod_reason_title { get; set; }
        //public int? gilded { get; set; }
        //public bool? clicked { get; set; }
        //public List<object>? link_flair_richtext { get; set; }
        //public string? subreddit_name_prefixed { get; set; }
        //public bool? hidden { get; set; }
        //public int? pwls { get; set; }
        //public object? link_flair_css_class { get; set; }
        //public int? downs { get; set; }
        //public object? top_awarded_type { get; set; }
        //public bool? hide_score { get; set; }
        //public string? name { get; set; }
        //public bool? quarantine { get; set; }
        //public string? link_flair_text_color { get; set; }
        //public double? upvote_ratio { get; set; }
        //public object? author_flair_background_color { get; set; }
        //public string? subreddit_type { get; set; }
        //public int? total_awards_received { get; set; }
        //public MediaEmbed? media_embed { get; set; }
        //public object? author_flair_template_id { get; set; }
        //public bool? is_original_content { get; set; }
        //public List<object>? user_reports { get; set; }
        //public SecureMedia? secure_media { get; set; }
        //public bool? is_reddit_media_domain { get; set; }
        //public bool? is_meta { get; set; }
        //public object? category { get; set; }
        //public SecureMediaEmbed? secure_media_embed { get; set; }
        //public object? link_flair_text { get; set; }
        //public bool? can_mod_post { get; set; }
        //public int? score { get; set; }
        //public object? approved_by { get; set; }
        //public bool? is_created_from_ads_ui { get; set; }
        //public bool? author_premium { get; set; }
        //public string? thumbnail { get; set; }
        //public object? author_flair_css_class { get; set; }
        //public List<object>? author_flair_richtext { get; set; }
        //public Gildings? gildings { get; set; }
        //public object? content_categories { get; set; }
        //public bool? is_self { get; set; }
        //public object? mod_note { get; set; }
        //public double? created { get; set; }
        //public string? link_flair_type { get; set; }
        //public int? wls { get; set; }
        //public object? removed_by_category { get; set; }
        //public object? banned_by { get; set; }
        //public string? author_flair_type { get; set; }
        //public string? domain { get; set; }
        //public bool? allow_live_comments { get; set; }
        //public string? selftext_html { get; set; }
        //public object? likes { get; set; }
        //public object? suggested_sort { get; set; }
        //public object? banned_at_utc { get; set; }
        //public string? url_overridden_by_dest { get; set; }
        //public object? view_count { get; set; }
        //public bool? archived { get; set; }
        //public bool? no_follow { get; set; }
        //public bool? is_crosspostable { get; set; }
        //public bool? pinned { get; set; }
        //public bool? over_18 { get; set; }
        //public List<object>? all_awardings { get; set; }
        //public List<object>? awarders { get; set; }
        //public bool? media_only { get; set; }
        //public bool? can_gild { get; set; }
        //public bool? spoiler { get; set; }
        //public bool? locked { get; set; }
        //public object? author_flair_text { get; set; }
        //public List<object>? treatment_tags { get; set; }
        //public bool? visited { get; set; }
        //public object? removed_by { get; set; }
        //public object? num_reports { get; set; }
        //public object? distinguished { get; set; }
        //public string? subreddit_id { get; set; }
        //public bool? author_is_blocked { get; set; }
        //public object? mod_reason_by { get; set; }
        //public object? removal_reason { get; set; }
        //public string? link_flair_background_color { get; set; }
        //public bool? is_robot_indexable { get; set; }
        //public object? report_reasons { get; set; }
        //public object? discussion_type { get; set; }
        //public int? num_comments { get; set; }
        //public bool? send_replies { get; set; }
        //public string? whitelist_status { get; set; }
        //public bool? contest_mode { get; set; }
        //public List<object>? mod_reports { get; set; }
        //public bool? author_patreon_flair { get; set; }
        //public object? author_flair_text_color { get; set; }
        //public string? parent_whitelist_status { get; set; }
        //public bool? stickied { get; set; }
        //public string? url { get; set; }
        //public int? subreddit_subscribers { get; set; }
        //public int? num_crossposts { get; set; }
        //public Media? media { get; set; }
        //public bool? is_video { get; set; }
    }

    //public class Gildings
    //{
    //}

    //public class Media
    //{
    //    public string? type { get; set; }
    //    public Oembed? oembed { get; set; }
    //}

    //public class MediaEmbed
    //{
    //    public string? content { get; set; }
    //    public int? width { get; set; }
    //    public bool? scrolling { get; set; }
    //    public int? height { get; set; }
    //}

    //public class Oembed
    //{
    //    public string? provider_url { get; set; }
    //    public string? version { get; set; }
    //    public string? title { get; set; }
    //    public string? type { get; set; }
    //    public int? thumbnail_width { get; set; }
    //    public int? height { get; set; }
    //    public int? width { get; set; }
    //    public string? html { get; set; }
    //    public string? author_name { get; set; }
    //    public string? provider_name { get; set; }
    //    public string? thumbnail_url { get; set; }
    //    public int? thumbnail_height { get; set; }
    //    public string? author_url { get; set; }
    //}

    internal class Root
    {
        public string? kind { get; set; }
        public Data? data { get; set; }
    }

    //public class SecureMedia
    //{
    //    public string? type { get; set; }
    //    public Oembed? oembed { get; set; }
    //}

    //public class SecureMediaEmbed
    //{
    //    public string? content { get; set; }
    //    public int? width { get; set; }
    //    public bool? scrolling { get; set; }
    //    public string? media_domain_url { get; set; }
    //    public int? height { get; set; }
    //}

}
