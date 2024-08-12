using System.Text.Json;
using Xunit;

namespace RedditListener.tests
{
    public class RedditListenerTests
    {
        //Make sure Config File can load 
        [Fact]
        public void CanLoadConfigFile()
        {
            //Instantiate config object
            Configuration Config = new Configuration();
            Assert.Equal("config.json", Config.SetFrom);
        }

        //Make sure HTTP client can get setup ok
        [Fact]
        public async Task CanSetupClient()
        {
            //Instantiate config object
            Configuration Config = new Configuration();

            //Setup HTTP Client
            HttpClient Client = new HttpClient();
            Client = await RedditAPIUtility.SetupClient(Config, Client);
            Assert.NotNull(Client.DefaultRequestHeaders.Authorization);
        }

        //Make sure we can get a Reddit Token. Disabled for now as Token is not needed.
        //[Fact]
        //public async Task CanGetAccessToken()
        //{
        //    //Instantiate config object
        //    Configuration Config = new Configuration();

        //    //Setup HTTP Client
        //    HttpClient Client = new HttpClient();
        //    Client = await RedditAPIUtility.SetupClient(Config, Client);

        //    //Get the Reddit Token
        //    string TokenJSON = await RedditAPIUtility.PostForRedditToken(Client);
        //    RedditToken? Token = JsonSerializer.Deserialize<RedditToken>(TokenJSON);
        //    // Assert
        //    Assert.NotEqual(string.Empty, Token.Access_Token);
        //}

    }
}
