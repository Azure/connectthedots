using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrafficFlow.Common.Utils;

namespace TrafficFlow.Common.Repositories
{
    public class FlowDataRepository
    {
        private const string FlowDataId = "FlowDataId";
        private const string Value = "Value";
        private const string Time = "Time";

        private const string InsertStoreProcedure = "InsertFlowData";
        private const string InsertParameter = "@dataList";

        private const string StartDateParameter = "@startDate";
        private const string EndDateParameter = "@endDate";
        private const string FetchByDateStoreProcedure = "FetchFlowDataByDate";

        private const string TableType = "FlowDataTableType";

        private readonly string _sqlDatabaseConnectionString;

        public FlowDataRepository(string sqlDatabaseConnectionString)
        {
            _sqlDatabaseConnectionString = sqlDatabaseConnectionString;
        }

        public void ProcessEvents(IList<Flow> eventDataList)
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
                    table.Columns.Add(FlowDataId, typeof(int));
                    table.Columns.Add(Value, typeof(int));
                    table.Columns.Add(Time, typeof(DateTime));

                    // Add rows to the table
                    foreach (var eventData in eventDataList)
                    {
                        // Note: EventData is disposable
                        table.Rows.Add(eventData.FlowDataID, eventData.FlowReadingValue, eventData.Time);
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

        public IList<Flow> QueryByDateInterval(DateTime? start, DateTime? end)
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

                    IList<Flow> result = new List<Flow>();
                    using (SqlDataReader r = sqlCommand.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Flow flow = new Flow
                            {
                                FlowDataID = r.SafeParse<int>(FlowDataId),
                                FlowReadingValue = r.SafeParse<int>(Value),
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
