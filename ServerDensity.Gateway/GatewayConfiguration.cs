using System;
using System.IO;
using System.Xml;

namespace ServerDensity.Gateway
{
    /// <summary>
    /// Helper class for managing the configuration file for the gateway
    /// </summary>
    public class GatewayConfiguration
    {
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string ServerDensityUrl { get; set; }

        /// <summary>
        /// Gets or sets the Listening URL.
        /// </summary>
        public string ListeningUrl { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="GatewayConfiguration"/> class
        /// with the provided values.
        /// </summary>
        public GatewayConfiguration(string serverDensityUrl, string listeningUrl)
        {
            ServerDensityUrl = serverDensityUrl;
            ListeningUrl = listeningUrl;
        }

        /// <summary>
        /// Writes the configuration file.
        /// </summary>
        public void Write()
        {
            using (var writer = new StreamWriter(ConfigFile))
            {
                writer.Write(string.Format(DefaultConfig, ServerDensityUrl));
            }
        }

        /// <summary>
        /// Reads the configuration file.
        /// </summary>
        public static GatewayConfiguration Load()
        {
            var xml = new XmlDocument();

            if (File.Exists(ConfigFile))
            {
                try
                {
                    xml.Load(ConfigFile);
                    var node = xml.SelectSingleNode("//gateway");
                    return new GatewayConfiguration(
                        node.Attributes["serverDensityUrl"].InnerText,
                        node.Attributes["listeningUrl"].InnerText
                        );
                }
                catch { }
            }
            // Default
            return new GatewayConfiguration("http://example.serverdensity.com", "http://*:8080/");
        }

        private static readonly string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerDensity.Gateway.WindowsService.exe.config");
        private readonly string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <configSections>
    <section name=""gateway"" type=""ServerDensity.Gateway.GatewayConfigurationSection, ServerDensity.Gateway"" />
    <section name=""log4net"" type=""log4net.Config.Log4NetConfigurationSectionHandler, log4net"" />
  </configSections>
  <gateway url=""{0}"" listeningUrl=""{1}"" />
   <log4net>
    <appender name=""EventLog"" type=""log4net.Appender.EventLogAppender"" >
      <param name=""ApplicationName"" value=""Server Density Gateway"" />
      <layout type=""log4net.Layout.PatternLayout"">
        <param name=""ConversionPattern"" value=""%m"" />
      </layout>
    </appender>
    <root>
      <level value=""WARN"" />
      <appender-ref ref=""EventLog"" />
    </root>
  </log4net>
</configuration>";
    }
}
