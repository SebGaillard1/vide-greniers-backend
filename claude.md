# CLAUDE.md - Vide-Greniers Project

## Project Overview
A .NET 9 backend API for a garage sale/flea market locator application with iOS Swift/SwiftUI frontend.
- Users can browse events without authentication
- Authentication required for favorites and creating events
- OAuth support (Sign in with Apple, Google)

## Architecture Rules

### Clean Architecture Structure
```
src/
├── VideGreniers.Domain/        # Core business logic, no dependencies
├── VideGreniers.Application/   # Use cases, CQRS handlers, interfaces
├── VideGreniers.Infrastructure/# External concerns (DB, Auth, APIs)
└── VideGreniers.API/          # Web API layer
```

### CQRS Pattern
- Use MediatR for all business operations
- Commands: Modify state (CreateEventCommand, AddToFavoritesCommand)
- Queries: Read state (GetNearbyEventsQuery, GetEventDetailsQuery)
- One handler per command/query
- Keep handlers thin, delegate to services

## Development Commands

### Build & Run
```bash
# Build solution
dotnet build

# Run API
dotnet run --project src/VideGreniers.API

# Run with watch mode (hot reload)
dotnet watch run --project src/VideGreniers.API

# Run tests
dotnet test

# Run specific test project
dotnet test tests/VideGreniers.Application.Tests
```

### Database Commands
```bash
# Add migration
dotnet ef migrations add MigrationName -p src/VideGreniers.Infrastructure -s src/VideGreniers.API

# Update database
dotnet ef database update -p src/VideGreniers.Infrastructure -s src/VideGreniers.API

# Remove last migration
dotnet ef migrations remove -p src/VideGreniers.Infrastructure -s src/VideGreniers.API
```

### Package Management
```bash
# Add package to specific project
dotnet add src/VideGreniers.Application package MediatR

# Update all packages
dotnet restore
```

### iOS Development Commands
```bash
# Run API for iOS development (HTTP only, no HTTPS redirection)
dotnet run --project src/VideGreniers.API --urls "http://localhost:5029"

# Or use the dedicated iOS development profile
dotnet run --project src/VideGreniers.API --launch-profile ios-dev
```

## Code Style Guidelines

### General C# Conventions
- Use file-scoped namespaces
- Prefer records for DTOs and Value Objects
- Use nullable reference types (`<Nullable>enable</Nullable>`)
- Follow Microsoft naming conventions
- One class/interface per file
- Order: Fields, Properties, Constructors, Methods

### Clean Architecture Rules
1. **Domain Layer**: No external dependencies, pure C#
2. **Application Layer**: Only depends on Domain
3. **Infrastructure Layer**: Implements Application interfaces
4. **API Layer**: Thin controllers, delegate to MediatR

### CQRS Conventions
```csharp
// Commands return Result<T> or Result
public sealed record CreateEventCommand(...) : IRequest<Result<Guid>>;

// Queries are immutable
public sealed record GetNearbyEventsQuery(
    double Latitude, 
    double Longitude, 
    int RadiusKm) : IRequest<Result<List<EventDto>>>;

// Handlers follow naming: [Command/Query]Handler
public sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<Guid>>
```

### Entity Framework Conventions
- Use fluent API for configuration (not attributes)
- Configuration in separate files: `EntityNameConfiguration.cs`
- Always use async methods
- Use IQueryable for filtering before materialization

## API Design Guidelines

### Endpoints Structure
```
GET    /api/events                 # Public - list events
GET    /api/events/{id}            # Public - event details
GET    /api/events/nearby          # Public - nearby events
POST   /api/events                 # Auth - create event
PUT    /api/events/{id}            # Auth - update event
DELETE /api/events/{id}            # Auth - delete event

GET    /api/users/favorites        # Auth - user's favorites
POST   /api/users/favorites/{id}   # Auth - add to favorites
DELETE /api/users/favorites/{id}   # Auth - remove from favorites

POST   /api/auth/register          # Register with email
POST   /api/auth/login             # Login with email
POST   /api/auth/oauth/google      # OAuth Google
POST   /api/auth/oauth/apple       # OAuth Apple
POST   /api/auth/refresh           # Refresh JWT token
```

### Response Format
```csharp
// Success response
{
    "data": { ... },
    "success": true
}

// Error response
{
    "errors": ["Error message"],
    "success": false
}
```

## Testing Guidelines

### Test Naming Convention
```csharp
[MethodName]_[Scenario]_[ExpectedBehavior]
// Example: CreateEvent_WithValidData_ReturnsSuccessResult
```

### Test Organization
- Unit tests for Domain logic
- Integration tests for Application handlers
- API tests for endpoints
- Use xUnit, FluentAssertions, Moq/NSubstitute

### Test Data
- Use Builder pattern for test objects
- Keep test data realistic
- Use in-memory database for integration tests

## Authentication & Authorization

### JWT Configuration
- Access token: 15 minutes expiry
- Refresh token: 7 days expiry
- Store refresh tokens in database
- Use asymmetric keys for production

### OAuth Providers
- Implement Apple Sign In (required for iOS)
- Implement Google Sign In
- Map external logins to local user accounts

## Performance Considerations

### Caching Strategy
- Cache nearby events queries (Redis, 5 min TTL)
- Cache event details (Redis, 10 min TTL)
- Invalidate cache on event updates

### Database Optimization
- Index on Location coordinates
- Index on Event date
- Composite index on UserId + EventId for favorites
- Use pagination for list queries

### Query Optimization
- Use projection (Select) to avoid over-fetching
- Use AsNoTracking() for read-only queries
- Implement specification pattern for complex queries

## Error Handling

### Global Exception Handler
- Log all exceptions with correlation ID
- Return consistent error format
- Don't expose internal details in production

### Validation
- Use FluentValidation for request validation
- Validate in Application layer
- Return detailed validation errors

## Security Requirements

### API Security
- Enable CORS for iOS app domain
- Implement rate limiting
- Use HTTPS only
- Validate all inputs
- Sanitize user-generated content

### Data Protection
- Hash passwords with BCrypt/Argon2
- Encrypt sensitive data at rest
- GDPR compliance for EU users
- Implement soft delete for user data

## Deployment Notes

### Environment Variables
```
ConnectionStrings__DefaultConnection
JWT__Secret
JWT__Issuer
JWT__Audience
OAuth__Google__ClientId
OAuth__Google__ClientSecret
OAuth__Apple__TeamId
OAuth__Apple__ClientId
OAuth__Apple__KeyId
Redis__ConnectionString
```

### Docker Support
- Multi-stage Dockerfile for optimized images
- Use .dockerignore
- Health check endpoint: /health

## Git Workflow

### Branch Naming
- feature/description
- bugfix/description
- hotfix/description

### Commit Messages
- Use conventional commits
- feat: new feature
- fix: bug fix
- refactor: code refactoring
- test: adding tests
- docs: documentation

## Common Issues & Solutions

### EF Core Migrations
- Always review generated migrations
- Test migrations on a copy of production data
- Use idempotent scripts for production

### DateTime Handling
- Store all dates in UTC
- Convert to user timezone in frontend
- Use DateTimeOffset for event dates

### Geolocation
- Use PostGIS extension for PostgreSQL
- Or use SQL Server geography types
- Calculate distances using Haversine formula

## Project Dependencies

### Essential NuGet Packages
```xml
<!-- Domain -->
<PackageReference Include="ErrorOr" />

<!-- Application -->
<PackageReference Include="MediatR" />
<PackageReference Include="FluentValidation" />
<PackageReference Include="AutoMapper" />

<!-- Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
<PackageReference Include="StackExchange.Redis" />

<!-- API -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" />
<PackageReference Include="Swashbuckle.AspNetCore" />
<PackageReference Include="Serilog.AspNetCore" />
```

## Development Priorities

1. Setup project structure with Clean Architecture
2. Implement basic CRUD for Events (no auth)
3. Add geolocation queries
4. Implement authentication (JWT + OAuth)
5. Add favorites functionality
6. Implement caching
7. Add comprehensive tests
8. Optimize performance
9. Prepare for deployment

## Notes for Claude Code

- Always follow Clean Architecture principles
- Create unit tests alongside new features
- Use Result pattern for error handling (ErrorOr package)
- Keep methods small and focused
- Document complex business logic
- Validate all user inputs
- Consider mobile app constraints (bandwidth, offline scenarios)
- Optimize for read-heavy workload (more browsing than creating)