
CREATE LOGIN [CDEUser] WITH PASSWORD=N'n@M#!guA', 
DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

USE [PercCancerGov]
GO


CREATE USER [CDEUser] FOR LOGIN [CDEUser] WITH DEFAULT_SCHEMA=[dbo]

exec sp_addrolemember 'db_datareader', cdeuser