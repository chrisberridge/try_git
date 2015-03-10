/*==========================================================================*/
/* Source File:   HANDLEDATABASE.CS                                         */
/* Description:   Helper class to assist with database operations.          */
/* Author:        Carlos Adolfo Ortiz Quirós (COQ)                          */
/* Date:          Jul.18/2013                                               */
/* Last Modified: Feb.21/2015                                               */
/* Version:       1.5                                                       */
/* Copyright (c), 2013, 2015 Arkix, El Colombiano                           */
/*==========================================================================*/

/*===========================================================================
History
Jul.18/2013 COQ File created.
============================================================================*/

using System;
using System.Data;
using System.Data.SqlClient;

namespace ELCOLOMBIANO.EcCines.Data
{
    /// <summary>
    /// Helper class to assist with database operations.
    /// </summary>
    public class HandleDatabase : IDisposable
    {
        private string _connectionPath = "Data Source=medvrt02;Initial Catalog=SalaEdicion4;User ID=se4;Password=se4;";
        private SqlConnection _conn;
        private bool open = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public HandleDatabase()
        {
            _conn = new SqlConnection(_connectionPath);
        }

        /// <summary>
        /// Constructor using a supplied connection string
        /// </summary>
        /// <param name="connectionPath">Valid Microsoft SQL Server Connection string</param>
        public HandleDatabase(string connectionPath)
        {
            _conn = new SqlConnection(connectionPath);
        }

        /// <summary>
        /// Open connection to databse
        /// </summary>
        public void Open()
        {
            _conn.Open();
            open = true;
        }

        /// <summary>
        /// Close connection to database
        /// </summary>
        public void Close()
        {
            if (open)
                _conn.Close();
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        /// <param name="disposing">True to dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        /// <summary>
        /// not to virtually overriden.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Execute an statement like SELECT count(1) FROM table and returns the first column value.
        /// </summary>
        /// <param name="trn">Transaction scope to use</param>
        /// <param name="sql">SQL statement to use, it can be INSERT, DELETE, UPDATE, SELECT.</param>
        /// <param name="parameters">List of named parameters to use.</param>
        /// <returns>SqlDataReader to process data</returns>
        public int ExecuteSelectSQLStmtAsScalar(SqlTransaction trn, string sql, params SqlParameter[] parameters)
        {
            int rslt = 0;
            SqlCommand cmd = _conn.CreateCommand();
            cmd.CommandTimeout = 3600;
            cmd.CommandText = sql;
            cmd.Transaction = trn;
            cmd.CommandType = CommandType.Text;
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            rslt = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Parameters.Clear();
            return rslt;
        }

        /// <summary>
        /// Executes the SQL SELECT statement and returns a reader to retrieve data.
        /// </summary>
        /// <param name="trn">Transaction scope to use</param>
        /// <param name="sql">SQL statement to use, it can be INSERT, DELETE, UPDATE, SELECT.</param>
        /// <param name="parameters">List of named parameters to use.</param>
        /// <returns>SqlDataReader to process data</returns>
        public SqlDataReader ExecSelectSQLStmtAsReader(SqlTransaction trn, string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = _conn.CreateCommand();
            cmd.CommandTimeout = 3600;
            cmd.CommandText = sql;
            cmd.Transaction = trn;
            cmd.CommandType = CommandType.Text;
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            SqlDataReader reader = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            return reader;
        }

        /// <summary>
        /// Executes the SQL Statement given parameters
        /// </summary>
        /// <param name="trn">Transaction scope to use</param>
        /// <param name="sql">SQL statement to use, it can be INSERT, DELETE, UPDATE, SELECT.</param>
        /// <param name="parameters">List of named parameters to use.</param>
        public void ExecSQLStmt(SqlTransaction trn, string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = _conn.CreateCommand();
            cmd.CommandTimeout = 3600;
            cmd.CommandText = sql;
            cmd.Transaction = trn;
            cmd.CommandType = CommandType.Text;
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
        }

        /// <summary>
        /// Start a local transaction
        /// </summary>
        /// <returns>A reference to the transaction</returns>
        public SqlTransaction BeginTransaction()
        {
            // Start a local transaction.            
            return _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Start a local transaction and give it a name.
        /// </summary>
        /// <param name="trnName">Transaction name</param>
        /// <returns>A reference to the transaction</returns>
        public SqlTransaction BeginTransaction(string trnName)
        {
            return _conn.BeginTransaction(IsolationLevel.ReadCommitted, trnName);
        }


        public static DataTable ExecuteTableSP(string cnnStr,
            string procedure, params SqlParameter[] args)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cnn = new SqlConnection(cnnStr))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procedure;
                    cmd.CommandTimeout = 60;

                    if (args != null && args.Length > 0)
                    {
                        foreach (SqlParameter p in args)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
    }
}
