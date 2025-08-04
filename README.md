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

## Pattern

You can specify a custom Grok pattern for parsing logs using the `-p` option:

```shell
analog input.log -p "%{TIMESTAMP_ISO8601:timestamp} %{LOGLEVEL:level} %{GREEDYDATA:message}" -o output.json
```

This allows you to define how log lines are parsed, enabling support for different log formats. See [Grok patterns documentation](https://github.com/logstash-plugins/logstash-filter-grok) for details.

If you do not specify a custom pattern, Analog uses the following default Grok pattern to parse log lines:

```
[%{TIMESTAMP_ISO8601:timestamp}] [%{LOGLEVEL:level}] %{GREEDYDATA:message}
```

This pattern extracts the timestamp, log level, and message from log entries in the following format:

```
[2025-06-16T21:14:03Z] [INFO] Starting service...
```

## Filter

Analog supports powerful filtering capabilities to process only the log entries you need. You can filter by log levels,
timestamps, content, and combine multiple conditions.

### Supported Operators

- **Comparison**: `=`, `>`, `<`
- **String matching**: `~` (contains), `^` (starts with), `$` (ends with)
- **Logical**: `&` (and), `|` (or), `!` (not)
- **Grouping**: Use parentheses `()` for complex expressions

### Examples

#### Filter by log level:

```shell
analog input.log -f "level = 'ERROR'" -o errors.json
```

#### Filter by timestamp range:

```shell
analog input.log -f "timestamp > '2025-06-16T21:14:04Z'" -o recent.json
```

#### Search for specific content in messages:

```shell
analog input.log -f "message ~ 'database'" -o database-logs.json
```

#### Check if messages start or end with specific text:

```shell
analog input.log -f "message ^ 'Starting'" -o startup-logs.json
analog input.log -f "message $ 'successfully'" -o success-logs.json
```

#### Use logical operators to combine multiple conditions:

```shell
# Get ERROR or WARN level logs
analog input.log -f "level = 'ERROR' | level = 'WARN'" -o alerts.json

# Get INFO logs containing 'service'
analog input.log -f "level = 'INFO' & message ~ 'service'" -o service-info.json

# Get all logs except DEBUG level
analog input.log -f "! level = 'DEBUG'" -o production.json
```

#### Filter for recent errors:

```shell
analog input.log -f "level = 'ERROR' & timestamp > '2025-06-16T21:14:04Z'" -o recent-errors.json
```

#### Complex filtering with grouping:

```shell
analog input.log -f "(level = 'ERROR' | level = 'WARN') & message ~ 'database'" -o database-issues.json
```