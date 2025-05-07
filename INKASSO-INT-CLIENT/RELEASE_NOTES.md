# Release Notes

## Version 0.3.0 (2025-05-07)

### Overview
Added mock SOAP endpoint for Inkasso integration testing, allowing continued development and testing even when the real Inkasso service is unavailable.

### New Features
- **Mock SOAP Endpoint**:
  - New `/mock-inkasso` endpoint that returns a fixed SOAP 1.2 response
  - Accepts any SOAP envelope and always returns the same response
  - Content-Type: application/soap+xml
  - Fixed response with sample claim data
  - Status code 200

- **Enhanced InkassoRawSoapClient**:
  - Support for environment variable configuration for mock testing
  - JSON mock endpoint for direct mock data access
  - JSON to SOAP conversion for seamless integration
  - Robust parsing with fallback mechanisms

- **Additional Endpoints**:
  - New `/mock-inkasso-json` endpoint for direct JSON access to mock data
  - Enhanced `/test-raw-soap` endpoint with `useMock` parameter

### Technical Details
- Mock endpoint runs on http://localhost:5284/mock-inkasso
- JSON endpoint runs on http://localhost:5284/mock-inkasso-json
- To use the mock endpoint, set the INKASSO_URL environment variable:
  ```csharp
  Environment.SetEnvironmentVariable("INKASSO_URL", "http://localhost:5284/mock-inkasso");
  ```

### Usage Instructions
1. Start the backend:
   ```bash
   cd backend/InkassoMiddleware
   dotnet run
   ```

2. Test the mock endpoint:
   ```bash
   curl -X POST -H "Content-Type: application/soap+xml" -d "<soap:Envelope xmlns:soap='http://www.w3.org/2003/05/soap-envelope'><soap:Body><test>Test</test></soap:Body></soap:Envelope>" "http://localhost:5284/mock-inkasso"
   ```

3. Test the JSON endpoint:
   ```bash
   curl -X GET "http://localhost:5284/mock-inkasso-json"
   ```

4. Test with the raw SOAP client:
   ```bash
   curl -X GET "http://localhost:5284/test-raw-soap?useMock=true"
   ```

## Version 0.2.0 (2025-05-07)

### Overview
Added raw SOAP client implementation for Inkasso integration, providing a more direct and controllable approach to SOAP communication.

### New Features
- **Raw SOAP Client**:
  - Manual construction of SOAP 1.2 envelopes with WS-Security headers
  - UsernameToken authentication with proper security timestamps
  - Direct HTTP communication without WCF dependencies
  - Detailed request/response logging for debugging

- **Testing Tools**:
  - Standalone console application for testing the raw SOAP client
  - New `/test-raw-soap` endpoint in the web API

### Technical Details
- SOAP 1.2 compliant with proper WS-Security headers
- UsernameToken authentication with username/password
- Security timestamps for request validity
- XML parsing and formatting utilities

### Usage Instructions
1. Using the raw SOAP client:
   ```csharp
   string response = await InkassoRawSoapClient.QueryClaimsAsync(claimantId, fromDate, toDate);
   var claims = InkassoRawSoapClient.ParseClaimsFromSoapResponse(response);
   ```

2. Testing with the web API:
   ```bash
   curl -X GET "http://localhost:5284/test-raw-soap"
   ```

## Version 0.1.0 (2024-05-06)

### Overview
Initial release of the Inkasso Middleware UI, providing a full-stack solution for claim submission with a modern React frontend and .NET 8 backend.

### Frontend Features
- **React + TypeScript**: Modern frontend development with type safety
- **Tailwind CSS**: Responsive and modern UI design
- **Form Features**:
  - Input validation for all fields
  - Real-time feedback
  - Success/error message display
  - Responsive layout
- **Fields**:
  - Claimant ID
  - Account Number
  - Capital
  - Client Reference
  - Due Date
  - Payor ID
  - Reference

### Backend Features
- **.NET 8 Minimal API**: Lightweight and fast backend
- **Endpoints**:
  - POST `/create-claim`: Accepts claim data and logs to console
- **CORS**: Configured for local development
- **Logging**: Console output of received claims

### Technical Details
- Frontend runs on http://localhost:3000
- Backend runs on http://localhost:5284
- CORS configured to allow frontend-backend communication
- TypeScript interfaces for type safety
- Tailwind CSS for styling

### Setup Instructions
1. Frontend:
   ```bash
   cd frontend
   npm install
   npm start
   ```

2. Backend:
   ```bash
   cd backend/InkassoMiddleware
   dotnet restore
   dotnet run
   ```

### Future Improvements
- Database integration
- Authentication
- Additional validation rules
- Error handling improvements
- Unit tests
- CI/CD pipeline
