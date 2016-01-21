# UmbracoFileSystemProviders.SQLFileStream
A SQL Server IFileSystem provider to store Umbraco media in a SQL database table instead of the file system.  It is an IFileSystem privider that can be added to an existing Umbraco installation (tested on Umbraco v7.3.0).  It acts against a table that you can add to your Umbraco database (see sql file).

Its very rough and ready, and needs work and testing to make it better.  The IFileSystem interface didn't make this an easy task.  The code uses http://serilog.net/ for loggin, but it would be better to remove this in favour of an interface.

## Installation

* Add the files to your project.
* Create your additional table in your database (note it uses FILESTREAM so you will need to enable this on your table/server)
* Change the FileSystemProviders.config file to use your IFileSystem implementation
* Add your handler to the web.config

### Example FileSystemProviders.config

```
  <Provider alias="media" type="UmbracoSQLMedia.Logic.FileSystem, UmbracoSQLMedia">
    <Parameters>
      <add key="tableName" value="Media" />
      <add key="handlerPath" value="/SQLMedia.axd?path=" />
    </Parameters>
  </Provider>
```

### Example web.config handler entry

```
 <system.webServer>
    <handlers accessPolicy="Read, Write, Script, Execute">
      <add verb="GET" path="SQLMedia.axd" name="UmbracoSQLMedia" type="UmbracoSQLMedia.Logic.SQLMediaHandler, UmbracoSQLMedia" />
    </handlers>
 </system.webServer>
```

## Notes

* It breaks ImageProcessor in that you can no longer put widths/heights on the resulting image urls for resizing
* The umbraco CMS says the image has zero size, not sure why
