using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace ExportDb
{
    /// <summary>
    /// Arguments class, parses command line args.
    /// </summary>
    public sealed class Arguments
    {
        private StringDictionary m_parameters;

        /// <summary>
        /// Initializes a new instance of Arguments class.
        /// </summary>
        /// <param name="args">String repre of the arguments.</param>
        public Arguments(string[] args)
        {
            this.m_parameters = new StringDictionary();
            Regex _spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex _remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string _parameter = null;
            string[] _parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string _txt in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                _parts = _spliter.Split(_txt, 3);

                switch (_parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (_parameter != null)
                        {
                            if (!this.m_parameters.ContainsKey(_parameter))
                            {
                                _parts[0] = _remover.Replace(_parts[0], "$1");
                                this.m_parameters.Add(_parameter, _parts[0]);
                            }
                            _parameter = null;
                        }

                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (_parameter != null)
                        {
                            if (!this.m_parameters.ContainsKey(_parameter))
                            {
                                this.m_parameters.Add(_parameter, "true");
                            }
                        }
                        _parameter = _parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (_parameter != null)
                        {
                            if (!this.m_parameters.ContainsKey(_parameter))
                            {
                                this.m_parameters.Add(_parameter, "true");
                            }
                        }
                        _parameter = _parts[1];

                        // Remove possible enclosing characters (",')
                        if (!this.m_parameters.ContainsKey(_parameter))
                        {
                            _parts[2] = _remover.Replace(_parts[2], "$1");
                            this.m_parameters.Add(_parameter, _parts[2]);
                        }
                        _parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (_parameter != null)
            {
                if (!this.m_parameters.ContainsKey(_parameter))
                {
                    this.m_parameters.Add(_parameter, "true");
                }
            }
        }

        /// <summary>
        /// Retrieve a parameter value if it exists.
        /// </summary>
        /// <param name="param">Name of the param whose value we want.</param>
        /// <returns>Value of the param.</returns>
        public string this[string param]
        {
            get
            {
                return (this.m_parameters[param]);
            }
        }
    }
}
