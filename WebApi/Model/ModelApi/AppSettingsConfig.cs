using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Model.ModelApi
{
    public class AppSettingsConfig
    {
        public  class Rootobject
        {
            public Logging Logging { get; set; }
            public string AllowedHosts { get; set; }
            public Jwttokenmanagement JwtTokenManagement { get; set; }
        }

        public class Logging
        {
            public Loglevel LogLevel { get; set; }
        }

        public class Loglevel
        {
            public string Default { get; set; }
            public string Microsoft { get; set; }
            public string MicrosoftHostingLifetime { get; set; }
        }

        public class Jwttokenmanagement
        {
            public string secret { get; set; }
            public string issuer { get; set; }
            public string audience { get; set; }
        }
    }
}
