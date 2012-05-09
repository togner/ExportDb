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
                string _connectionString = "server=(local);database=Metadata;trusted_connection=yes";
                ScriptingOptions _options = new ScriptingOptions()
                {
                    AllowSystemObjects = false,
                    ScriptData = true,
                    ScriptSchema = false
                };
                string _outputDir = @"C:\data";
                Encoding _encoding = Encoding.Unicode;
                int _commandTerminatorInterval = 100;
                
                // connection
                SqlConnection _connection = new SqlConnection(_connectionString);
                ServerConnection _serverConn = new ServerConnection(_connection);
                Server _server = new Server(_serverConn);
                _server.SetDefaultInitFields(typeof(Table), "IsSystemObject");
                Database _db = _server.Databases[_connection.Database];

                Scripter _scripter = new Scripter(_server);
                _scripter.Options = _options;
                foreach (Table _table in _db.Tables)
                {
                    if (!_table.IsSystemObject)
                    {
                        // output file
                        string _regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                        Regex _regex = new Regex(string.Format("[{0}]", Regex.Escape(_regexSearch)));
                        string _escapedName = _regex.Replace(_table.Name, ".");
                        string _fileName = Path.Combine(_outputDir, string.Format("{0}.{1}.Table.sql", _table.Schema, _escapedName));
                        string _directoryName = Path.GetDirectoryName(_fileName);
                        if (!Directory.Exists(_directoryName))
                        {
                            Directory.CreateDirectory(_directoryName);
                        }

                        // alternatively
                        // _scripter.Options.ToFileOnly = true;
                        // _scripter.Options.FileName = _fileName;
                        // _options.NoCommandTerminator = true;
                        // _scripter.EnumScript(new Urn[] { _table.Urn });
                        using (StreamWriter _writer = new StreamWriter(_fileName, false, _encoding))
                        {
                            int _row = 0;
                            foreach (string _scriptLine in _scripter.EnumScript(new Urn[] { _table.Urn }))
                            {
                                if (_row > 0 && _row % _commandTerminatorInterval == 0)
                                {
                                    _writer.WriteLine("GO");
                                }
                                _writer.WriteLine(_scriptLine);
                                _row++;
                            }
                        }
                    }
                }

                //Console.ReadKey();
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
