using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UmbracoSQLMedia.Logic
{
    // A handler to return the image. The hander is installed via the web.config to match the handler path that the IFileSystem code
    // is initiated with, eg /SQLMedia.axd?path=
    class SQLMediaHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            using (FilestreamRepository fr = new FilestreamRepository(ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString, "Media"))
            {
                string path = context.Request.Params["path"];

                if (String.IsNullOrEmpty(path))
                {
                    // return 404
                }

                string mimeType;

                MemoryStream s = fr.GetFileStream(path, out mimeType);

                context.Response.ContentType = mimeType;
                context.Response.AppendHeader("Content-Length", s.Length.ToString());
                context.Response.BinaryWrite(s.ToArray());
            }
        }
    }
}
