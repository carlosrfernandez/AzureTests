using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
                listener = new TcpListener(RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["DefaultEndpoint"].IPEndpoint.Address, 
                    RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["DefaultEndpoint"].IPEndpoint.Port)
                {
                    ExclusiveAddressUse = false
                };
                listener.Start();

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

            var clientGuid = Guid.NewGuid();
            Trace.WriteLine(string.Format("Accepted connection with ID {0}", clientGuid));

            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            var input = string.Empty;
            while (input != "9")
            {
                Thread.Sleep(10);
                writer.WriteLineAsync("Say something... 9 to stop");
                var readAsyncResult = reader.ReadLineAsync();
                input = readAsyncResult.Result;
                writer.WriteLineAsync(string.Format("Received: {0}", input));
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
