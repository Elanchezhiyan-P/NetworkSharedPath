using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSharedPathAccess
{
    internal class Program
    {
        private readonly static string _networkPath = @"\\xxx\xx\xx";
        private readonly static string _userName = "xxxx";
        private readonly static string _password = "xxxx";

        static void Main(string[] args)
        {
            NetworkConnection _networkConnection = new NetworkConnection(_networkPath, _userName, _password, string.Empty);

            try
            {
                _networkConnection.CheckAccess();

                string[] files = Directory.GetFiles(_networkPath);

                foreach (var file in files)
                {
                    Console.WriteLine(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            _networkConnection.Dispose();

            Console.ReadLine();
        }
    }
}
