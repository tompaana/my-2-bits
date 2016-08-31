using CommunicationProtocol;
using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace ProcessMonitorConsole
{
    public class AppServiceConnectionRequestHandler
    {
        /// <summary>
        /// Receives message from UWP app and sends a response back
        /// </summary>
        public void OnAppServiceConnectionRequestReceived(
            AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string requestKey = args.Request.Message.First().Key;
            string requestValue = args.Request.Message.First().Value.ToString();

            Logger.Log(string.Format("Received request '{0}' with value '{1}'",
                requestKey, requestValue), ConsoleColor.Magenta);

            ValueSet valueSet = new ValueSet();

            if (requestKey.Equals(Keys.KeyGeneralRequest))
            {
                // Just echo the request value
                valueSet.Add(requestKey, requestValue);
            }
            else if (requestKey.Equals(Keys.KeyProcessDetailsByWindowHandleRequest))
            {
                string responseValue =
                    ProcessUtils.ProcessProxyByMainWindowHandle(int.Parse(requestValue)).ToJson().ToString();
                valueSet.Add(requestKey, responseValue);
            }

            Logger.Log(string.Format("Responding to request '{0}' with value '{1}'",
                requestKey, valueSet.First().Value.ToString()), ConsoleColor.Magenta);

            args.Request.SendResponseAsync(valueSet).Completed += delegate { };
        }
    }
}
