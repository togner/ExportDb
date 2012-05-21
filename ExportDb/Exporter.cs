using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace ExportDb
{
    public class Exporter
    {
        public int CommandTerminatorInterval { get; set; }
        public Encoding Encoding { get; set; }
        public bool PrintProcessedRecords { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether when command terminator
        /// interval is set to 101 print processed should display 100
        /// (...202 rows processed, but we indicate to user only 200, 303 rows 
        /// processed, we indicate 300, etc.)
        /// Replicating buggy behavior in SSMS 10.50.
        /// </summary>
        public bool OffByOne { get; set; }

        private static readonly string CommandTerminator = "GO";
        private static readonly string PrintProcessedRecordsFormat = "print 'Processed {0} total records'";

        public Exporter()
        {
            this.Encoding = Encoding.Unicode;
            this.CommandTerminatorInterval = 100;
            this.PrintProcessedRecords = true;
        }

        public void ExportData(string connectionString, string outputDirectory, bool verbose)
        {
            ScriptingOptions _options = new ScriptingOptions()
            {
                AllowSystemObjects = false,
                ScriptData = true,
                ScriptSchema = false
            };

            // connection
            using (SqlConnection _connection = new SqlConnection(connectionString))
            {
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
                        string _outputPath = this.CreateOutputFile(outputDirectory, _table);
                        this.ScriptTable(_scripter, _table, _outputPath, false);
                    }
                }
            }
        }

        public void ScriptTable(Scripter scripter, Table table, string outputPath, bool append)
        {
            // alternatively
            // _scripter.Options.ToFileOnly = true;
            // _scripter.Options.FileName = _fileName;
            // _options.NoCommandTerminator = true;
            // _scripter.EnumScript(new Urn[] { _table.Urn });
            using (StreamWriter _writer = new StreamWriter(outputPath, append, this.Encoding))
            {
                int _row = 0;
                foreach (string _scriptLine in scripter.EnumScript(new Urn[] { table.Urn }))
                {
                    // FIXME: IDENTITY_INSERT ON ends with [space][cr+lf] when scripted by SMO
                    // but not when scripted by SSMS 10.50
                    string _trimmedScriptLine = _scriptLine.TrimEnd('\r', '\n', ' ');
                    if (_row > 0 && _row % this.CommandTerminatorInterval == 0)
                    {
                        _writer.WriteLine(Exporter.CommandTerminator);
                        if (this.PrintProcessedRecords)
                        {
                            int _processedRows = _row;

                            //FIXME: Find out why is this necessary
                            if (this.OffByOne)
                            {
                                _processedRows -= _row / this.CommandTerminatorInterval;
                            }
                            _writer.WriteLine(Exporter.PrintProcessedRecordsFormat, _processedRows);
                        }
                    }
                    _writer.WriteLine(_trimmedScriptLine);
                    _row++;
                }
            }
        }

        public string CreateOutputFile(string outputDirectory, Table table)
        {
            // output file
            string _regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex _regex = new Regex(string.Format("[{0}]", Regex.Escape(_regexSearch)));
            string _escapedName = _regex.Replace(table.Name, ".");
            string _outputPath = Path.Combine(outputDirectory, string.Format("{0}.{1}.Table.sql", table.Schema, _escapedName));
            return _outputPath;
        }
    }
}
