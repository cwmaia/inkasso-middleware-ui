# Inkasso Claim Submission Application

[![Version](https://img.shields.io/badge/version-0.1.0-blue.svg)](CHANGELOG.md)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![React](https://img.shields.io/badge/React-18.2.0-blue.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-4.9.5-blue.svg)](https://www.typescriptlang.org/)

This is a full-stack web application for submitting claims, built with React (TypeScript) and .NET 8.

## Prerequisites

- Node.js (LTS version)
- .NET 8 SDK
- npm or yarn

## Setup Instructions

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
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

## Versioning

This project follows [Semantic Versioning](https://semver.org/). For the versions available, see the [CHANGELOG.md](CHANGELOG.md) file.

For detailed information about each release, see the [RELEASE_NOTES.md](RELEASE_NOTES.md) file.

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request 