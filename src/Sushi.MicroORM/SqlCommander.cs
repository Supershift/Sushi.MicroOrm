using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Represents the SQL Server data command wrapper.
    /// </summary>
    internal class SqlCommander : IDisposable
    {
        private SqlCommand Command;        
        private SqlConnection Connection;
        internal string Parameterlist;
        private bool m_Disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCommander"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>                
        /// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        public SqlCommander(string connection, int? commandTimeout)
        {
            if (string.IsNullOrWhiteSpace(connection))
                throw new ArgumentNullException(nameof(connection));

            this.Parameterlist = string.Empty;            
            this.Commandtype = CommandType.Text;
            this.ConnectionString = connection;
            this.CommandTimeout = commandTimeout;

            //create sql connection
            Connection = new SqlConnection(ConnectionString);

            //create a command
            Command = new SqlCommand() {
                Connection = Connection
            };
            //set the commands time out
            if(commandTimeout.HasValue)
                Command.CommandTimeout = commandTimeout.Value;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_Disposed)
            {
                if (disposing)
                {
                    if (Command != null)
                        Command.Dispose();
                    if (Connection.State != ConnectionState.Closed)
                        Connection.Close();
                }
            }
            this.m_Disposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SqlCommander"/> is reclaimed by garbage collection.
        /// </summary>
        ~SqlCommander()
        {
            Dispose(false);
        }        

        /// <summary>
        /// The database connectionString
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; protected set; }
                
        /// <summary>
        /// Get or sets the SQL statement that will be executed.
        /// </summary>        
        public string SqlText
        {
            get { return Command.CommandText; }
            set
            {
                Command.CommandText = value;
            }
        }

        
        /// <summary>
        /// Gets or sets the commandtype.
        /// </summary>
        /// <value>The commandtype.</value>
        public CommandType Commandtype { get; set; }

        /// <summary>
        /// Gets or sets the wait time in seconds before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int? CommandTimeout { get; protected set; }        

        /// <summary>
        /// Removes unnecessary whitespaces and line endings from <paramref name="sqlText"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        /// <returns></returns>
        private static string CleanSql(string sqlText)
        {
            sqlText = Regex.Replace(sqlText, "\n", "");
            sqlText = Regex.Replace(sqlText, "(  *)", " ");
            sqlText = Regex.Replace(sqlText, "( , )", " ,");
            sqlText = Regex.Replace(sqlText, "( = )", " =");
            return sqlText;
        }

        /// <summary>
        /// Set Sqlparameter as output value
        /// </summary>
        public void SetParameterOutput(string name, SqlDbType type, int length)
        {
            this.SetParameter(name, null, type, length, ParameterDirection.Output);
        }        
        
        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type)
        {
            this.SetParameter(name, itemvalue, type, 0, ParameterDirection.Input);
        }

        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type, string typeName)
        {
            this.SetParameter(name, itemvalue, type, 0, ParameterDirection.Input, typeName);
        }                

        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type, int length)
        {
            this.SetParameter(name, itemvalue, type, length, ParameterDirection.Input);
        }

        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type, int length, string typeName)
        {
            this.SetParameter(name, itemvalue, type, length, ParameterDirection.Input, typeName);
        }

        /// <summary>
        /// Set Sql parameter
        /// </summary>
        public void SetParameter(string name, object itemvalue, SqlDbType type, int length, ParameterDirection direction)
        {
            this.SetParameter(name, itemvalue, type, length, 0, direction);
        }
        /// <summary>
        /// Set Sql parameter
        /// </summary>
        public void SetParameter(string name, object itemvalue, SqlDbType type, int length, ParameterDirection direction, string typeName)
        {
            this.SetParameter(name, itemvalue, type, length, 0, direction, typeName);
        }        

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="itemvalue">The itemvalue.</param>
        /// <param name="type">The type.</param>
        /// <param name="length">The length.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="typeName">The type name for a table-valued parameter.</param>
        public void SetParameter(string name, object itemvalue, SqlDbType type, int length, byte scale, ParameterDirection direction, string typeName = null)
        {
            //if we already have the parameter, ignore the call
            if (this.Command.Parameters.Contains(name)) return;

            //create the parametr and add it to the command's collection of parameters
            var parameter = this.Command.Parameters.Add(name, type, length);

            parameter.Direction = direction;
            parameter.Scale = scale;

            if(!string.IsNullOrWhiteSpace(typeName))
            {
                parameter.TypeName = typeName;
            }

            //set the value. if supplied value is null, set the DB equivalent
            if (itemvalue == null)
                parameter.Value = DBNull.Value;
            else
            {
                parameter.Value = itemvalue;

                //  Verify the SqlTypes exception
                if (itemvalue.GetType().Namespace.ToLower() == "system.data.sqltypes")
                {
                    if (itemvalue.ToString().ToLower() == "null")
                        parameter.Value = DBNull.Value;
                }
            }

            this.Parameterlist += string.Format("{0} = '{1}' ({2})\r\n", name, itemvalue, type.ToString());            
        }

        /// <summary>
        /// Get the return value (object) of a specific parameter
        /// </summary>
        public object GetParameter(string name)
        {
            return this.Command.Parameters[name].Value;
        }

        /// <summary>
        /// Get the return value (int) of a specific parameter
        /// </summary>
        public int GetParamInt(string name)
        {
            object param = this.GetParameter(name);
            if (param == System.DBNull.Value)
                return 0;

            return int.Parse(param.ToString());
        }        

        
        /// <summary>
        /// Executes the <see cref="Command"/> and returns a <see cref="SqlDataReader"/> to read the result set.
        /// </summary>
        /// <value>The exec reader.</value>
        public SqlDataReader ExecReader()
        {            
            try
            {
                Open();
                var reader = this.Command.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        public async Task<SqlDataReader> ExecReaderAsync(CancellationToken cancellationToken)
        {
            try
            {
                await OpenAsync(cancellationToken).ConfigureAwait(false);
                var reader = await Command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                return reader;
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {
                throw new Exception(this.GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }       

        /// <summary>
        /// Execute the SqlCommand non query.
        /// </summary>
        public int ExecNonQuery()
        {            
            try
            {
                Open();
                return Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        public async Task<int> ExecNonQueryAsync(CancellationToken cancellationToken)
        {            
            try
            {
                await OpenAsync(cancellationToken).ConfigureAwait(false);
                return await Command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {
                throw new Exception(this.GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        /// <summary>
        /// Execute the SqlCommand scalar.
        /// </summary>
        public object ExecScalar()
        {            
            try
            {
                Open();
                return Command.ExecuteScalar();
            }
            catch (Exception ex)
            {                
                throw new Exception(this.GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        public async Task<object> ExecScalarAsync(CancellationToken cancellationToken)
        {
            try
            {
                await OpenAsync(cancellationToken).ConfigureAwait(false);
                return await Command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {   
                throw new Exception(this.GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        /// <summary>
        /// Calls <see cref="DatabaseConfiguration.Log"/> for the current command's text.
        /// </summary>
        protected void Log()
        {
            try
            {
                if (DatabaseConfiguration.Log != null)
                {
                    string message = $"\r\n{Command.CommandText}\r\n{Parameterlist}{DateTime.Now}\r\n";
                    DatabaseConfiguration.Log(message);
                }
            }
            catch
            {
#if DEBUG
                throw;
#endif
            }
        }

        public void Open()
        {   
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {   
            if (Connection.State != ConnectionState.Open)
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the error text.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private string GetErrorText(string error)
        {
            return string.Format("Error while executing<br/>{0}<br/>{1}<br/><br/><b>{2}</b>",
                this.SqlText,
                this.Parameterlist,
                error);
        }
    }
}
