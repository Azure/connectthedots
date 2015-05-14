CREATE PROCEDURE [dbo].[UpdateFlowSources]
	@sourcesList [dbo].[FlowSourcesTableType] READONLY
AS
	MERGE INTO [dbo].[FlowSources] AS A 
    USING ( 
        SELECT * FROM @sourcesList 
    ) B ON (A.Id = B.Id) 
    WHEN MATCHED THEN 
        UPDATE SET
			A.[Region] = B.[Region], 
			A.[StationName] = B.[StationName],
			A.[LocationDescription] = B.[LocationDescription],
			A.[LocationDirection] = B.[LocationDirection],
			A.[LocationLatitude] = B.[LocationLatitude],
			A.[LocationLongitude] = B.[LocationLongitude],
			A.[LocationMilePost] = B.[LocationMilePost],
			A.[LocationRoadName] = B.[LocationRoadName]
    WHEN NOT MATCHED THEN 
        INSERT (
			[Id],
			[Region],
			[StationName],
			[LocationDescription],
			[LocationDirection],
			[LocationLatitude],
			[LocationLongitude],
			[LocationMilePost],
			[LocationRoadName]
		)
        VALUES
		(
			B.[Id],
			B.[Region],
			B.[StationName],
			B.[LocationDescription],
			B.[LocationDirection],
			B.[LocationLatitude],
			B.[LocationLongitude],
			B.[LocationMilePost],
			B.[LocationRoadName]
		);
RETURN 0
