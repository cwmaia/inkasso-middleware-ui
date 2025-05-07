# Inkasso Claim Submission Application

[![Version](https://img.shields.io/badge/version-0.2.0-blue.svg)](CHANGELOG.md)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![React](https://img.shields.io/badge/React-18.2.0-blue.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-4.9.5-blue.svg)](https://www.typescriptlang.org/)

This is a full-stack web application for submitting claims, built with React (TypeScript) and .NET 8.

## Prerequisites

- Node.js (LTS version)
- .NET 8 SDK
- npm or yarn
- VSCode with the following extensions:
  - Claude AI (with Agentic mode enabled)
  - Mistral Codestral for autocomplete

## Setup Instructions

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend-new
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```
   The frontend will run on http://localhost:3000

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend/InkassoMiddleware
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```
   The backend will run on http://localhost:5284

## AI Development Tools

This project is configured to use AI-assisted development tools to enhance productivity:

### Claude's Agentic Mode

Claude AI with Agentic mode enabled can help with:

- Code generation and modification
- Debugging and problem-solving
- Documentation creation
- Testing and validation

Configuration is provided in the `.claude` file at the project root.

### Mistral Codestral for Autocomplete

Mistral Codestral provides intelligent code completion for:

- C# backend development
- JavaScript/TypeScript frontend development
- API integration
- Component development

Configuration is provided in the `.mistral` file at the project root.

For detailed instructions on using these tools, see the README files in the `backend` and `frontend-new` directories.

## Features

- Form to submit claims with the following fields:
  - Claimant ID
  - Account Number
  - Capital
  - Client Reference
  - Due Date
  - Payor ID
  - Reference
- Real-time form validation
- Success/error message display
- CORS enabled for local development
- Backend logging of received claims

## Development

- Frontend: React + TypeScript + Tailwind CSS
- Backend: .NET 8 Minimal API
- Communication: JSON over HTTP

## Project Structure

- `backend/`: Contains the .NET 8 backend application
- `frontend-new/`: Contains the React frontend application (in development)
- `.claude`: Configuration for Claude AI
- `.mistral`: Configuration for Mistral Codestral

## Raw SOAP Integration (v0.2.0)

This version includes a raw SOAP client implementation for direct integration with the Inkasso SOAP service, bypassing WCF complexity.

### Features

- Manual construction of SOAP 1.2 envelopes with WS-Security headers
- UsernameToken authentication with proper security timestamps
- Direct HTTP communication without WCF dependencies
- Detailed request/response logging for debugging

### Running the Tests

#### Console Test

To run the standalone console test:

```bash
cd backend/InkassoConsoleTest
dotnet run
```

This will execute a test query against the Inkasso SOAP service and display the full request and response.

#### Web API Test

To test through the web API:

1. Start the backend server:
   ```bash
   cd backend/InkassoMiddleware
   dotnet run
   ```

2. Access the test endpoint in your browser:
   ```
   http://localhost:5284/test-raw-soap
   ```

This endpoint will execute the raw SOAP client and return the results as JSON.

## Versioning

This project follows [Semantic Versioning](https://semver.org/). For the versions available, see the [CHANGELOG.md](CHANGELOG.md) file.

For detailed information about each release, see the [RELEASE_NOTES.md](RELEASE_NOTES.md) file.

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
