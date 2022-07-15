IF EXISTS ( SELECT [name] FROM sys.tables WHERE [name] = 'DP_CloudStorage_Queue' ) DROP TABLE DP_CloudStorage_Queue;
CREATE TABLE DP_CloudStorage_Queue(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NOT NULL,
	[OldProviderSystemName] [nvarchar](max) NOT NULL,
	[TypeId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
	[CreatedOnUtc] [datetime] NOT NULL,
	[ChangedOnUtc] [datetime] NOT NULL,
	[FilePath] [nvarchar](max) NULL,
	 CONSTRAINT [PK_DP_CloudStorage_Queue] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) );

IF COL_LENGTH('[dbo].[Picture]', 'ExternalUrl') IS NULL
BEGIN
        ALTER TABLE [dbo].[Picture]
        ADD [ExternalUrl] [nvarchar](max) NULL
END