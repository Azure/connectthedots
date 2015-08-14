CREATE TYPE [dbo].[FlowSourcesTableType] AS TABLE(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Region] VARCHAR(20) NULL, 
    [StationName] VARCHAR(20) NULL, 
    [LocationDescription] VARCHAR(20) NULL, 
    [LocationDirection] VARCHAR(20) NULL, 
    [LocationLatitude] VARCHAR(20) NULL, 
    [LocationLongitude] VARCHAR(20) NULL, 
    [LocationMilePost] FLOAT NULL, 
    [LocationRoadName] VARCHAR(20) NULL
) 