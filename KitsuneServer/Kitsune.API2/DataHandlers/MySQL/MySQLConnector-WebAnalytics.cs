using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitsune.API.Model.ApiRequestModels;
using MySql.Data.MySqlClient;
using Kitsune.API2.EnvConstants;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Kitsune.API2.DataHandlers.MySQL
{
    public partial class MySQLConnector
    {
        internal List<AnalyticsData> GetVistorsForSite(string website, VistorsFilterType filterType, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                Initialize();

                MySqlCommand command = new MySqlCommand(MySQLStoredProcedures.GetVisitorByDay, _connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("?customer_id", website);
                command.Parameters.AddWithValue("?from_date", fromDate);
                command.Parameters.AddWithValue("?to_date", toDate);

                switch (filterType)
                {
                    case VistorsFilterType.CITY:
                        command.CommandText = MySQLStoredProcedures.GetVisitorsByCity;
                        return GetVisitorsByCity(command);

                    case VistorsFilterType.COUNTRY:
                        command.CommandText = MySQLStoredProcedures.GetVisitorsByCountry;
                        return GetVisitorsByCountry(command);

                    case VistorsFilterType.DAY:
                        command.CommandText = MySQLStoredProcedures.GetVisitorByDay;
                        return GetVisitorsByDay(command);

                    case VistorsFilterType.HOUR:
                        command.CommandText = MySQLStoredProcedures.GetVisitorByHour;
                        return GetVisitorsByHour(command);

                    default:
                        return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                CleanUp(_connection);
            }
        }

        internal List<AnalyticsData> GetVisitorsByDay(MySqlCommand mySqlCommand)
        {
            try
            {
                var cursor = mySqlCommand.ExecuteReader();
                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = Convert.ToDateTime(cursor["date"]).ToString("yyyy-MM-dd"),
                            DataCount = Convert.ToInt64(cursor["unique_visitors"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        internal List<AnalyticsData> GetVisitorsByHour(MySqlCommand mySqlCommand)
        {
            try
            {
                var cursor = mySqlCommand.ExecuteReader();
                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = Convert.ToDateTime(cursor["date"]).ToString("yyyy-MM-dd"),
                            Key2 = cursor["hour"].ToString(),
                            DataCount = Convert.ToInt64(cursor["unique_visitors"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal List<AnalyticsData> GetVisitorsByCountry(MySqlCommand mySqlCommand)
        {
            try
            {
                var cursor = mySqlCommand.ExecuteReader();
                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = cursor["country"].ToString(),
                            Key2 = cursor["country_code"].ToString(),
                            DataCount = Convert.ToInt64(cursor["visitors"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal List<AnalyticsData> GetVisitorsByCity(MySqlCommand mySqlCommand)
        {
            try
            {
                var cursor = mySqlCommand.ExecuteReader();
                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = cursor["city"].ToString(),
                            Key2 = cursor["country"].ToString(),
                            DataCount = Convert.ToInt64(cursor["visitors"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal List<AnalyticsData> GetAllRequestsPerDayByUserId(List<string> projects, List<string>websites, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                Initialize();

                MySqlCommand command = new MySqlCommand(MySQLStoredProcedures.GetAllRequestsPerDayByUserId, _connection);
                command.CommandType = CommandType.StoredProcedure;

                projects = projects.Select(x => string.Format("'{0}'", x)).ToList();
                websites = websites.Select(x => string.Format("'{0}'", x)).ToList();
                websites.AddRange(projects);

                command.Parameters.AddWithValue("?customer_ids", string.Join(',', websites));
                command.Parameters.AddWithValue("?from_date", string.Format("'{0}'",fromDate?.ToString("yyyy-MM-dd")));
                command.Parameters.AddWithValue("?to_date", string.Format("'{0}'", toDate?.ToString("yyyy-MM-dd")));

                var cursor = command.ExecuteReader();

                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = Convert.ToDateTime(cursor["date"]).ToString("yyyy-MM-dd"),
                            DataCount = Convert.ToInt64(cursor["requests"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CleanUp(_connection);
            }
        }
 
        internal List<AnalyticsData> GetStoragePerDay(List<string>projectIds, List<string> websiteIds, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                Initialize();

                projectIds = projectIds.Select(x => string.Format("'{0}'", x)).ToList();
                websiteIds = websiteIds.Select(x => string.Format("'{0}'", x)).ToList();

                if (projectIds.Count == 0)
                {
                    projectIds.Add("''");
                }

                if(websiteIds.Count == 0)
                {
                    websiteIds.Add("''");
                }

                MySqlCommand command = new MySqlCommand(MySQLStoredProcedures.GetStorageByDayForProjectWebsiteIDs, _connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("?project_ids", string.Join(',', projectIds));
                command.Parameters.AddWithValue("?website_ids", string.Join(',', websiteIds));
                command.Parameters.AddWithValue("?from_date", string.Format("'{0}'", fromDate?.ToString("yyyy-MM-dd")));
                command.Parameters.AddWithValue("?to_date", string.Format("'{0}'", toDate?.ToString("yyyy-MM-dd")));

                var cursor = command.ExecuteReader();

                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = Convert.ToDateTime(cursor["date"]).ToString("yyyy-MM-dd"),
                            DataCount = Convert.ToDouble(cursor["space"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CleanUp(_connection);
            }
        }

        internal List<AnalyticsData> GetRequestsByBrowser(string website, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                Initialize();

                MySqlCommand command = new MySqlCommand(MySQLStoredProcedures.GetRequestsByBrowsers, _connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("?customer_id", website);
                command.Parameters.AddWithValue("?from_date", fromDate);
                command.Parameters.AddWithValue("?to_date", toDate);

                var cursor = command.ExecuteReader();

                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = cursor["browsers"].ToString(),
                            DataCount = Convert.ToInt64(cursor["requests"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                CleanUp(_connection);
            }
        }

        internal List<AnalyticsData> GetRequestsByDevices(string website, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                Initialize();

                MySqlCommand command = new MySqlCommand(MySQLStoredProcedures.GetRequestsByDevices, _connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("?customer_id", website);
                command.Parameters.AddWithValue("?from_date", fromDate);
                command.Parameters.AddWithValue("?to_date", toDate);

                var cursor = command.ExecuteReader();

                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = cursor["devices"].ToString(),
                            DataCount = Convert.ToInt64(cursor["requests"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CleanUp(_connection);
            }
        }

        internal List<AnalyticsData> GetTrafficSources(string website, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                Initialize();

                MySqlCommand command = new MySqlCommand(MySQLStoredProcedures.GetRequestsByReferrers, _connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("?customer_id", website);
                command.Parameters.AddWithValue("?from_date", fromDate);
                command.Parameters.AddWithValue("?to_date", toDate);

                var cursor = command.ExecuteReader();

                if (cursor.HasRows)
                {
                    var data = new List<AnalyticsData>();
                    while (cursor.Read())
                    {
                        data.Add(new AnalyticsData()
                        {
                            Key1 = cursor["referer_domain"].ToString(),
                            DataCount = Convert.ToInt64(cursor["requests"])
                        });
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CleanUp(_connection);
            }
        }
    }
}
