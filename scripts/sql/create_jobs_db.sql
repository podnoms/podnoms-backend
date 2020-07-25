USE master
GO
WHILE EXISTS(select NULL
from sys.databases
where name='PodNoms.Jobs')
BEGIN
    DECLARE @SQL varchar(max)
    SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
    FROM MASTER..SysProcesses
    WHERE DBId = DB_ID(N'PodNoms.Jobs') AND SPId <> @@SPId
    EXEC(@SQL)
    DROP DATABASE [PodNoms.Jobs]
END
GO

CREATE DATABASE [PodNoms.Jobs]
GO
CREATE LOGIN podnomsjobs
WITH PASSWORD = 'podnomsjobs', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
USE [PodNoms.Jobs]
GO
CREATE USER podnomsjobs
 FOR LOGIN podnomsjobs
WITH DEFAULT_SCHEMA = dbo
GO

EXEC sp_addrolemember N'db_owner', N'podnomsjobs'
EXEC sp_addrolemember N'db_datareader', N'podnomsjobs'
EXEC sp_addrolemember N'db_ddladmin', N'podnomsjobs'
GO

CREATE SCHEMA maintenance
GO

CREATE PROC maintenance.SP_ResetHangfire
AS
BEGIN
    TRUNCATE TABLE [HangFire].[AggregatedCounter]
    TRUNCATE TABLE [HangFire].[Counter]
    TRUNCATE TABLE [HangFire].[JobParameter]
    TRUNCATE TABLE [HangFire].[JobQueue]
    TRUNCATE TABLE [HangFire].[List]
    TRUNCATE TABLE [HangFire].[State]
    DELETE FROM [HangFire].[Job]
    DBCC CHECKIDENT ('[HangFire].[Job]', reseed, 0)
    UPDATE [HangFire].[Hash] SET Value = 1 WHERE Field = 'LastJobId'
END
GO
