
MERGE  [dbo].[DP_CloudStorage_Queue] trgt
USING Picture AS src
ON  trgt.[EntityId]=src.Id 
WHEN NOT MATCHED
	THEN INSERT([EntityId]
      ,[OldProviderSystemName]
      ,[TypeId]
      ,[StatusId]
      ,[CreatedOnUtc]
      ,[ChangedOnUtc])
	VALUES(src.Id
			  ,@providerName
			  ,10
			  ,10
			  ,GETDATE()
			  ,GETDATE()
			)
	WHEN MATCHED THEN UPDATE SET
		trgt.[OldProviderSystemName] = @providerName,
		trgt.[TypeId] = 10,
		trgt.[StatusId] = 10,
		trgt.[ChangedOnUtc] = GETDATE();
