using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sorlov.Windows.UserExec
{
    public class CommandlineArguments
    {
        private readonly Dictionary<string,string> parameters = new Dictionary<string,string>();

        public CommandlineArguments(IEnumerable<string> cmdArgs)
        {
            var regexSplitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var regexCleaner = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string parameter = null;

            foreach (string textPart in cmdArgs)
            {
                string[] parts = regexSplitter.Split(textPart, 3);

                switch (parts.Length)
                {
                    case 1:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                            {
                                parts[0] = regexCleaner.Replace(parts[0], "$1");
                                parameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        break;

                    case 2:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                                parameters.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        break;

                    case 3:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                                parameters.Add(parameter, "true");
                        }

                        parameter = parts[1];

                        if (!parameters.ContainsKey(parameter))
                        {
                            parts[2] = regexCleaner.Replace(parts[2], "$1");
                            parameters.Add(parameter, parts[2]);
                        }

                        parameter = null;
                        break;
                }
            }
            if (parameter != null)
            {
                if (!parameters.ContainsKey(parameter))
                    parameters.Add(parameter, "true");
            }
        }

        public string[] Keys
        {
            get
            {
                var result = new List<string>();
                foreach(var key in parameters.Keys)
                    result.Add(key);
                return result.ToArray();
            }
        }

        public string this[string parameterName]
        {
            get
            {
                var data =  (parameters[parameterName]);

                return data ?? string.Empty;
            }
        }
    }
}
