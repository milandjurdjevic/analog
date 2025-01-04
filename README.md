# Analog (Experimental Phase)

An efficient tool designed for rapid log analysis, enabling users to quickly parse, filter, and interpret log data.
Equipped with powerful search and visualization features, it provides immediate insights into system events, errors, and
performance metrics, streamlining troubleshooting and decision-making processes. Ideal for developers, system
administrators, and IT professionals seeking to improve operational efficiency and reduce downtime.


## Getting Started

### Parse log file using the default pattern

```bash
analog /path/to/logfile.log
```

### Parse log file using a custom pattern

```bash
analog /path/to/logfile.log -p "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}"
```

### Filter log entries

```bash
analog /path/to/logfile.log -f "loglevel = 'ERROR' | loglevel = 'WARNING'"
```
