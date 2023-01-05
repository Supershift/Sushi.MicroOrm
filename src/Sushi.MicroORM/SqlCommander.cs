using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Data;

using System.Threading.Tasks;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Represents the SQL Server data command wrapper.
    /// </summary>
    internal class SqlCommander : IDisposable
    {
        private readonly SqlCommand _command;        
        private readonly SqlConnection _connection;
        private string _parameterlist;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCommander"/> class.
        /// </summary>
        /// <param name="connectionString">The connection.</param>                
        /// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        public SqlCommander(string connectionString, int? commandTimeout)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _parameterlist = string.Empty;            
            Commandtype = CommandType.Text;
            ConnectionString = connectionString;
            CommandTimeout = commandTimeout;

            //create sql connection
            _connection = new SqlConnection(ConnectionString);

            //create a command
            _command = new SqlCommand() {
                Connection = _connection
            };
            //set the commands time out
            if(commandTimeout.HasValue)
                _command.CommandTimeout = commandTimeout.Value;
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
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (_command != null)
                        _command.Dispose();
                    if (_connection.State != ConnectionState.Closed)
                        _connection.Close();
                }
            }
            isDisposed = true;
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
            get { return _command.CommandText; }
            set
            {
                _command.CommandText = value;
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
        /// Set Sqlparameter as output value
        /// </summary>
        public void SetParameterOutput(string name, SqlDbType type, int length)
        {
            SetParameter(name, null, type, length, ParameterDirection.Output);
        }        
        
        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type)
        {
            SetParameter(name, itemvalue, type, 0, ParameterDirection.Input);
        }

        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type, string typeName)
        {
            SetParameter(name, itemvalue, type, 0, ParameterDirection.Input, typeName);
        }                

        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type, int length)
        {
            SetParameter(name, itemvalue, type, length, ParameterDirection.Input);
        }

        /// <summary>
        /// Set Sqlparameter as input value
        /// </summary>
        public void SetParameterInput(string name, object itemvalue, SqlDbType type, int length, string typeName)
        {
            SetParameter(name, itemvalue, type, length, ParameterDirection.Input, typeName);
        }

        /// <summary>
        /// Set Sql parameter
        /// </summary>
        public void SetParameter(string name, object itemvalue, SqlDbType type, int length, ParameterDirection direction)
        {
            SetParameter(name, itemvalue, type, length, 0, direction);
        }
        /// <summary>
        /// Set Sql parameter
        /// </summary>
        public void SetParameter(string name, object itemvalue, SqlDbType type, int length, ParameterDirection direction, string typeName)
        {
            SetParameter(name, itemvalue, type, length, 0, direction, typeName);
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
            if (_command.Parameters.Contains(name)) return;

            //create the parametr and add it to the command's collection of parameters
            var parameter = _command.Parameters.Add(name, type, length);

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

            _parameterlist += string.Format("{0} = '{1}' ({2})\r\n", name, itemvalue, type.ToString());            
        }

        /// <summary>
        /// Get the return value (object) of a specific parameter
        /// </summary>
        public object GetParameter(string name)
        {
            return _command.Parameters[name].Value;
        }

        /// <summary>
        /// Get the return value (int) of a specific parameter
        /// </summary>
        public int GetParamInt(string name)
        {
            object param = GetParameter(name);
            if (param == System.DBNull.Value)
                return 0;

            return int.Parse(param.ToString());
        }        

        /// <summary>
        /// Executes the <see cref="_command"/> and returns a <see cref="SqlDataReader"/> to read the result set.
        /// </summary>        
        public async Task<SqlDataReader> ExecReaderAsync(CancellationToken cancellationToken)
        {
            try
            {
                await OpenAsync(cancellationToken).ConfigureAwait(false);
                var reader = await _command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                return reader;
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {
                throw new Exception(GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        /// <summary>
        /// Executes the SqlCommand non query.
        /// </summary>
        public async Task<int> ExecNonQueryAsync(CancellationToken cancellationToken)
        {            
            try
            {
                await OpenAsync(cancellationToken).ConfigureAwait(false);
                return await _command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {
                throw new Exception(GetErrorText(ex.Message), ex);
            }
            finally
            {
                Log();
            }
        }

        /// <summary>
        /// Execute the SqlCommand scalar.
        /// </summary>
        public async Task<object> ExecScalarAsync(CancellationToken cancellationToken)
        {
            try
            {
                await OpenAsync(cancellationToken).ConfigureAwait(false);
                return await _command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {   
                throw new Exception(GetErrorText(ex.Message), ex);
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
                    string message = $"\r\n{_command.CommandText}\r\n{_parameterlist}{DateTime.Now}\r\n";
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
        
        public async Task OpenAsync(CancellationToken cancellationToken)
        {   
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the error text.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private string GetErrorText(string error)
        {
            return string.Format("Error while executing<br/>{0}<br/>{1}<br/><br/><b>{2}</b>",
                SqlText,
                _parameterlist,
                error);
        }
    }
}
