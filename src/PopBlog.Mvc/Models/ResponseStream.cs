using System;
using System.Data.Common;
using System.IO;

namespace PopBlog.Mvc.Models;

public class ResponseStream(DbDataReader reader, DbConnection connection, Stream stream) : IDisposable
{
    public Stream Stream => stream;
    
    public void Dispose()
    {
        reader.Close();
        connection.Close();
        stream.Close();
        
        reader.Dispose();
        connection.Dispose();
        stream.Dispose();
    }
}