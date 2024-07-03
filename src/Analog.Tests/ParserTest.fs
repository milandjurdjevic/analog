module Analog.Tests.ParserTest

open System.IO
open System.Text
open VerifyTests
open VerifyXunit
open Xunit
open FsUnit.Xunit
open Analog.Parser

let pattern =
    @"^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))"

let rawLogs =
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

let parsedLogs =
    [ Map
          [ "Timestamp", "2018-10-15 15:38:22.685 +02:00"
            "Severity", "INF"
            "Message",
            "Configuration Result:\n[Success] Name GTS.MAUTO.Service\n[Success] DisplayName GTS MAUTO service\n[Success] Description Service for standalone mode linked to GTS Vision application.\n[Success] ServiceName GTS.MAUTO.Service\n" ]
      Map
          [ "Timestamp", "2018-10-15 15:38:22.729 +02:00"
            "Severity", "INF"
            "Message", "Topshelf v4.0.0.0, .NET Framework v4.0.30319.42000\n" ]
      Map
          [ "Timestamp", "2018-10-15 15:38:22.829 +02:00"
            "Severity", "INF"
            "Message", "Using default implementation for object serializer\n" ]
      Map
          [ "Timestamp", "2018-10-15 15:38:22.845 +02:00"
            "Severity", "INF"
            "Message", "Using default implementation for ThreadExecutor\n" ]
      Map
          [ "Timestamp", "2018-10-15 15:38:22.860 +02:00"
            "Severity", "INF"
            "Message", "Initialized Scheduler Signaller of type: Quartz.Core.SchedulerSignalerImpl\n" ]
      Map
          [ "Timestamp", "2018-10-15 15:38:22.860 +02:00"
            "Severity", "INF"
            "Message", "Quartz Scheduler v.2.6.1.0 created.\n" ]
      Map
          [ "Timestamp", "2018-10-15 15:38:22.861 +02:00"
            "Severity", "INF"
            "Message", "JobFactory set to: GTS.Scheduler.AutofacJobFactory\n" ]
      Map["Timestamp", "2018-10-15 15:38:22.861 +02:00"
          "Severity", "INF"
          "Message", "RAMJobStore initialized.\n"]
      Map["Timestamp", "2018-10-15 15:38:22.863 +02:00"
          "Severity", "INF"

          "Message",
          "Scheduler meta-data: Quartz Scheduler (v2.6.1.0) 'QuartzScheduler' with instanceId 'NON_CLUSTERED'\n  Scheduler class: 'Quartz.Core.QuartzScheduler' - running locally.\n  NOT STARTED.\n  Currently in standby mode.\n  Number of jobs executed: 0\n  Using thread pool 'Quartz.Simpl.SimpleThreadPool' - with 1 threads.\n  Using job-store 'Quartz.Simpl.RAMJobStore' - which does not support persistence. and is not clustered.\n"]
      Map["Timestamp", "2018-10-15 15:38:22.863 +02:00"
          "Severity", "INF"
          "Message", "Quartz scheduler 'QuartzScheduler' initialized\n"]
      Map["Timestamp", "2018-10-15 15:38:22.863 +02:00"
          "Severity", "INF"
          "Message", "Quartz scheduler version: 2.6.1.0\n"]
      Map["Timestamp", "2018-10-15 15:38:22.888 +02:00"
          "Severity", "DBG"
          "Message", "Started by the Windows services process\n"]
      Map["Timestamp", "2018-10-15 15:38:22.888 +02:00"
          "Severity", "DBG"
          "Message", "Running as a service, creating service host.\n"]
      Map["Timestamp", "2018-10-15 15:38:22.889 +02:00"
          "Severity", "INF"
          "Message", "Starting as a Windows service\n"]
      Map["Timestamp", "2018-10-15 15:38:22.891 +02:00"
          "Severity", "DBG"
          "Message", "[Topshelf] Starting up as a windows service application\n"]
      Map["Timestamp", "2018-10-15 15:38:22.893 +02:00"
          "Severity", "INF"
          "Message", "[Topshelf] Starting\n"]
      Map["Timestamp", "2018-10-15 15:38:22.893 +02:00"
          "Severity", "DBG"
          "Message", "[Topshelf] Current Directory: D:\\GTS_VISION\\MAUTO Service\n"]
      Map["Timestamp", "2018-10-15 15:38:22.893 +02:00"
          "Severity", "DBG"
          "Message", "[Topshelf] Arguments:\n"]
      Map["Timestamp", "2018-10-15 15:38:22.895 +02:00"
          "Severity", "INF"
          "Message", "MAUTO Windows Service starting\n"]
      Map["Timestamp", "2018-10-15 15:38:22.994 +02:00"
          "Severity", "DBG"

          "Message",
          "Connected to Data Source=localhost;Initial Catalog=IRECVISION_MQB_PROD_LOCAL;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False\n"]
      Map["Timestamp", "2018-10-15 15:38:23.032 +02:00"
          "Severity", "DBG"

          "Message",
          "Connected to Data Source=GTSSQL-VISION;Initial Catalog=IRECVISION_MQB_PROD;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False\n"]
      Map["Timestamp", "2018-10-15 15:38:23.033 +02:00"
          "Severity", "DBG"

          "Message",
          "Connected to Data Source=localhost;Initial Catalog=IRECVISION_MQB_PROD_LOCAL;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False\n"]
      Map["Timestamp", "2018-10-15 15:38:23.038 +02:00"
          "Severity", "DBG"

          "Message",
          "Connected to Data Source=GTSSQL-VISION;Initial Catalog=IRECVISION_MQB_PROD;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False\n"]
      Map["Timestamp", "2018-10-15 15:38:23.040 +02:00"
          "Severity", "DBG"

          "Message",
          "Connected to Data Source=localhost;Initial Catalog=IRECVISION_MQB_PROD_LOCAL;Integrated Security=False;User ID=GTSMAUTO;Password=***;MultipleActiveResultSets=False\n"]
      Map["Timestamp", "2018-10-15 15:38:23.040 +02:00"
          "Severity", "DBG"

          "Message",
          "Get MAUTOParameter\nSELECT\n  [Mp_jetonmaitre] AS [RemoteToken],\n  [Mp_jetonlocal]  AS [LocalToken]\nFROM [dbo].[MAUTOParametre]\n"]
      Map["Timestamp", "2018-10-15 15:38:23.162 +02:00"
          "Severity", "ERR"

          "Message",
          "Failed to start service\nSystem.Exception: Integrity error, the token of the local base does not correspond to the remote database.\n   à GTS.MAUTO.MAUTOClient.CheckIntegrity() dans D:\\TFS\\Beta\\Quattro.2.x\\Quattro.2.28\\Quattro.2.28.0\\MAUTO\\src\\GTS.MAUTO\\MAUTOClient.cs:ligne 122\n   à GTS.MAUTO.Service.MAUTOService.Start() dans D:\\TFS\\Beta\\Quattro.2.x\\Quattro.2.28\\Quattro.2.28.0\\MAUTO\\src\\GTS.MAUTO.Service\\MAUTOService.cs:ligne 44\n"]
      Map["Timestamp", "2018-10-15 15:38:23.184 +02:00"
          "Severity", "INF"
          "Message", "[Topshelf] Started\n"]
      Map["Timestamp", "2018-10-15 15:40:55.598 +02:00"
          "Severity", "INF"
          "Message", "[Topshelf] Stopping\n"]
      Map["Timestamp", "2018-10-15 15:40:55.599 +02:00"
          "Severity", "INF"
          "Message", "MAUTO Windows Service stopping\n"]
      Map["Timestamp", "2018-10-15 15:40:55.599 +02:00"
          "Severity", "INF"
          "Message", "Cron scheduler stopping\n"]
      Map["Timestamp", "2018-10-15 15:40:55.601 +02:00"
          "Severity", "INF"
          "Message", "Scheduler QuartzScheduler_$_NON_CLUSTERED shutting down.\n"]
      Map["Timestamp", "2018-10-15 15:40:55.601 +02:00"
          "Severity", "INF"
          "Message", "Scheduler QuartzScheduler_$_NON_CLUSTERED paused.\n"]
      Map["Timestamp", "2018-10-15 15:40:55.604 +02:00"
          "Severity", "DBG"
          "Message", "Shutting down threadpool...\n"]
      Map["Timestamp", "2018-10-15 15:40:55.604 +02:00"
          "Severity", "DBG"
          "Message", "Shutdown of threadpool complete.\n"]
      Map["Timestamp", "2018-10-15 15:40:55.604 +02:00"
          "Severity", "INF"
          "Message", "Scheduler QuartzScheduler_$_NON_CLUSTERED Shutdown complete.\n"]
      Map["Timestamp", "2018-10-15 15:40:55.604 +02:00"
          "Severity", "INF"
          "Message", "Cron scheduler stopped\n"]
      Map["Timestamp", "2018-10-15 15:40:55.604 +02:00"
          "Severity", "INF"
          "Message", "MAUTO Windows Service stopped\n"]
      Map["Timestamp", "2018-10-15 15:40:55.606 +02:00"
          "Severity", "INF"
          "Message", "Cron scheduler stopping\n"]
      Map["Timestamp", "2018-10-15 15:40:55.606 +02:00"
          "Severity", "INF"
          "Message", "Cron scheduler stopped\n"]
      Map["Timestamp", "2018-10-15 15:40:55.660 +02:00"
          "Severity", "INF"
          "Message", "[Topshelf] Stopped\n"]
      Map["Timestamp", "2018-10-15 15:40:55.953 +02:00"
          "Severity", "DBG"
          "Message", "WorkerThread is shut down\n"] ]

[<Fact>]
let ``parse iterates through all log entries`` () =
    use stream = new MemoryStream(rawLogs)
    let result = System.Collections.Generic.List<Map<string, string>>()
    let tap (log: Map<string, string>) = result.Add(log)
    parse tap pattern stream
    Snapshot.compare result
