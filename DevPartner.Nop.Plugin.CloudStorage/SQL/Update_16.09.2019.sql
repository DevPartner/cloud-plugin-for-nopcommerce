UPDATE [dbo].[Picture]
  SET VirtualPath=ExternalUrl
  WHERE ExternalUrl is not null

ALTER TABLE Picture DROP COLUMN ExternalUrl