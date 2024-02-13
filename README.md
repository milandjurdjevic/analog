# Analog - Log Analysis

Powerful utility designed to help you make sense of your application and system logs. Whether
you're troubleshooting issues, monitoring performance, or auditing security events, this tool provides the capabilities
you need to efficiently analyze log files.

![preview](./img/preview.png)

## Features

- **Templates**: easily parse logs from various sources using built-in or custom log templates.
  - `analog.cli test.log -t|template Default`
- **Query**: query logs using flexible syntax.
  - filter: `analog.cli test.log -f|--filter 'Severity eq "ERR"'`
  - sort: `analog.cli test.log -s|--sortby Severity`
## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for more details.
