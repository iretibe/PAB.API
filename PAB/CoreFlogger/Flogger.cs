using Serilog;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer;

namespace CoreFlogger
{
    public static class Flogger
    {
        private static readonly ILogger _perfLogger;
        private static readonly ILogger _usageLogger;
        private static readonly ILogger _errorLogger;
        private static readonly ILogger _diagnosticLogger;
        public static IConfiguration Configuration { get; set; }

        static Flogger()
        {
            
            var connStr = Environment.GetEnvironmentVariable("FLOG_CONNSTR");

            //// If necessary, create it.
            if (connStr == null)
            {
                Environment.SetEnvironmentVariable("FLOG_CONNSTR",
                    "Data Source=psl-swd5;Initial Catalog=IDPAdmin;user id=sa;password=persol123");

                // Now retrieve it.
                connStr = Environment.GetEnvironmentVariable("FLOG_CONNSTR");
            }

            _perfLogger = new LoggerConfiguration()
                //.WriteTo.File(path: Environment.GetEnvironmentVariable("FLOG_CONNSTR"))
                .WriteTo.MSSqlServer(connStr, "PerfLogs", autoCreateSqlTable: true,
                    columnOptions: GetSqlColumnOptions(), batchPostingLimit: 20)
                .CreateLogger();

            _usageLogger = new LoggerConfiguration()
                //.WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_USAGE"))
                .WriteTo.MSSqlServer(connStr, "UsageLogs", autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions(), batchPostingLimit: 20)
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                //.WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_ERROR"))
                .WriteTo.MSSqlServer(connStr, "ErrorLogs", autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions(), batchPostingLimit: 20)
                .CreateLogger();

            _diagnosticLogger = new LoggerConfiguration()
                //.WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_DIAG"))
                .WriteTo.MSSqlServer(connStr, "DiagnosticLogs", autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions(), batchPostingLimit: 20)
                .CreateLogger();
        }

        private static string GetMessageFromException(Exception ex)
        {
            return ex.InnerException != null ? GetMessageFromException(ex.InnerException) : ex.Message;
        }

        private static string FindProcName(Exception ex)
        {
            var sqlEx = ex as SqlException;
            var procName = sqlEx?.Procedure;
            if (!string.IsNullOrEmpty(procName))
                return procName;

            if (!string.IsNullOrEmpty((string)ex.Data["Procedure"]))
            {
                return (string)ex.Data["Procedure"];
            }

            return ex.InnerException != null ? FindProcName(ex.InnerException) : null;
        }

        public static ColumnOptions GetSqlColumnOptions()
        {
            var colOptions = new ColumnOptions();
            colOptions.Store.Remove(StandardColumn.Properties);
            colOptions.Store.Remove(StandardColumn.MessageTemplate);
            colOptions.Store.Remove(StandardColumn.Message);
            colOptions.Store.Remove(StandardColumn.Exception);
            colOptions.Store.Remove(StandardColumn.TimeStamp);
            colOptions.Store.Remove(StandardColumn.Level);

            colOptions.AdditionalDataColumns = new Collection<DataColumn>
            {
                new DataColumn {DataType = typeof(DateTime), ColumnName = "Timestamp"},
                new DataColumn {DataType = typeof(string), ColumnName = "Product"},
                new DataColumn {DataType = typeof(string), ColumnName = "Layer"},
                new DataColumn {DataType = typeof(string), ColumnName = "Location"},
                new DataColumn {DataType = typeof(string), ColumnName = "Message"},
                new DataColumn {DataType = typeof(string), ColumnName = "Hostname"},
                new DataColumn {DataType = typeof(string), ColumnName = "UserId"},
                new DataColumn {DataType = typeof(string), ColumnName = "UserName"},
                new DataColumn {DataType = typeof(string), ColumnName = "Exception"},
                new DataColumn {DataType = typeof(int), ColumnName = "ElapsedMilliseconds"},
                new DataColumn {DataType = typeof(string), ColumnName = "CorrelationId"},
                new DataColumn {DataType = typeof(string), ColumnName = "CustomException"},
                new DataColumn {DataType = typeof(string), ColumnName = "AdditionalInfo"},
            };

            return colOptions;
        }

        public static void WritePerf(FlogDetail infoToLog)
        {
            //_perfLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);
            _perfLogger.Write(LogEventLevel.Information,
                    "{Timestamp}{Message}{Layer}{Location}{Product}" +
                    "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                    "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                    infoToLog.Timestamp, infoToLog.Message,
                    infoToLog.Layer, infoToLog.Location,
                    infoToLog.Product, infoToLog.CustomException,
                    infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                    infoToLog.Hostname, infoToLog.UserId,
                    infoToLog.UserName, infoToLog.CorrelationId,
                    infoToLog.AdditionalInfo
                    );
        }
        public static void WriteUsage(FlogDetail infoToLog)
        {
            //_usageLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);

            _usageLogger.Write(LogEventLevel.Information,
                    "{Timestamp}{Message}{Layer}{Location}{Product}" +
                    "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                    "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                    infoToLog.Timestamp, infoToLog.Message,
                    infoToLog.Layer, infoToLog.Location,
                    infoToLog.Product, infoToLog.CustomException,
                    infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                    infoToLog.Hostname, infoToLog.UserId,
                    infoToLog.UserName, infoToLog.CorrelationId,
                    infoToLog.AdditionalInfo
                    );
        }
        public static void WriteError(FlogDetail infoToLog)
        {
            if (infoToLog.Exception != null)
            {
                var procName = FindProcName(infoToLog.Exception);
                infoToLog.Location = string.IsNullOrEmpty(procName) ? infoToLog.Location : procName;
                infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            }
            //_errorLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);            
            _errorLogger.Write(LogEventLevel.Information,
                    "{Timestamp}{Message}{Layer}{Location}{Product}" +
                    "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                    "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                    infoToLog.Timestamp, infoToLog.Message,
                    infoToLog.Layer, infoToLog.Location,
                    infoToLog.Product, infoToLog.CustomException,
                    infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                    infoToLog.Hostname, infoToLog.UserId,
                    infoToLog.UserName, infoToLog.CorrelationId,
                    infoToLog.AdditionalInfo
                    );
        }
        public static void WriteDiagnostic(FlogDetail infoToLog)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)
                return;

            //_diagnosticLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);
            _diagnosticLogger.Write(LogEventLevel.Information,
                    "{Timestamp}{Message}{Layer}{Location}{Product}" +
                    "{CustomException}{ElapsedMilliseconds}{Exception}{Hostname}" +
                    "{UserId}{UserName}{CorrelationId}{AdditionalInfo}",
                    infoToLog.Timestamp, infoToLog.Message,
                    infoToLog.Layer, infoToLog.Location,
                    infoToLog.Product, infoToLog.CustomException,
                    infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                    infoToLog.Hostname, infoToLog.UserId,
                    infoToLog.UserName, infoToLog.CorrelationId,
                    infoToLog.AdditionalInfo
                    );
        }
    }
}