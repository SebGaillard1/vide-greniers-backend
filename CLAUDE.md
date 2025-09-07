# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VideGreniers is a .NET 9.0 web API for managing garage sales (vide-greniers) events. The project follows Clean Architecture principles with clear separation of concerns across multiple layers.

## Architecture

This solution uses **Clean Architecture** with the following layers:

- **VideGreniers.Domain**: Core business logic, entities, value objects, domain services, and specifications
- **VideGreniers.Application**: Application services, CQRS commands/queries, DTOs, and business orchestration using MediatR
- **VideGreniers.Infrastructure**: Data persistence, external services, authentication, caching, and cross-cutting concerns
- **VideGreniers.API**: REST API controllers, middleware, and presentation layer

### Key Patterns Used
- **CQRS with MediatR**: Commands and queries are separated with handlers
- **Domain-Driven Design**: Rich domain models with value objects (Email, PhoneNumber, Money, Location, etc.)
- **Repository Pattern**: Generic repository implementation with Unit of Work
- **Specification Pattern**: For complex queries and business rules
- **ErrorOr Pattern**: Functional error handling throughout the application

### Core Domain Entities
- **User**: User management with OAuth authentication (Google, Apple)
- **Event**: Garage sale events with location, date ranges, and organizer information
- **Favorite**: User favorites for events
- **Notification**: User notification system
- **UserActivity**: Activity tracking for analytics

## Common Development Commands

### Building the Solution
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build VideGreniers.API/VideGreniers.API.csproj

# Build in Release mode
dotnet build --configuration Release
```

### Running the Application
```bash
# Run the API (from API directory)
cd VideGreniers.API
dotnet run

# Or from solution root
dotnet run --project VideGreniers.API
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test tests/VideGreniers.Domain.Tests
dotnet test tests/VideGreniers.Application.Tests  
dotnet test tests/VideGreniers.Infrastructure.Tests
dotnet test tests/VideGreniers.API.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project VideGreniers.Infrastructure --startup-project VideGreniers.API

# Update database
dotnet ef database update --project VideGreniers.Infrastructure --startup-project VideGreniers.API

# Drop database
dotnet ef database drop --project VideGreniers.Infrastructure --startup-project VideGreniers.API
```

## Project Structure

### Domain Layer (`VideGreniers.Domain`)
- `Entities/`: Core business entities (User, Event, Favorite, etc.)
- `ValueObjects/`: Domain value objects (Email, Money, Location, Address, etc.)
- `Enums/`: Domain enumerations (EventStatus, UserRole, AuthProvider, etc.)
- `Services/`: Domain services for business logic
- `Specifications/`: Query specifications for complex filtering
- `Events/`: Domain events for cross-cutting concerns

### Application Layer (`VideGreniers.Application`)
- `Authentication/`: Login, register, OAuth commands and handlers
- `Events/`: Event CRUD operations, queries, and business logic
- `Users/`: User profile management and favorites
- `Notifications/`: Notification management
- `UserActivities/`: Activity tracking and statistics
- `Common/`: Shared DTOs, interfaces, behaviors, and mapping profiles

### Infrastructure Layer (`VideGreniers.Infrastructure`)
- `Persistence/`: Entity Framework configurations, repositories, DbContext
- `Identity/`: JWT token service, OAuth services (Google, Apple)
- `Services/`: External service implementations
- `Caching/`: Redis caching implementation
- `Migrations/`: EF Core database migrations

### API Layer (`VideGreniers.API`)
- `Controllers/`: REST API endpoints organized by feature
- `Middleware/`: Error handling, logging, caching middleware
- `Services/`: API-specific services (CurrentUserService)

## Key Technologies

- **.NET 9.0**: Latest .NET framework
- **Entity Framework Core**: ORM with PostgreSQL
- **MediatR**: CQRS implementation and request/response pipeline
- **Serilog**: Structured logging
- **JWT Authentication**: With refresh token support
- **OAuth Integration**: Google and Apple Sign-In
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Swagger/OpenAPI**: API documentation

## Authentication & Authorization

The application supports multiple authentication methods:
- Traditional email/password registration and login
- Google OAuth 2.0
- Apple Sign-In

JWT tokens are used with refresh token support. Access tokens expire after 15 minutes.

## Development Setup Requirements

1. .NET 9.0 SDK
2. PostgreSQL database
3. Redis (for caching)
4. Google Cloud Console project (for OAuth)
5. Apple Developer account (for Apple Sign-In)

Configuration details for OAuth setup are available in `OAUTH_SETUP.md`.

## API Documentation

Comprehensive API documentation with examples is available in `API_REFERENCE_iOS.md`. The API follows RESTful conventions with:
- Base URL: `http://localhost:5030` (development)
- Authentication: Bearer token
- Content-Type: `application/json`
- Structured error responses using ErrorOr pattern

## Working with the Codebase

### Adding New Features
1. Start with domain entities and value objects
2. Create application commands/queries with handlers
3. Add infrastructure implementations if needed
4. Expose through API controllers
5. Add comprehensive tests at each layer

### Code Conventions
- Use ErrorOr<T> for error handling instead of exceptions
- Follow CQRS pattern with separate command and query models
- Domain entities should be rich and encapsulate business rules
- Use specifications for complex query logic
- All public APIs should have proper validation and error handling

### Database Changes
- Always create migrations for schema changes
- Follow EF Core naming conventions
- Use configurations in Infrastructure/Persistence/Configurations/
- Seed initial data through ApplicationDbContextSeed