# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2025-05-07

### Added
- Mock SOAP endpoint for Inkasso integration testing
  - New `/mock-inkasso` endpoint that returns a fixed SOAP 1.2 response
  - Support for Content-Type: application/soap+xml
  - Fixed response with sample claim data
- Enhanced InkassoRawSoapClient with mock support
  - JSON mock endpoint for direct mock data access
  - JSON to SOAP conversion for seamless integration
  - Robust parsing with fallback mechanisms
- New `/mock-inkasso-json` endpoint for direct JSON access to mock data

### Changed
- Updated InkassoRawSoapClient to support environment variable configuration for mock testing
- Improved error handling and logging in SOAP response parsing

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
