using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.DataHandlers.MySQL
{
    public partial class MySQLConnector
    {
        MySqlConnection _connection;

        public void Initialize(string connectionString = null)
        {
            try
            {
                if (_connection == null)
                {
                    var configurationSettings = EnvironmentConstants.ApplicationConfiguration;
                    if (String.IsNullOrEmpty(connectionString))
                        _connection = new MySqlConnection(configurationSettings.DBConnectionStrings.KitsuneWebLogConnectionUrl);
                    else
                        _connection = new MySqlConnection(connectionString);
                }

                if (ConnectionState.Broken == _connection.State || ConnectionState.Closed == _connection.State)
                    _connection.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void CleanUp(MySqlConnection connection)
        {
            try
            {
                if (null != connection)
                    connection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
