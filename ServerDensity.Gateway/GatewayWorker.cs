using System.IO;
using System.Net;
using System.Text;
using log4net;

namespace ServerDensity.Gateway
{
    /// <summary>
    /// Class to PUSH agent payload data to the Server Density servers.
    /// </summary>
    class GatewayWorker
    {
        /// <summary>
        /// Initializes a new instance of the GatewayWorker 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public GatewayWorker(HttpListenerContext context, GatewayConfigurationSection config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Push the POST request
        /// </summary>
        public void ProcessRequest()
        {
            try
            {
                InternalProcessRequest();
            }
            catch { }
        }

        private void InternalProcessRequest()
        {
            var request = _context.Request;
            var response = _context.Response;

            // Handles only queries from a ServerDensity Agent
            if (request.HttpMethod == HTTP_POST && request.RawUrl.StartsWith(SERVERDENSITY_POSTBACK))
            {
                // Push the query
                var url = _config.ServerDensityUrl + SERVERDENSITY_POSTBACK;
                var data = ReadStream(request.InputStream);
                var webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = HTTP_POST;
                webRequest.ContentType = CONTENT_TYPE_ENCODED;
                using (var webRequestStream = webRequest.GetRequestStream())
                {
                    webRequestStream.Write(data, 0, data.Length);
                }
                var responseText = string.Empty;
                using (var resp = webRequest.GetResponse())
                {
                    using (var s = resp.GetResponseStream())
                    {
                        responseText = Encoding.ASCII.GetString(ReadStream(s));
                    }
                }

                if (responseText != "OK")
                {
                    Log.ErrorFormat("URL {0} returned: {1}", url, responseText);
                }
                Log.Debug(responseText);

                // Transmit the response back to the agent
                var buffer = Encoding.ASCII.GetBytes(responseText);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
            }
        }

        private byte[] ReadStream(Stream stream)
        {
            var buffer = new byte[1024];
            var memoryStream = new MemoryStream();
            var nbBytesRead = 0;
            while ((nbBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, nbBytesRead);
            }
            return memoryStream.ToArray();
        }

        private HttpListenerContext _context;
        private readonly GatewayConfigurationSection _config;
        private readonly static ILog Log = LogManager.GetLogger(typeof(GatewayWorker));

        private static readonly string HTTP_POST = "POST";
        private static readonly string CONTENT_TYPE_ENCODED = "application/x-www-form-urlencoded";
        private static readonly string SERVERDENSITY_POSTBACK = "/postback/";
    }
}
