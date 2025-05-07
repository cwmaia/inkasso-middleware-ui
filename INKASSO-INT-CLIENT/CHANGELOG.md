# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2025-05-07

### Added
- Raw SOAP client implementation for Inkasso integration
  - Manual construction of SOAP 1.2 envelopes with WS-Security headers
  - UsernameToken authentication with proper security timestamps
  - Direct HTTP communication without WCF dependencies
- Standalone console application for testing the raw SOAP client
- New `/test-raw-soap` endpoint in the web API
- Detailed request/response logging for debugging SOAP communication

### Changed
- Updated README.md with instructions for using the raw SOAP client

## [0.1.0] - 2024-05-06

### Added
- Initial release of the Inkasso Middleware UI
- React frontend with TypeScript and Tailwind CSS
  - Form for submitting claims with validation
  - Success/error message display
  - Responsive design with Tailwind CSS
- .NET 8 Minimal API backend
  - POST endpoint for claim submission
  - CORS configuration for local development
  - Console logging of received claims
- Project structure and documentation
  - README.md with setup instructions
  - CHANGELOG.md for version tracking
  - RELEASE_NOTES.md for detailed release information
