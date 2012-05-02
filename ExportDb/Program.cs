using System;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace ExportDb
{
    /// <summary>
    /// Where to get SMO DLLs:
    /// http://msdn.microsoft.com/en-us/library/ms162129%28v=SQL.105%29.aspx
    /// 
    /// How to use SMOs:
    /// http://msdn.microsoft.com/en-us/library/ms162202.aspx
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Arguments _arguments = new Arguments(args);
            try
            {
                if (_arguments["?"] != null)
                {
                    Program.PrintHelp();
                    return;
                }

                string _connectionString = "server=(local);database=Metadata;trusted_connection=yes";

                SqlConnection _connection = new SqlConnection(_connectionString);
                ServerConnection _serverConn = new ServerConnection(_connection);
                Server _server = new Server(_serverConn);

                Database _db = _server.Databases[_connection.Database];
                foreach (Table _table in _db.Tables)
                {
                    Console.WriteLine(_table.Name);
                }

                Console.ReadKey();
            }
            catch (Exception _e)
            {
                Console.WriteLine("Exception caught in Main()");
                Console.WriteLine("---------------------------------------");
                while (_e != null)
                {
                    Console.WriteLine(_e.Message);
                    Console.WriteLine();
                    Console.WriteLine(_e.GetType().FullName);
                    Console.WriteLine();
                    Console.WriteLine(_e.StackTrace);
                    Console.WriteLine("---------------------------------------");
                    _e = _e.InnerException;
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine(
@"ExportDb.exe usage:

");
        }
    }
}
