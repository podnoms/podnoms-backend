USE master;
GO
IF EXISTS(SELECT 1
          FROM sys.databases
          WHERE [name] = N'PodNoms')
    BEGIN
        ALTER DATABASE [PodNoms] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE [PodNoms];
    END;
GO