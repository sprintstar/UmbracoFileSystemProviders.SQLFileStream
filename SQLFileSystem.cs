using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core.IO;

namespace UmbracoSQLMedia.Logic
{
    public class SQLFileSystem
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public string HandlerPath { get; set; }
        public ILogger Logger { get; set; }

        public SQLFileSystem(string tableName, string handlerPath)
        {
            this.ConnectionString = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString;
            this.TableName = tableName;
            this.HandlerPath = handlerPath;

            string log = HostingEnvironment.MapPath("~/") + "Log-{Date}.txt";

            this.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile(log)
                .CreateLogger();
        }

        public void AddFile(string path, System.IO.Stream stream, bool overrideIfExists)
        {
            this.Logger.Information("AddFile({0}, {1}, {2})", path, stream, overrideIfExists);

            int directory;
            string filename;

            if (UmbracoPath.MediaPathParse(path, out directory, out filename))
            {
                string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                using (FilestreamRepository fsr = new FilestreamRepository(this.ConnectionString, this.TableName))
                {
                    bool exists = fsr.FileExists(path);

                    if (overrideIfExists)
                    {
                        if (exists)
                        {
                            fsr.UpdateFile(path, stream, filename, mimeType);
                        }
                        else
                        {
                            fsr.AddFile(directory, path, mimeType, filename, stream);
                        }
                    }
                    else
                    {
                        if (exists)
                        {
                            throw new Exception(String.Format("File '{0}' already exists", path));
                        }
                        else
                        {
                            fsr.AddFile(directory, path, mimeType, filename, stream);
                        }
                    }
                }
            }
        }

        public void AddFile(string path, System.IO.Stream stream)
        {
            this.Logger.Information("AddFile({0}, {1})", path, stream);

            this.AddFile(path, stream, true);
        }

        public void DeleteDirectory(string directory, bool recursive)
        {
            this.Logger.Information("DeleteDirectory({0}, {1})", directory, recursive);

            int dir;

            if (int.TryParse(directory, out dir))
            {
                using (FilestreamRepository fsr = new FilestreamRepository(this.ConnectionString, this.TableName))
                {
                    fsr.DeleteDirectory(dir);
                }
            }
            else
            {
                throw new Exception(String.Format("Cannot parse directory '{0}' as a number", directory));
            }
        }

        public void DeleteDirectory(string directory)
        {
            this.Logger.Information("DeleteDirectory({0})", directory);

            this.DeleteDirectory(directory, true);
        }

        // Sometimes this gets called with n\x where n is directory and x is file, and sometimes
        // it gets called with what I returned from GetUrl
        public bool FileExists(string path)
        {
            this.Logger.Information("FileExists({0})", path);

            path = UmbracoPath.MediaUrlParse(this.HandlerPath, path);

            using (FilestreamRepository fsr = new FilestreamRepository(this.ConnectionString, this.TableName))
            {
                return fsr.FileExists(path);
            }
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            this.Logger.Information("GetDirectories({0})", path);
            using (FilestreamRepository fsr = new FilestreamRepository(this.ConnectionString, this.TableName))
            {
                List<string> found = fsr.GetDirectories();
                if (found != null)
                {
                    return found;
                }
            }

            return new List<string>();
        }

        public DateTimeOffset GetLastModified(string path)
        {
            this.Logger.Information("GetLastModified({0})", path);

            path = UmbracoPath.MediaUrlParse(this.HandlerPath, path);

            using (FilestreamRepository fsr = new FilestreamRepository(this.ConnectionString, this.TableName))
            {
                return fsr.GetModifiedDate(path);
            }
        }

        public string GetFullPath(string path)
        {
            this.Logger.Information("GetFullPath({0})", path);
            //return "/" + path;
            return path;
        }

        public string GetRelativePath(string fullPathOrUrl)
        {
            this.Logger.Information("GetRelativePath({0})", fullPathOrUrl);

            //return String.Format(this.HandlerPath, fullPathOrUrl);
            return fullPathOrUrl;
        }

        // Get url eg /SQLMedia.axd?path={0}
        // path = "1001\img_2227.jpg"
        public string GetUrl(string path)
        {
            //{0}SQLMedia.axd?path={1}
            this.Logger.Information("GetUrl({0})", path);

            if (path.StartsWith(this.HandlerPath))
            {
                return path;
            }
            else
            {
                return this.HandlerPath + path;
            }
        }

        public System.IO.Stream OpenFile(string path)
        {
            this.Logger.Information("OpenFile({0})", path);

            path = UmbracoPath.MediaUrlParse(this.HandlerPath, path);

            using (FilestreamRepository fsr = new FilestreamRepository(this.ConnectionString, this.TableName))
            {
                string mimeType;
                return fsr.GetFileStream(path, out mimeType);
            }
        }
    }
}
