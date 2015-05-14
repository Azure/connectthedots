CREATE PROCEDURE [dbo].[InsertFlowData]
	@dataList [dbo].[FlowDataTableType] READONLY
AS
	insert into [dbo].[FlowData] ([FlowDataId], [Value], [Time])
		select [FlowDataId], [Value], [Time] from @dataList
RETURN 0
