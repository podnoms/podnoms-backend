USE [PodNoms.Jobs]
GO
TRUNCATE TABLE [HangFire].[AggregatedCounter]
TRUNCATE TABLE [HangFire].[Counter]
TRUNCATE TABLE [HangFire].[JobParameter]
TRUNCATE TABLE [HangFire].[JobQueue]
TRUNCATE TABLE [HangFire].[List]
TRUNCATE TABLE [HangFire].[State]
DELETE FROM [HangFire].[Job]
DBCC CHECKIDENT ('[HangFire].[Job]', reseed, 0)
UPDATE [HangFire].[Hash] SET Value = 1 WHERE Field = 'LastJobId'