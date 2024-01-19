using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static NetworkSharedPathAccess.NetworkConnection;

namespace NetworkSharedPathAccess
{
    public class NetworkConnection : IDisposable
    {
        private readonly string _networkPath;
        private readonly string _userName;
        private readonly NetworkCredential _networkCredential;
        private readonly NetResource _netResource;

        public NetworkConnection(string networkPath, string userName, string password, string Domain)
        {
            _networkPath = networkPath;

            _netResource = new NetResource
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = _networkPath
            };
            _networkCredential = new NetworkCredential(userName, password);

            _userName = string.IsNullOrEmpty(Domain) ? userName : $@"{Domain}\{userName}";
        }

        public string CheckAccess()
        {
            var result = WNetAddConnection2(_netResource, _networkCredential.Password, _userName, 0);
            Win32Exception ex = new Win32Exception(result);

            switch (result)
            {
                case 0:
                    Console.WriteLine("Connection successful!");
                    break;
                case 5:
                    throw new UnauthorizedAccessException(ex.Message);  // ERROR_ACCESS_DENIED
                case 85:
                    throw new InvalidOperationException(ex.Message); // ERROR_ALREADY_ASSIGNED
                case 86:
                    throw new ArgumentException(ex.Message); // ERROR_INVALID_PASSWORD
                case 1326:
                    throw new ArgumentException(ex.Message); // ERROR_LOGON_FAILURE
                case 53:
                    throw new ArgumentException(ex.Message); // ERROR_BAD_NETPATH
                case 1222:
                    throw new InvalidOperationException(ex.Message); // ERROR_NO_NETWORK
                case 1219:
                    throw new InvalidOperationException(ex.Message);  // ERROR_SESSION_CREDENTIAL_CONFLICT
                case 87:
                    throw new ArgumentException(ex.Message); // ERROR_INVALID_PARAMETER
                case 1208:
                    throw new InvalidOperationException(ex.Message); // ERROR_EXTENDED_ERROR
                default:
                    throw ex; // For other unhandled error codes, use Win32Exception
            }
            return _networkPath;
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkPath, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
    }
}