/*
 * SOURCE NOTICE
 * 
 * The code in this file was adopted from AppService Bridge Sample by Microsoft
 * (https://github.com/Microsoft/DesktopBridgeToUWP-Samples/tree/master/Samples/AppServiceBridgeSample)
 * licensed under MIT, see https://github.com/Microsoft/DesktopBridgeToUWP-Samples/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace ProcessMonitorUwp
{
    public class AppServiceBridgeManager
    {
        public bool BackgroundProcessLaunched
        {
            get;
            private set;
        }

        /// <summary>
        /// Launches the background process.
        /// Make sure the BackgroundProcess is in your AppX folder, if not rebuild the solution.
        /// </summary>
        public async Task LaunchBackgroundProcessAsync()
        {
            await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            BackgroundProcessLaunched = true;
        }

        /// <summary>
        /// Sends message to the full trust process and receives a response back.
        /// </summary>
        /// <param name="requestKey">The request key.</param>
        /// <param name="requestValue">The request value (message).</param>
        /// <returns>The response from the background process or null, if no response was received.</returns>
        public async Task<string> SendRequestToBackgroundProcessAsync(string requestKey, string requestValue)
        {
            ValueSet valueSet = new ValueSet();
            valueSet.Add(requestKey, requestValue);
            string responseValue = null;

            if (App.AppServiceConnection != null)
            {
                AppServiceResponse appServiceResponse = await App.AppServiceConnection.SendMessageAsync(valueSet);

                try
                {
                    responseValue = appServiceResponse.Message[requestKey] as string;
                }
                catch (KeyNotFoundException e)
                {
                    System.Diagnostics.Debug.WriteLine("SendRequestToBackgroundProcessAsync: Failed to get the response for request '" + requestKey + "': " + e.Message);
                }
            }

            return responseValue;
        }
    }
}
