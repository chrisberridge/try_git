/*==========================================================================*/
/* Source File:   HANDLEDATABASE.CS                                         */
/* Description:   Helper class to assist with database operations.          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.18/2013                                               */
/* Last Modified: May.05/2015                                               */
/* Version:       1.7                                                       */
/* Copyright (c), 2013, 2015 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Jul.18/2013 COQ File created.
============================================================================*/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ELCOLOMBIANO.EcCines.Data {
    /// <summary>
    /// Helper class to assist with database operations.
    /// </summary>
    public class HandleDatabase : IDisposable {
        protected readonly ILog log = null;
        private string _connectionPath = "Data Source=medvrt02;Initial Catalog=SalaEdicion4;User ID=se4;Password=se4;";
        private SqlConnection _conn;
        private bool open = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public HandleDatabase() {
            this.log = LogManager.GetLogger(this.GetType());
            if (log.IsDebugEnabled) {
                log.Debug("HandleDatabase Starts");
            }
            _conn = new SqlConnection(_connectionPath);
            if (log.IsDebugEnabled) {
                log.Debug("HandleDatabase Ends");
            }
        }

        /// <summary>
        /// Constructor using a supplied connection string
        /// </summary>
        /// <param name="connectionPath">Valid Microsoft SQL Server Connection string</param>
        public HandleDatabase(string connectionPath) : this() {
            if (log.IsDebugEnabled) {
                log.Debug("HandleDatabase Starts, Connection set");
            }
            _conn = new SqlConnection(connectionPath);
            if (log.IsDebugEnabled) {
                log.Debug("HandleDatabase Ends");
            }
        }

        /// <summary>
        /// Open connection to databse
        /// </summary>
        public void Open() {
            if (log.IsDebugEnabled) {
                log.Debug("Open Starts");
            }
            _conn.Open();
            open = true;
            if (log.IsDebugEnabled) {
                log.Debug("Open Ends");
            }
        }

        /// <summary>
        /// Close connection to database
        /// </summary>
        public void Close() {
            if (log.IsDebugEnabled) {
                log.Debug("Close Starts");
            }
            if (open) {
                _conn.Close();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Close Ends");
            }
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        /// <param name="disposing">True to dispose</param>
        protected virtual void Dispose(bool disposing) {
            if (log.IsDebugEnabled) {
                log.Debug("Dispose Starts");
            }
            if (disposing) {
                this.Close();
            }
            if (log.IsDebugEnabled) {
                log.Debug("Dispose Ends");
            }
        }

        /// <summary>
        /// not to virtually overriden.
        /// </summary>
        public void Dispose() {
            if (log.IsDebugEnabled) {
                log.Debug("Dispose");
            }
            Dispose(true);
            GC.SuppressFinalize(this);
            if (log.IsDebugEnabled) {
                log.Debug("Dispose");
            }
        }
        /// <summary>
        /// Execute an statement like SELECT count(1) FROM table and returns the first column value.
        /// </summary>
        /// <param name="trn">Transaction scope to use</param>
        /// <param name="sql">SQL statement to use, it can be INSERT, DELETE, UPDATE, SELECT.</param>
        /// <param name="parameters">List of named parameters to use.</param>
        /// <returns>SqlDataReader to process data</returns>
        public int ExecuteSelectSQLStmtAsScalar(SqlTransaction trn, string sql, params SqlParameter[] parameters) {
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSelectSQLStmtAsScalar Starts");
            }
            int rslt = 0;
            SqlCommand cmd = _conn.CreateCommand();
            cmd.CommandTimeout = 3600;
            cmd.CommandText = sql;
            cmd.Transaction = trn;
            cmd.CommandType = CommandType.Text;
            if (log.IsDebugEnabled) {
                log.Debug("sql=[" + sql + "]");
            }
            if (parameters != null && parameters.Length > 0) {
                var i = 1;
                foreach (var p in parameters) {
                    cmd.Parameters.Add(p);
                    var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                    log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                }
            }
            rslt = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Parameters.Clear();
            if (log.IsDebugEnabled) {
                log.Debug("ExecuteSelectSQLStmtAsScalar Ends");
            }
            return rslt;
        }

        /// <summary>
        /// Executes the SQL SELECT statement and returns a reader to retrieve data.
        /// </summary>
        /// <param name="trn">Transaction scope to use</param>
        /// <param name="sql">SQL statement to use, it can be INSERT, DELETE, UPDATE, SELECT.</param>
        /// <param name="parameters">List of named parameters to use.</param>
        /// <returns>SqlDataReader to process data</returns>
        public SqlDataReader ExecSelectSQLStmtAsReader(SqlTransaction trn, string sql, params SqlParameter[] parameters) {
            if (log.IsDebugEnabled) {
                log.Debug("ExecSelectSQLStmtAsReader Starts");
            }
            SqlCommand cmd = _conn.CreateCommand();
            cmd.CommandTimeout = 3600;
            cmd.CommandText = sql;
            cmd.Transaction = trn;
            cmd.CommandType = CommandType.Text;
            if (log.IsDebugEnabled) {
                log.Debug("sql=[" + sql + "]");
            }
            if (parameters != null && parameters.Length > 0) {
                var i = 1;
                foreach (var p in parameters) {
                    cmd.Parameters.Add(p);
                    var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                    log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                }
            }
            SqlDataReader reader = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            if (log.IsDebugEnabled) {
                log.Debug("ExecSelectSQLStmtAsReader Ends");
            }
            return reader;
        }

        /// <summary>
        /// Executes the SQL Statement given parameters
        /// </summary>
        /// <param name="trn">Transaction scope to use</param>
        /// <param name="sql">SQL statement to use, it can be INSERT, DELETE, UPDATE, SELECT.</param>
        /// <param name="parameters">List of named parameters to use.</param>
        public void ExecSQLStmt(SqlTransaction trn, string sql, params SqlParameter[] parameters) {
            if (log.IsDebugEnabled) {
                log.Debug("ExecSQLStmt Starts");
            }
            SqlCommand cmd = _conn.CreateCommand();
            cmd.CommandTimeout = 3600;
            cmd.CommandText = sql;
            cmd.Transaction = trn;
            cmd.CommandType = CommandType.Text;
            if (log.IsDebugEnabled) {
                log.Debug("sql=[" + sql + "]");
            }
            if (parameters != null && parameters.Length > 0) {
                var i = 1;
                foreach (var p in parameters) {
                    cmd.Parameters.Add(p);
                    var paramValues = "ParameterName=[" + p.ParameterName + "], Value=[" + p.Value + "], SqlDbType=[" + p.SqlDbType + "]";
                    log.Debug("Parameter " + i++ + " val=[" + paramValues + "]");
                }
            }
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            if (log.IsDebugEnabled) {
                log.Debug("ExecSQLStmt Ends");
            }
        }

        /// <summary>
        /// Start a local transaction
        /// </summary>
        /// <returns>A reference to the transaction</returns>
        public SqlTransaction BeginTransaction() {
            if (log.IsDebugEnabled) {
                log.Debug("BeginTransaction Starts");
            }            
            // Start a local transaction.            
            return _conn.BeginTransaction(IsolationLevel.ReadCommitted);            
        }

        /// <summary>
        /// Start a local transaction and give it a name.
        /// </summary>
        /// <param name="trnName">Transaction name</param>
        /// <returns>A reference to the transaction</returns>
        public SqlTransaction BeginTransaction(string trnName) {
            if (log.IsDebugEnabled) {
                log.Debug("BeginTransaction with name=["+trnName + "]");
            }

            return _conn.BeginTransaction(IsolationLevel.ReadCommitted, trnName);
        }
    }
}
