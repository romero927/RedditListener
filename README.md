# RedditListener
Monitor Defined Subreddit(s) and output post data in near real-time.

# General Logic Flow
- Start App
- Get Auth Token
- Capture Current Time as Start Time
- For Each Subreddit
  -- Monitor the API threshold limits and limit API requests accordingly
  -- Capture any posts created after Start Time ordered by upvotes and add to structure
  -- After each capture, loop through posts and ID count post per user
  -- Display current top posts and current user post counts

# Reddit API Links
- https://github.com/reddit-archive/reddit/wiki/OAuth2
- https://support.reddithelp.com/hc/en-us/articles/16160319875092-Reddit-Data-API-Wiki
- https://www.reddit.com/dev/api/#GET_top

# Reddit API Limits
- X-Ratelimit-Used: Approximate number of requests used in this period
- X-Ratelimit-Remaining: Approximate number of requests left to use
- X-Ratelimit-Reset: Approximate number of seconds to end of period

# Known Issues
- Due to the async multhreaded nature of the HTTPS requests, the console output can sometimes get out of order. This would not be an issue in a DB or File save.
- I built this for .NET 8, but couldn't find a way to add unit tests for this, I was maxed out at .NET 4.8 for the unit test project. Unit tests are the first thing I would go back and fix if able.
- I have run this for 30 mins and it was stable, but I haven't checked long term stability that would be needed for production
- If there is an error, the app will continue working with basic error handling. A real app would need much more in-depth checking.
