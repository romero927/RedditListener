using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using System.Text.Json;
using System.Collections;
using System.Threading.RateLimiting;
using System;
using System.Net.Http.Json;

namespace RedditListenerUnitTests
{
    [TestClass]
    public class TestRedditConnections
    {
        [TestMethod]
        public void TestTokenGet()
        {
            string TokenJSON = await RedditListener.PostForRedditToken(client);
            RedditToken Token = JsonSerializer.Deserialize<RedditToken>(TokenJSON);

        }
    }
}
