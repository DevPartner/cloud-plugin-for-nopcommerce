ALTER TABLE [dbo].[DP_CloudStorage_Queue] ADD [OldProviderSystemName] [nvarchar](max) NULL

MERGE  [dbo].[DP_CloudStorage_Queue] trgt
USING Picture src
ON  trgt.[EntityId]=src.Id 
WHEN NOT MATCHED
	THEN INSERT([EntityId]
      ,[OldProviderSystemName]
	  ,[OldProviderId]
      ,[TypeId]
      ,[StatusId]
      ,[CreatedOnUtc]
      ,[ChangedOnUtc])
	VALUES(src.Id
			  ,'nopCommerce'
			  ,-1
			  ,10
			  ,10
			  ,GETDATE()
			  ,GETDATE()
			)
	WHEN MATCHED THEN UPDATE SET
		trgt.[OldProviderSystemName] = 'nopCommerce';

DELETE [DP_CloudStorage_Queue] WHERE [OldProviderSystemName] IS NULL;

ALTER TABLE [dbo].[DP_CloudStorage_Queue] DROP COLUMN [OldProviderId]

ALTER TABLE [dbo].[DP_CloudStorage_Queue] ALTER COLUMN [OldProviderSystemName] [nvarchar](max) NOT NULL

update [RetailHub_Apr_6].[dbo].[Setting]
  set Value='Azure'
  where Name like '%devpartnercloudstoragesetting.picturestoretype%'

update [RetailHub_Apr_6].[dbo].[Setting]
  set Value='Azure'
  where Name like '%devpartnercloudstoragesetting.thumbpicturestoretype%'