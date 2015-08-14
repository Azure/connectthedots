CREATE PROCEDURE [dbo].[FetchFlowDataByDate]
	@startDate DateTime2,
	@endDate DateTime2
AS
BEGIN
	SELECT
		[FlowDataId], 
		[Value], 
		[Time]
	FROM [dbo].[FlowData]
	WHERE [Time] >= @startDate AND [Time] <= @endDate
END