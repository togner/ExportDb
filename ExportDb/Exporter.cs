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
                    if (_row > 0 && _row % this.CommandTerminatorInterval == 0)
                    {
                        _writer.WriteLine("GO");
                        if (this.PrintProcessedRecords)
                        {
                            _writer.WriteLine("print 'Processed {0} total records'", _row);
                        }
                    }
                    _writer.WriteLine(_scriptLine);
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
