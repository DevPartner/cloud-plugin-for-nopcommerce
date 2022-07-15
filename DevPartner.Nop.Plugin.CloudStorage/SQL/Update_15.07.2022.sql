INSERT INTO [dbo].[LocaleStringResource]
           ([LanguageId]
           ,[ResourceName]
           ,[ResourceValue])
     VALUES
           (1
           ,'DevPartner.CloudStorage.ConfigureModel.MainImageUrlFormat'
           ,'MainImageUrlFormat')

INSERT INTO [dbo].[LocaleStringResource]
           ([LanguageId]
           ,[ResourceName]
           ,[ResourceValue])
     VALUES
           (1
           ,'DevPartner.CloudStorage.ConfigureModel.MainImageUrlFormat.Hint'
           ,'"{0}?w={1}", 0 - URL, 1 - targetSize')

INSERT INTO [dbo].[Setting]
           ([Name]
           ,[Value]
           ,[StoreId])
     VALUES
           ('devpartnercloudstoragesetting.mainimageurlformat'
           ,'{0}'
           ,0)