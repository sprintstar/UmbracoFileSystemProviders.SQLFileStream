using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace UmbracoSQLMedia.Logic
{
    // Do the work off adding and retrieving files from the database
    class FilestreamRepository : IDisposable
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }

        internal FilestreamRepository(string connectionString, string tableName)
        {
            this.ConnectionString = connectionString;
            this.TableName = tableName;
        }

        // I did make this class create and use a transaction, but didn't go down that route in the end.
        public void Dispose()
        {
        }

        // Add file to the media table
        internal Guid AddFile(int directory, string path, string mimeType, string filename, System.IO.Stream stream)
        {
            string sql = String.Format(@"
                DECLARE @mediaId table (MediaId uniqueidentifier)
          
                INSERT INTO [{0}](Directory, Path, MimeType, Filename, [Data], DataLength, Created)
                OUTPUT inserted.MediaId INTO @mediaId
                VALUES(@directory, @path, @mimetype, @filename, @data, @datalength, @created);
          
                SELECT r.MediaId FROM @mediaId r", this.TableName);

            int streamLength = (int)stream.Length;

            stream.Seek(0, SeekOrigin.Begin);
            byte[] fileData = null;
            using (BinaryReader rdr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                fileData = rdr.ReadBytes(streamLength);
            }

            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@directory", SqlDbType.Int).Value = directory;
                cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = path;
                cmd.Parameters.Add("@mimetype", SqlDbType.VarChar).Value = mimeType;
                cmd.Parameters.Add("@filename", SqlDbType.VarChar).Value = filename;
                cmd.Parameters.Add("@data", SqlDbType.VarBinary, fileData.Length).Value = fileData;
                cmd.Parameters.Add("@datalength", SqlDbType.Int).Value = streamLength;
                cmd.Parameters.Add("@created", SqlDbType.DateTime2).Value = DateTime.Now;

                con.Open();
                Guid mediaId = (Guid)cmd.ExecuteScalar();

                return mediaId;
            }
        }

        // Update the file in the database
        internal void UpdateFile(string path, Stream stream, string filename, string mimeType)
        {
            string sql = String.Format(@"
                UPDATE [{0}]
                SET filename = @filename, mimeType = @mimetype, data = @data, datalength = @datalength, modified = @modified
                WHERE Path = @path
            ", this.TableName);

            int streamLength = (int)stream.Length;

            stream.Seek(0, SeekOrigin.Begin);
            byte[] fileData = null;
            using (BinaryReader rdr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                fileData = rdr.ReadBytes(streamLength);
            }

            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = path;
                cmd.Parameters.Add("@mimetype", SqlDbType.VarChar).Value = mimeType;
                cmd.Parameters.Add("@filename", SqlDbType.VarChar).Value = filename;
                cmd.Parameters.Add("@data", SqlDbType.VarBinary, fileData.Length).Value = fileData;
                cmd.Parameters.Add("@datalength", SqlDbType.Int).Value = streamLength;
                cmd.Parameters.Add("@modified", SqlDbType.DateTime2).Value = DateTime.Now;

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // Delete any file that has the specified directory
        internal void DeleteDirectory(int directory)
        {
            string sql = String.Format(@"
                DELETE [{0}]
                WHERE Directory = @directory
            ", this.TableName);

            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@directory", SqlDbType.Int).Value = directory;

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // Returns true if specified file exists
        internal bool FileExists(string path)
        {
            string sql = String.Format(@"
                SELECT Path
                FROM [{0}]
                WHERE Path = @path
            ", this.TableName);

            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = path;

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        // Returns a list of the 'directories' in the database
        internal List<string> GetDirectories()
        {
            List<string> directories = new List<string>();

            string sql = String.Format(@"
                SELECT Directory FROM [{0}]", this.TableName);

            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        directories.Add(reader.GetInt32(0).ToString());
                    }

                    return directories;
                }
            }
        }

        // Returns the modified date of the file
        internal DateTimeOffset GetModifiedDate(string path)
        {
            string sql = String.Format(@"
                SELECT Modified, Created
                FROM [{0}]
                WHERE Path = @path
            ", this.TableName);

            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = path;

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows == false)
                    {
                        throw new Exception(String.Format("Specified file '{0}' doesn't exist", path));
                    }

                    reader.Read();

                    if (reader.IsDBNull(0))
                    {
                        return reader.GetDateTime(1);
                    }
                    else
                    {
                        return reader.GetDateTime(0);
                    }
                }
            }
        }

        // Returns a memory stream of the file, using FILESTREAM technology
        internal MemoryStream GetFileStream(string path, out string mimeType)
        {
            string sql = String.Format(@"
                    SELECT
                        MimeType,
                        [Data].PathName(),
                        GET_FILESTREAM_TRANSACTION_CONTEXT()
                    FROM [{0}]
                    WHERE Path = @path", this.TableName);

            using (TransactionScope tx = new TransactionScope())
            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@path", path);

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    mimeType = reader.GetFieldValue<string>(0);
                    string filePath = reader.GetFieldValue<string>(1);
                    byte[] txnToken = reader.GetFieldValue<byte[]>(2);
                    reader.Close();

                    using (SqlFileStream sqlFileStream = new SqlFileStream(filePath, txnToken, FileAccess.Read))
                    {
                        MemoryStream ms = new MemoryStream();
                        sqlFileStream.Seek(0, SeekOrigin.Begin);
                        sqlFileStream.CopyTo(ms);

                        tx.Complete();

                        return ms;
                    }
                }
            }
        }
    }
}
