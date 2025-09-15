# Copilot Instructions for Analog

CLI for parsing heterogeneous log lines into structured JSON with optional filtering.

## Architecture
- Use Onion architecture.
- Use pipeline model to implement workflows.
- Business logic makes decisions.
- Keep Business logic and IO separate.
- Keep IO at edges of the application.

## Testing
- Only unit test the business logic.
- IO is tested with integration tests (Out of Scope).
- Use Expecto framework for unit tests.