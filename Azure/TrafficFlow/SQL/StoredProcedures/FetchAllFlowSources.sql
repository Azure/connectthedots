CREATE PROCEDURE [dbo].[FetchAllFlowSources]
AS
BEGIN
	SELECT
		[Id],
		[Region],
		[StationName],
		[LocationDescription],
		[LocationDirection],
		[LocationLatitude],
		[LocationLongitude],
		[LocationMilePost],
		[LocationRoadName]
	FROM [dbo].[FlowSources]
END