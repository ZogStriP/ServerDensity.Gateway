using System;
using System.Configuration;
using System.Net;
using System.Threading;
using log4net;

namespace ServerDensity.Gateway
{
    public class Gateway
    {
        /// <summary>
        /// Main method; used when running the gateway as a standalone console EXE.
        /// </summary>
        public static void Main()
        {
            // prout
            var config = (GatewayConfigurationSection)ConfigurationManager.GetSection("gateway");
            var gateway = new Gateway(config);

            gateway.Run();
            Console.ReadLine();
            gateway.Stop();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Gateway"/>.
        /// </summary>
        /// <param name="config"></param>
        public Gateway(GatewayConfigurationSection config)
        {
            _config = config;
        }

        /// <summary>
        /// Runs the gateway.
        /// </summary>
        public void Run()
        {
            _thread = new Thread(new ThreadStart(Start));
            _thread.Start();
            Log.Info("Gateway started.");
        }

        private void Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(_config.ListeningUrl);
                _listener.Start();
                while (!_isStopped)
                {
                    // Waits for the next request
                    var context = _listener.GetContext();
                    // Throw a new thread that will handle the request
                    new Thread(new GatewayWorker(context, _config).ProcessRequest).Start();
                }
            }
            catch { }
        }

        /// <summary>
        /// Stops the gateway.
        /// </summary>
        public void Stop()
        {
            _isStopped = true;
            try
            {
                _listener.Stop();
            }
            catch { }
            _thread.Join(0);
            try
            {
                _thread.Abort();
            }
            catch (ThreadAbortException) { }
            Log.InfoFormat("Gateway stopped.");
        }

        private bool _isStopped;
        private Thread _thread;
        private HttpListener _listener;
        private readonly GatewayConfigurationSection _config;
        private static ILog Log = LogManager.GetLogger(typeof(Gateway));
    }
}
