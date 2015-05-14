using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrafficFlow.Common.Utils;

namespace TrafficFlow.Common.Repositories
{
    public class FlowSourcesRepository
    {
        private const string Id = "Id";
        private const string Region = "Region";
        private const string StationName = "StationName";
        private const string LocationDescription = "LocationDescription";
        private const string LocationDirection = "LocationDirection";
        private const string LocationLatitude = "LocationLatitude";
        private const string LocationLongitude = "LocationLongitude";
        private const string LocationMilePost = "LocationMilePost";
        private const string LocationRoadName = "LocationRoadName";

        private const string UpdateStoreProcedure = "UpdateFlowSources";
        private const string InsertParameter = "@sourcesList";

        private const string FetchAllSourcesProcedure = "FetchAllFlowSources";

        private const string TableType = "FlowSourcesTableType";

        private readonly string _sqlDatabaseConnectionString;

        public FlowSourcesRepository(string sqlDatabaseConnectionString)
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
                    table.Columns.Add(Id, typeof(int));
                    table.Columns.Add(Region, typeof(string));
                    table.Columns.Add(StationName, typeof(string));
                    table.Columns.Add(LocationDescription, typeof(string));
                    table.Columns.Add(LocationDirection, typeof(string));
                    table.Columns.Add(LocationLatitude, typeof(string));
                    table.Columns.Add(LocationLongitude, typeof(string));
                    table.Columns.Add(LocationMilePost, typeof(double));
                    table.Columns.Add(LocationRoadName, typeof(string));


                    // Add rows to the table
                    foreach (var eventData in eventDataList)
                    {
                        table.Rows.Add(
                            eventData.FlowDataID,
                            eventData.Region,
                            eventData.StationName,
                            eventData.FlowStationLocation.Description,
                            eventData.FlowStationLocation.Direction,
                            eventData.FlowStationLocation.Latitude,
                            eventData.FlowStationLocation.Longitude,
                            eventData.FlowStationLocation.MilePost,
                            eventData.FlowStationLocation.RoadName
                            );
                    }

                    // Create command
                    var sqlCommand = new SqlCommand(UpdateStoreProcedure, sqlConnection)
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

        public IList<Flow> FetchAll()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(_sqlDatabaseConnectionString))
                {
                    sqlConnection.Open();

                    // Create command
                    var sqlCommand = new SqlCommand(FetchAllSourcesProcedure, sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Execute the query

                    IList<Flow> result = new List<Flow>();
                    using (SqlDataReader r = sqlCommand.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Flow flow = new Flow
                            {
                                FlowDataID = r.SafeParse<int>(Id),
                                Region = r.SafeParse<string>(Region),
                                StationName = r.SafeParse<string>(StationName),
                                FlowStationLocation = new FlowStationLocation
                                {
                                    Description = r.SafeParse<string>(LocationDescription),
                                    Direction = r.SafeParse<string>(LocationDirection),
                                    Latitude = r.SafeParse<double>(LocationLatitude),
                                    Longitude = r.SafeParse<double>(LocationLongitude),
                                    MilePost = r.SafeParse<double>(LocationMilePost),
                                    RoadName = r.SafeParse<string>(LocationRoadName)
                                }
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
