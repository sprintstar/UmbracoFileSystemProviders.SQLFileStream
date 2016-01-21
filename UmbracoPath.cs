using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoSQLMedia.Logic
{
    class UmbracoPath
    {
        // Takes the standard Umbraco media path of nnnn\y where n is the directory and y the filename
        // and splits it into directory and filename
        internal static bool MediaPathParse(string path, out int directory, out string filename)
        {
            bool success = false;
            directory = 0;
            filename = null;

            if (String.IsNullOrWhiteSpace(path) == false)
            {
                string[] parts = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts != null && parts.Length == 2)
                {
                    if (int.TryParse(parts[0], out directory))
                    {
                        filename = parts[1];
                        success = true;
                    }
                }
            }

            return success;
        }

        // A helper to always return the nnnn\y format path, because sometimes the functions are called
        // with that path, and sometimes the handler path as returned by GetUrl.
        internal static string MediaUrlParse(string handlerPath, string path)
        {
            if (path.StartsWith(handlerPath))
            {
                return path.Replace(handlerPath, "");
            }
            else
            {
                return path;
            }
        }
    }
}
