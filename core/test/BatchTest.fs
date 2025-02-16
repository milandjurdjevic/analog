module Analog.Core.Tests.BatchTest

open System.IO
open System.Text
open Analog.Core
open FSharp.Control
open Swensen.Unquote
open Xunit

let input =
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

[<Fact>]
let ``load stream as sync sequence of strings chopped by regex`` () =
    use stream = new MemoryStream(input)

    
    let actual =
        Batch.capture "\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}\]"
        |> Batch.sync stream
        |> Seq.toList

    test <@ actual = expected @>

[<Fact>]
let ``load stream as async sequence of strings chopped by regex`` () =
    task {
        use stream = new MemoryStream(input)
        
        let! actual =
            Batch.capture "\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}\]"
            |> Batch.async stream
            |> TaskSeq.toListAsync

        test <@ actual = expected @>
    }
