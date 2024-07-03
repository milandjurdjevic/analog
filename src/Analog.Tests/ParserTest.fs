module Analog.Tests.ParserTest

open System.IO
open System.Text
open Xunit
open Analog.Parser

let pattern =
    @"^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))"

let bytes =
    """
[2018-10-15 15:38:22.685 +02:00] [INF] Configuration Result:
[Success] Name GTS.MAUTO.Service
[Success] DisplayName GTS MAUTO service
[Success] Description Service for standalone mode linked to GTS Vision application.
[Success] ServiceName GTS.MAUTO.Service
[2018-10-15 15:38:22.729 +02:00] [INF] Topshelf v4.0.0.0, .NET Framework v4.0.30319.42000
[2018-10-15 15:38:22.829 +02:00] [INF] Using default implementation for object serializer
[2018-10-15 15:38:22.845 +02:00] [INF] Using default implementation for ThreadExecutor
[2018-10-15 15:38:22.860 +02:00] [INF] Initialized Scheduler Signaller of type: Quartz.Core.SchedulerSignalerImpl
[2018-10-15 15:38:22.860 +02:00] [INF] Quartz Scheduler v.2.6.1.0 created.
[2018-10-15 15:38:22.861 +02:00] [INF] JobFactory set to: GTS.Scheduler.AutofacJobFactory
[2018-10-15 15:38:22.861 +02:00] [INF] RAMJobStore initialized.
[2018-10-15 15:38:22.863 +02:00] [INF] Scheduler meta-data: Quartz Scheduler (v2.6.1.0) 'QuartzScheduler' with instanceId 'NON_CLUSTERED'
  Scheduler class: 'Quartz.Core.QuartzScheduler' - running locally.
  NOT STARTED.
  Currently in standby mode.
  Number of jobs executed: 0
  Using thread pool 'Quartz.Simpl.SimpleThreadPool' - with 1 threads.
  Using job-store 'Quartz.Simpl.RAMJobStore' - which does not support persistence. and is not clustered.
[2018-10-15 15:38:22.863 +02:00] [INF] Quartz scheduler 'QuartzScheduler' initialized
[2018-10-15 15:38:22.863 +02:00] [INF] Quartz scheduler version: 2.6.1.0
[2018-10-15 15:38:22.888 +02:00] [DBG] Started by the Windows services process
[2018-10-15 15:38:22.888 +02:00] [DBG] Running as a service, creating service host.
[2018-10-15 15:38:22.889 +02:00] [INF] Starting as a Windows service
[2018-10-15 15:38:22.891 +02:00] [DBG] [Topshelf] Starting up as a windows service application
[2018-10-15 15:38:22.893 +02:00] [INF] [Topshelf] Starting
[2018-10-15 15:38:22.893 +02:00] [DBG] [Topshelf] Current Directory: D:\GTS_VISION\MAUTO Service
[2018-10-15 15:38:22.893 +02:00] [DBG] [Topshelf] Arguments:
[2018-10-15 15:38:22.895 +02:00] [INF] MAUTO Windows Service starting
[2018-10-15 15:38:22.994 +02:00] [DBG] Connected to Data Source=localhost;Initial Catalog=IRECVISION_MQB_PROD_LOCAL;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False
[2018-10-15 15:38:23.032 +02:00] [DBG] Connected to Data Source=GTSSQL-VISION;Initial Catalog=IRECVISION_MQB_PROD;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False
[2018-10-15 15:38:23.033 +02:00] [DBG] Connected to Data Source=localhost;Initial Catalog=IRECVISION_MQB_PROD_LOCAL;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False
[2018-10-15 15:38:23.038 +02:00] [DBG] Connected to Data Source=GTSSQL-VISION;Initial Catalog=IRECVISION_MQB_PROD;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False
[2018-10-15 15:38:23.040 +02:00] [DBG] Connected to Data Source=localhost;Initial Catalog=IRECVISION_MQB_PROD_LOCAL;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False
[2018-10-15 15:38:23.040 +02:00] [DBG] Get MAUTOParameter
SELECT
  [Mp_jetonmaitre] AS [RemoteToken],
  [Mp_jetonlocal]  AS [LocalToken]
FROM [dbo].[MAUTOParametre]
[2018-10-15 15:38:23.162 +02:00] [ERR] Failed to start service
System.Exception: Integrity error, the token of the local base does not correspond to the remote database.
   à GTS.MAUTO.MAUTOClient.CheckIntegrity() dans D:\TFS\Beta\Quattro.2.x\Quattro.2.28\Quattro.2.28.0\MAUTO\src\GTS.MAUTO\MAUTOClient.cs:ligne 122
   à GTS.MAUTO.Service.MAUTOService.Start() dans D:\TFS\Beta\Quattro.2.x\Quattro.2.28\Quattro.2.28.0\MAUTO\src\GTS.MAUTO.Service\MAUTOService.cs:ligne 44
[2018-10-15 15:38:23.184 +02:00] [INF] [Topshelf] Started
[2018-10-15 15:40:55.598 +02:00] [INF] [Topshelf] Stopping
[2018-10-15 15:40:55.599 +02:00] [INF] MAUTO Windows Service stopping
[2018-10-15 15:40:55.599 +02:00] [INF] Cron scheduler stopping
[2018-10-15 15:40:55.601 +02:00] [INF] Scheduler QuartzScheduler_$_NON_CLUSTERED shutting down.
[2018-10-15 15:40:55.601 +02:00] [INF] Scheduler QuartzScheduler_$_NON_CLUSTERED paused.
[2018-10-15 15:40:55.604 +02:00] [DBG] Shutting down threadpool...
[2018-10-15 15:40:55.604 +02:00] [DBG] Shutdown of threadpool complete.
[2018-10-15 15:40:55.604 +02:00] [INF] Scheduler QuartzScheduler_$_NON_CLUSTERED Shutdown complete.
[2018-10-15 15:40:55.604 +02:00] [INF] Cron scheduler stopped
[2018-10-15 15:40:55.604 +02:00] [INF] MAUTO Windows Service stopped
[2018-10-15 15:40:55.606 +02:00] [INF] Cron scheduler stopping
[2018-10-15 15:40:55.606 +02:00] [INF] Cron scheduler stopped
[2018-10-15 15:40:55.660 +02:00] [INF] [Topshelf] Stopped
[2018-10-15 15:40:55.953 +02:00] [DBG] WorkerThread is shut down
"""
    |> Encoding.UTF8.GetBytes

[<Fact>]
let ``parse iterates through all log entries`` () =
    use stream = new MemoryStream(bytes)
    let result = System.Collections.Generic.List<Map<string, string>>()
    let tap (log: Map<string, string>) = result.Add(log)
    parse tap pattern stream
    Snapshot.compare result
