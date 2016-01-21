using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core.IO;

namespace UmbracoSQLMedia.Logic
{
    public class FileSystem : IFileSystem
    {
        public SQLFileSystem SQLFileSystem { get; set; }

        public FileSystem(string tableName, string handlerPath)
        {
            this.SQLFileSystem = new SQLFileSystem(tableName, handlerPath);
        }

        public void AddFile(string path, Stream stream)
        {
            this.SQLFileSystem.AddFile(path, stream);
        }

        public void AddFile(string path, Stream stream, bool overrideIfExists)
        {
            this.SQLFileSystem.AddFile(path, stream, overrideIfExists);
        }

        public void DeleteDirectory(string path)
        {
            this.SQLFileSystem.DeleteDirectory(path);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            this.SQLFileSystem.DeleteDirectory(path, recursive);
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            return this.SQLFileSystem.FileExists(path);
        }

        public DateTimeOffset GetCreated(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            return this.SQLFileSystem.GetDirectories(path);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFiles(string path, string filter)
        {
            throw new NotImplementedException();
        }

        public string GetFullPath(string path)
        {
            return this.SQLFileSystem.GetFullPath(path);
        }

        public DateTimeOffset GetLastModified(string path)
        {
            return this.SQLFileSystem.GetLastModified(path);
        }

        public string GetRelativePath(string fullPathOrUrl)
        {
            return this.SQLFileSystem.GetRelativePath(fullPathOrUrl);
        }

        public string GetUrl(string path)
        {
            return this.SQLFileSystem.GetUrl(path);
        }

        public Stream OpenFile(string path)
        {
            return this.SQLFileSystem.OpenFile(path);
        }
    }
}
