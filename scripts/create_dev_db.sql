USE master
GO
WHILE EXISTS(select NULL
from sys.databases
where name='PodNoms')
BEGIN
    DECLARE @SQL varchar(max)
    SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
    FROM MASTER..SysProcesses
    WHERE DBId = DB_ID(N'PodNoms') AND SPId <> @@SPId
    EXEC(@SQL)
    DROP DATABASE [PodNoms]
END
GO


CREATE DATABASE PodNoms
GO
CREATE LOGIN podnomsweb
WITH PASSWORD = 'podnomsweb', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
USE PodNoms
GO
CREATE USER podnomsweb
 FOR LOGIN podnomsweb
WITH DEFAULT_SCHEMA = dbo
GO

EXEC sp_addrolemember N'db_owner', N'podnomsweb'
EXEC sp_addrolemember N'db_datareader', N'podnomsweb'
EXEC sp_addrolemember N'db_ddladmin', N'podnomsweb'

GO
