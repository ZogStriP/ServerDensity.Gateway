﻿using System.Configuration;

namespace ServerDensity.Gateway
{
    public class GatewayConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("serverDensityUrl", IsRequired = true)]
        public string ServerDensityUrl
        {
            get
            {
                // Remove leading "/"
                var url = (string)this["serverDensityUrl"];
                return url.EndsWith("/") ? url.Substring(0, url.Length - 1) : url;
            }
            set { this["serverDensityUrl"] = value; }
        }

        [ConfigurationProperty("listeningUrl", IsRequired = true)]
        public string ListeningUrl
        {
            get
            {
                // Add a "/" at the end
                var url = (string)this["listeningUrl"];
                return url.EndsWith("/") ? url : url + "/";
            }
            set { this["listeningUrl"] = value; }
        }
    }
}
