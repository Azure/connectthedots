CREATE TABLE [dbo].[FlowData]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
	[FlowDataId] INT NULL, 
    [Value] INT NULL, 
    [Time] DATETIME2 NULL, 
    CONSTRAINT [FK_FlowData_ToTable] FOREIGN KEY (FlowDataId) REFERENCES [dbo].[FlowSources]([Id])
)

GO

CREATE INDEX [IX_FlowDataIdTime_Column] ON [dbo].[FlowData] ([FlowDataId], [Time])

GO

CREATE INDEX [IX_FlowDataTimeId_Column] ON [dbo].[FlowData] ([Time])
