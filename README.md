# Analog

A developer-oriented tool designed to simplify the parsing and analysis of log files locally. It enables custom parsing
and transformation of raw logs into structured formats, making it easier to explore, debug, and prepare log data for
analytics.

## Quickstart

```shell
analog input.log -o output.json
```

_input.log_

```
[2025-06-16T21:14:03Z] [INFO] Starting service...
[2025-06-16T21:14:04Z] [WARN] Disk space is running low
[2025-06-16T21:14:05Z] [ERROR] Failed to connect to database
[2025-06-16T21:14:06Z] [DEBUG] Retry attempt 1
[2025-06-16T21:14:07Z] [INFO] Service started successfully
```

_output.json_

```json
[
  {
    "level": "INFO",
    "message": "Starting service...",
    "timestamp": "2025-06-16T21:14:03+00:00"
  },
  {
    "level": "WARN",
    "message": "Disk space is running low",
    "timestamp": "2025-06-16T21:14:04+00:00"
  },
  {
    "level": "ERROR",
    "message": "Failed to connect to database",
    "timestamp": "2025-06-16T21:14:05+00:00"
  },
  {
    "level": "DEBUG",
    "message": "Retry attempt 1",
    "timestamp": "2025-06-16T21:14:06+00:00"
  },
  {
    "level": "INFO",
    "message": "Service started successfully",
    "timestamp": "2025-06-16T21:14:07+00:00"
  }
]
```