# Contact API

A lightweight ASP.NET Core minimal API for managing a simple contact directory. The service stores contacts in SQLite and exposes a small REST API for creating, reading, updating, and deleting contact records.

## Design pattern

This project follows a clean, resource-oriented minimal API design:

- `Program.cs` wires up the ASP.NET Core application, dependency injection, database access, health checks, and endpoint registration.
- `ContactEndpoints` contains the HTTP route handlers and keeps the API surface explicit and easy to reason about.
- `Domain/Contact.cs` defines the core entity.
- `Contracts/ContactDtos.cs` defines the request and response DTOs used by the API.
- `Data/AppDbContext.cs` is the EF Core database context used for persistence.

This is a thin CRUD architecture rather than a full layered service/repository abstraction. It keeps the project simple while still separating concerns between HTTP contracts, domain model, and persistence.

## Use cases

This service is designed for lightweight contact management scenarios such as:

- Personal or team address books
- Simple customer/contact lookups in internal tools
- A backend for a small web or mobile app
- A demo or starter project for ASP.NET Core + EF Core + SQLite

The schema enforces a unique phone number per contact, which makes it suitable for scenarios where phone values are used as a stable identity or lookup key.

## Project structure

- `src/ContactApi/Program.cs` — application startup and route registration
- `src/ContactApi/Endpoints/ContactEndpoints.cs` — CRUD endpoint definitions
- `src/ContactApi/Domain/Contact.cs` — contact entity
- `src/ContactApi/Contracts/ContactDtos.cs` — request/response models
- `src/ContactApi/Data/AppDbContext.cs` — EF Core context and model configuration
- `src/ContactApi/Migrations/` — SQLite schema migrations
- `deploy/docker/Dockerfile` — container build for runtime and migration bundle

## API contract

Base path: `/api/v1/contacts`

### List contacts

- Method: `GET`
- Route: `/api/v1/contacts/`
- Response: `200 OK` with an array of contact objects

### Get a single contact

- Method: `GET`
- Route: `/api/v1/contacts/{id}`
- Response: `200 OK` with a single contact object
- Error: `404 Problem Details` when the ID does not exist

### Create a contact

- Method: `POST`
- Route: `/api/v1/contacts/`
- Body:

```json
{
  "name": "Jane Doe",
  "phoneNumber": "+1-555-0100",
  "email": "jane@example.com"
}
```

- Required fields: `name`, `phoneNumber`
- Optional field: `email`
- Response: `201 Created` with the created contact and a `Location` header pointing to the new resource

### Update a contact

- Method: `PUT`
- Route: `/api/v1/contacts/{id}`
- Body:

```json
{
  "name": "Jane Smith",
  "phoneNumber": "+1-555-0101",
  "email": "jane.smith@example.com"
}
```

- Response: `200 OK` with the updated contact
- Error: `404 Problem Details` when the contact does not exist

### Delete a contact

- Method: `DELETE`
- Route: `/api/v1/contacts/{id}`
- Response: `204 No Content` when the deletion succeeds
- Error: `404 Problem Details` when the contact does not exist

### Health check

- Route: `/healthz`
- Response: `200 OK` when the service is running

### OpenAPI

In development, the app exposes OpenAPI metadata for the API surface.

## Data model

A contact contains the following fields:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Jane Doe",
  "phoneNumber": "+1-555-0100",
  "email": "jane@example.com",
  "createdAt": "2026-10-07T01:23:24.000Z",
  "updatedAt": "2026-10-07T01:23:25.000Z"
}
```

Notes:

- `id` is a GUID generated with `Guid.CreateVersion7()`
- `name` and `phoneNumber` are required
- `email` is optional
- `createdAt` is set at creation time
- `updatedAt` is set when a contact is updated

## Local development

The service requires a connection string for SQLite. Configure it before running:

```bash
export ConnectionStrings__Default="Data Source=contacts.db"
dotnet restore src/ContactApi/ContactApi.csproj
dotnet run --project src/ContactApi
```

The app runs on the default ASP.NET Core development port (`http://localhost:5000` or the configured Kestrel port) unless you override it through environment variables or launch settings.

## Docker

The repository includes a Dockerfile that builds a production image and creates a SQLite database in `/data/contacts.db`.

```bash
docker build -f deploy/docker/Dockerfile -t contact-api .
docker run -p 8080:8080 contact-api
```

The runtime image exposes port `8080` and performs a health check against `/healthz`.

## Example requests

Create a contact:

```bash
curl -X POST http://localhost:5000/api/v1/contacts/ \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Doe",
    "phoneNumber": "+1-555-0100",
    "email": "jane@example.com"
  }'
```

List contacts:

```bash
curl http://localhost:5000/api/v1/contacts/
```

Get one contact:

```bash
curl http://localhost:5000/api/v1/contacts/{id}
```

Delete a contact:

```bash
curl -X DELETE http://localhost:5000/api/v1/contacts/{id}
```

## Notes

- The app uses EF Core migrations to manage the SQLite schema.
- The database is intentionally simple and compact, making the project well-suited as a starter service or demo backend.
- The design prioritizes simplicity and clarity over enterprise abstraction layers.
