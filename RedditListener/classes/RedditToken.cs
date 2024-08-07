using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditListener
{
    internal class RedditToken
    {
        public string Access_Token { get; set; }
        public string token_type { get; set; }
        public string device_id { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }

        public RedditToken()
        {
            Access_Token = "";
            token_type = "";
            device_id = "";
            expires_in = 0;
            scope = "";
        }
    }
}
