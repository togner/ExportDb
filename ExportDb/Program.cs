using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
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
                // params
                if (_arguments["?"] != null)
                {
                    Program.PrintHelp();
                    return;
                }
                string _connectionString = _arguments["con"];
                string _outputDirectory = _arguments["out"];
                bool _verbose = _arguments["v"] != null;
                if (_connectionString == null || _outputDirectory == null)
                {
                    Program.PrintHelp();
                    return;
                }

                if (!Directory.Exists(_outputDirectory))
                {
                    Directory.CreateDirectory(_outputDirectory);
                }

                Exporter _worker = new Exporter();
                _worker.ExportData(_connectionString, _outputDirectory, _verbose);

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
@"Usage:
    ExportDb.exe
        -con:<connection string> 
        -out:<output directory> 
        [-v] 
");
        }
    }
}
