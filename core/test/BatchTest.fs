module Analog.Core.Tests.BatchTest

open System.IO
open System.Text
open Analog.Core
open Swensen.Unquote
open Xunit

[<Fact>]
let ``load stream as sequence of strings chopped by regex`` () =
    let raw =
        """
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

    use stream = new MemoryStream(raw)

    let expected =
        [ "[2018-10-15 15:38:23.184 +02:00] [INF] [Topshelf] Started"
          "[2018-10-15 15:40:55.598 +02:00] [INF] [Topshelf] Stopping"
          "[2018-10-15 15:40:55.599 +02:00] [INF] MAUTO Windows Service stopping"
          "[2018-10-15 15:40:55.599 +02:00] [INF] Cron scheduler stopping"
          "[2018-10-15 15:40:55.601 +02:00] [INF] Scheduler QuartzScheduler_$_NON_CLUSTERED shutting down."
          "[2018-10-15 15:40:55.601 +02:00] [INF] Scheduler QuartzScheduler_$_NON_CLUSTERED paused."
          "[2018-10-15 15:40:55.604 +02:00] [DBG] Shutting down threadpool..."
          "[2018-10-15 15:40:55.604 +02:00] [DBG] Shutdown of threadpool complete."
          "[2018-10-15 15:40:55.604 +02:00] [INF] Scheduler QuartzScheduler_$_NON_CLUSTERED Shutdown complete."
          "[2018-10-15 15:40:55.604 +02:00] [INF] Cron scheduler stopped"
          "[2018-10-15 15:40:55.604 +02:00] [INF] MAUTO Windows Service stopped"
          "[2018-10-15 15:40:55.606 +02:00] [INF] Cron scheduler stopping"
          "[2018-10-15 15:40:55.606 +02:00] [INF] Cron scheduler stopped"
          "[2018-10-15 15:40:55.660 +02:00] [INF] [Topshelf] Stopped"
          "[2018-10-15 15:40:55.953 +02:00] [DBG] WorkerThread is shut down" ]

    let actual =
        Batch.load "\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}\]" 10 stream
        |> Seq.toList

    test <@ actual = expected @>
