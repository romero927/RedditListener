# RedditListener
Monitor Defined Subreddit(s) and output top post and top author data in near real-time. 
- Written in Visual Studio Community 2022.
- Targeting .NET 8 C#.

# Project Definition
Reddit, much like other social media platforms, provides a way for users to communicate their interests etc. For this exercise, we would like to see you build an application that listens to your choice of subreddits (best to choose one with a good amount of posts). You can use this LINK to help identify one that interests you.  We’d like to see the exercise completed in the language you are applying for, and you are free to use any 3rd party libraries you would like. Your app should consume the posts from your chosen subreddit in near real time and keep track of the following statistics between the time your application starts until it ends:
- Posts with most up votes
- Users with most posts
  
Your app should also provide some way to report these values to a user (periodically log to terminal, return from RESTful web service, etc.). If there are other interesting statistics you’d like to collect, that would be great. There is no need to store this data in a database; keeping everything in-memory is fine. That said, you should think about how you would persist data if that was a requirement. To acquire near real time statistics from Reddit, you will need to continuously request data from Reddit's rest APIs.  Reddit implements rate limiting and provides details regarding rate limit used, rate limit remaining, and rate limit reset period via response headers.  Your application should use these values to control throughput in an even and consistent manner while utilizing a high percentage of the available request rate. It’s very important that the various application processes do not block each other as Reddit can have a high volume on many of their subreddits.  The app should process posts as concurrently as possible to take advantage of available computing resources. While we are only asking to track a single subreddit, you should be thinking about his you could scale up your app to handle multiple subreddits. While designing and developing this application, you should keep SOLID principles in mind. Although this is a code challenge, we are looking for patterns that could scale and are loosely coupled to external systems / dependencies. In that same theme, there should be some level of error handling and unit testing. The submission should contain code that you would consider production ready. When you're finished, please put your project in a repository on either GitHub or Bitbucket and send us a link. Please be sure to provide guidance as to where the Reddit API Token values are located so that the team reviewing the code can replace/configure the value. After review, we may follow-up with an interview session with questions for you about your code and the choices made in design/implementation. While the coding exercise is intended to be an interesting and fun challenge, we are interested in seeing your best work - aspects that go beyond merely functional code, that demonstrate professionalism and pride in your work.  We look forward to your submission!

# Notes
- I added two operation modes, new and top. You can select this in the config file. I default it to top.
  - new = Look at all posts made from the point that the app was started and calculate the top NumberOfPostsToTrack and NumberOfAuthorsToTrack from those posts. This uses the new.json listing and will page backwards through the listing data slices until it finds the first post that was created after start.Time between requests will slow down over time as the number of pages you have to go through increases, as each page uses a request.
   - top = Look at the top posts listing and calculate the top NumberOfPostsToTrack and NumberOfAuthorsToTrack from those posts. Looks at current top 100. More memory and request stable.
 - The Listing endpoints don't actually require an access_token, but I still registered my app, get a token, and attach it so that reddit knows where the requests are coming from and so that we have the logic if we need to hit other endpoints in the future.

# General Logic Flow
- Start App
- Get Auth Token
- Capture Current Time as Start Time
- For Each Subreddit
  - Monitor the API threshold limits and limit API requests accordingly
  - Capture any posts created after Start Time ordered by upvotes and add to structure
  - After each capture, loop through posts and ID count post per user
  - Display current top posts and current user post counts

# Project Links
- Configuration: https://github.com/romero927/RedditListener/blob/main/RedditListener/Configuration.cs
- Program.cs: https://github.com/romero927/RedditListener/blob/main/RedditListener/Program.cs

# Reddit API Links
- https://github.com/reddit-archive/reddit/wiki/OAuth2
- https://support.reddithelp.com/hc/en-us/articles/16160319875092-Reddit-Data-API-Wiki
- https://www.reddit.com/dev/api/#GET_new

# Reddit API Limits
- X-Ratelimit-Used: Approximate number of requests used in this period
- X-Ratelimit-Remaining: Approximate number of requests left to use
- X-Ratelimit-Reset: Approximate number of seconds to end of period

# Future Concerns
- Due to the async multhreaded nature of the HTTPS requests, the console output can sometimes get out of order, especially on the first display. It resolves quickly and would not be an issue in a DB or File save.
- I built this for .NET 8, but couldn't find a way to add unit tests for this, I was maxed out at .NET 4.8 for the unit test project. Unit tests are the first thing I would go back and fix if able.
- I have run this for 30 mins and it was stable, but I haven't checked long term stability that would be needed for production
- If there is an error, the app will continue working with basic error handling. A real app would need much more in-depth checking.
- The only sorting criteria for Posts is # of Upvotes and Authors is # of Posts, so if there are multiple posts / authors with the same numbers, the display will be arbitrary. We would need to determine what additional filtering / sorting is desired.
- The Top Posts and Top Authors are calculated from the overall dataset and may not correlate after sorting / filtering. For instance, the Top Posting author may be spamming low upvoted posts and may not have anything in the Top posts section.
- If you start the app, let it run for a while, and restart it, the console may flash through the results quickly. This is due to it automatically trying to use the remaining requests before the reset time. It will go back to normal as soon as the request limit time resets.
