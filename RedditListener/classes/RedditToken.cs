using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RedditToken
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public string device_id { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }

    public RedditToken()
    {
        access_token = "";
        token_type = "";
        device_id = "";
        expires_in = 0;
        scope = "";
    }
}
