using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MessageServerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly AutoResetEvent _connectionWaitHandle = new AutoResetEvent(false);

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("MessageServerRole entry point called");
            TcpListener listener;

            try
            {
                var ipAddress = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["DefaultEndpoint"].IPEndpoint.Address;
                var port = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["DefaultEndpoint"].IPEndpoint.Port;
                listener = new TcpListener(ipAddress, port)
                {
                    ExclusiveAddressUse = false
                };
                listener.Start(); // start listening. 

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return;
            }
            while (true)
            {
                var result = listener.BeginAcceptTcpClient(HandleNewConnection, listener);
                _connectionWaitHandle.WaitOne();
                Trace.TraceInformation("Working");
            }
        }

        private void HandleNewConnection(IAsyncResult ar)
        {
            var listener = (TcpListener) ar.AsyncState;
            var client = listener.EndAcceptTcpClient(ar);
            _connectionWaitHandle.Set();
            var buffer = new byte[1024];
            var clientGuid = Guid.NewGuid();
            Trace.WriteLine(string.Format("Accepted connection with ID {0}", clientGuid));
            
            var stream = client.GetStream();

            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) {AutoFlush = true};


            var input = string.Empty;
            writer.WriteLine("Say something and I'll send it back to you, 9 to disconnect.");
            while (input != "9")
            {
                try
                {
                    input = reader.ReadLine();
                    writer.WriteLine("Received: {0}", input);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    throw;
                }
            }

            stream.Close();
            reader.Close();
            writer.Close();
            client.Close();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
