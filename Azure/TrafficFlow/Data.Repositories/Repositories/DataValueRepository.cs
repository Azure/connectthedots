using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Data.Contracts;
using Data.Repositories.Utils;

namespace Data.Repositories
{
    public class DataValueRepository
    {
        private const string DataId = "FlowDataId";
        private const string Value = "Value";
        private const string Time = "Time";

        private const string InsertStoreProcedure = "InsertFlowData";
        private const string InsertParameter = "@dataList";

        private const string StartDateParameter = "@startDate";
        private const string EndDateParameter = "@endDate";
        private const string FetchByDateStoreProcedure = "FetchFlowDataByDate";

        private const string TableType = "FlowDataTableType";

        private readonly string _sqlDatabaseConnectionString;

        public DataValueRepository(string sqlDatabaseConnectionString)
        {
            _sqlDatabaseConnectionString = sqlDatabaseConnectionString;
        }

        public void ProcessEvents(IList<ApiDataContract> eventDataList)
        {
            if (eventDataList == null)
            {
                return;
            }

            try
            {
                using (var sqlConnection = new SqlConnection(_sqlDatabaseConnectionString))
                {
                    sqlConnection.Open();

                    var table = new DataTable();

                    // Add columns to the table
                    table.Columns.Add(DataId, typeof(int));
                    table.Columns.Add(Value, typeof(int));
                    table.Columns.Add(Time, typeof(DateTime));

                    // Add rows to the table
                    foreach (var eventData in eventDataList)
                    {
                        // Note: EventData is disposable
                        table.Rows.Add(eventData.DataID, eventData.ReadingValue, eventData.Time);
                    }

                    // Create command
                    var sqlCommand = new SqlCommand(InsertStoreProcedure, sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add table-valued parameter
                    sqlCommand.Parameters.Add(new SqlParameter
                        {
                            ParameterName = InsertParameter,
                            SqlDbType = SqlDbType.Structured,
                            TypeName = TableType,
                            Value = table,
                        });

                    // Execute the query
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public IList<ApiDataContract> QueryByDateInterval(DateTime? start, DateTime? end)
        {
            if (start == null)
            {
                start = DateTime.UtcNow.AddYears(-20);
            }

            if (end == null)
            {
                end = DateTime.MaxValue.AddYears(-20);
            }

            try
            {
                using (var sqlConnection = new SqlConnection(_sqlDatabaseConnectionString))
                {
                    sqlConnection.Open();

                    // Create command
                    var sqlCommand = new SqlCommand(FetchByDateStoreProcedure, sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    sqlCommand.Parameters.AddWithValue(StartDateParameter, start);
                    sqlCommand.Parameters.AddWithValue(EndDateParameter, end);

                    // Execute the query

                    IList<ApiDataContract> result = new List<ApiDataContract>();
                    using (SqlDataReader r = sqlCommand.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ApiDataContract flow = new ApiDataContract
                            {
                                DataID = r.SafeParse<int>(DataId),
                                ReadingValue = r.SafeParse<int>(Value),
                                Time = r.SafeParse<DateTime>(Time)
                            };
                            result.Add(flow);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }
    }
}
