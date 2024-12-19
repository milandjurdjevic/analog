# Analog (Experimental Phase)

An efficient tool designed for rapid log analysis, enabling users to quickly parse, filter, and interpret log data.
Equipped with powerful search and visualization features, it provides immediate insights into system events, errors, and
performance metrics, streamlining troubleshooting and decision-making processes. Ideal for developers, system
administrators, and IT professionals seeking to improve operational efficiency and reduce downtime.

## Features

- [ ] **Templates**: Create custom templates to extract log data from stream.
- [ ] **Filtering**: Filter logs based on specific criteria.

## Getting Started

### Basic Usage

```bash
analog /path/to/logfile.log
```

_NOTE: If a template is not specified, the default one will be used._

### Using Specific Template (Preview)

```bash
analog /path/to/logfile.log -t custom_template
```

### Apply Filter (Preview)

```bash
analog /path/to/logfile.log -f "Severity = 'ERR' | Severity = 'WRN'"
```
