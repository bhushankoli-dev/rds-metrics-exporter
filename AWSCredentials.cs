using System.Collections.Generic;
using System.Configuration;

namespace RDSMetrics
{
    public class AWSCredentials
    {
        public string KeyID { get; set; }
        public string SecretKey { get; set; }

        public AWSCredentials(string _key, string _secret)
        {
            KeyID = _key;
            SecretKey = _secret;
        }
    }
}
