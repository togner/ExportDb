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
                int _goInterval;
                if (!int.TryParse(_arguments["go"], out _goInterval))
                {
                    _goInterval = 100;
                }
                bool _printProcessed;
                if (!bool.TryParse(_arguments["printProcessed"], out _printProcessed))
                {
                    _printProcessed = true;
                }
                bool _offByOne = _arguments["offByOne"] != null;

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
                if (_goInterval > 0)
                {
                    _worker.CommandTerminatorInterval = _goInterval;
                }
                _worker.PrintProcessedRecords = _printProcessed;
                _worker.OffByOne = _offByOne;
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
        [-v]                        Verbose (false)
        [-go:<number>]              GO interval (100)
        [-printProcessed:<bool>]    Print 'Processed xx total records' (true)
");
        }
    }
}
