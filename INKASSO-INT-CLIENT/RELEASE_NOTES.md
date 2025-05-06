# Release Notes

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