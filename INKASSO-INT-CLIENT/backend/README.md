# Inkasso Middleware UI - Backend

This directory contains the backend application for the Inkasso Middleware UI project, built with ASP.NET Core.

## Development Setup

### Prerequisites

- .NET SDK 8.0 or later
- Visual Studio or VSCode with C# extensions
- VSCode with the following AI extensions:
  - Claude AI (with Agentic mode enabled)
  - Mistral Codestral

## Using AI Assistants for Development

### Claude's Agentic Mode

Claude can help with:

1. **API Development**:
   - Generate new endpoint implementations
   - Create or modify DTOs and models
   - Implement validation logic

2. **SOAP Integration**:
   - Analyze and improve SOAP client code
   - Debug integration issues
   - Generate mapping code between SOAP and REST models

3. **Testing**:
   - Generate unit tests
   - Create test data
   - Implement integration tests for API endpoints

To use Claude:
- Open the Command Palette (Ctrl+Shift+P / Cmd+Shift+P)
- Type "Claude: Ask Claude" and describe your task
- For file operations, enable Agentic mode

### Mistral Codestral

Mistral Codestral provides intelligent code completion for:

1. **C# Development**:
   - Autocomplete for classes, methods, and properties
   - Suggestions for ASP.NET Core patterns
   - Type inference and validation

2. **API Development**:
   - Autocomplete for endpoint definitions
   - Suggestions for HTTP methods and status codes
   - Completion for dependency injection

Mistral Codestral is automatically active while coding and will provide suggestions as you type.

## Project Structure

- `InkassoMiddleware/`: Main ASP.NET Core application
  - `Models/`: Data transfer objects and domain models
  - `ServiceReference/`: Generated SOAP client code
  - `Program.cs`: Application entry point and configuration

## API Endpoints

The backend provides these endpoints:

- `POST /create-claim`: Create a new banking claim
- `POST /query-claims`: Query existing claims
- `GET /test-connection`: Test connection to the Inkasso API
- `GET /weatherforecast`: Sample endpoint (can be removed in production)

## SOAP Integration

The backend integrates with the Icelandic Online Banking Claims SOAP service. The SOAP client is configured in `Program.cs` and used in the `/query-claims` endpoint.
