Here's the improved `README.md` file, incorporating the new content while maintaining the existing structure and information:

# Homestay API

A .NET 8 booking API for a homestay system. The API uses ASP.NET Core, Entity Framework Core, SQL Server, JWT bearer authentication, and Swagger.

## Requirements

- .NET 8 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022 or the .NET CLI
- Git

## Clone and restore

To get started, clone the repository and restore the dependencies:

git clone https://github.com/ntruongg/Homestay.git
cd Homestay\API
dotnet restore

## Configure the database

Before running the application, you need to configure the database connection. Update the `ConnectionStrings:DefaultConnection` value in `appsettings.json`, or configure it with User Secrets:

dotnet user-secrets init --project API.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HOMESTAY_DB;Trusted_Connection=True;TrustServerCertificate=True;" --project API.csproj

If you need to use a SQL login instead of Windows authentication, use the following command:

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HOMESTAY_DB;User Id=<username>;Password=<password>;TrustServerCertificate=True;" --project API.csproj

**Important:** Do not commit passwords or production connection strings.

## Configure the JWT signing key

The API reads the JWT signing key from the configuration path `Jwt:Key`. To set this up using ASP.NET Core User Secrets, run the following commands from the `API` directory:

dotnet user-secrets init --project API.csproj

$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$key = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:Key" $key --project API.csproj

To verify that the secret exists, you can list the user secrets:

dotnet user-secrets list --project API.csproj

User Secrets are stored outside the repository and are not committed to Git. Ensure that the secret is different for development, testing, and production environments.

### Environment variable alternative

You can also set the JWT signing key using an environment variable. The equivalent environment variable is `Jwt__Key` because ASP.NET Core maps `__` to `:`:

$env:Jwt__Key = $key
dotnet run

To persist it for the current Windows user, use:

[Environment]::SetEnvironmentVariable("Jwt__Key", $key, "User")

Remember to restart Visual Studio or the terminal after changing a persistent environment variable. For production, configure `Jwt__Key` using the hosting platform's secret-management system. Never commit the JWT key to `appsettings.json`, `appsettings.Development.json`, source code, or Git.

## Apply database migrations

The project contains an initial EF Core migration. To apply migrations, run the following command from the `API` directory:

dotnet ef database update --project API.csproj

If the EF command is not installed, you can install it globally:

dotnet tool install --global dotnet-ef

Note that the application currently attempts to apply migrations at startup. For production deployments, it is recommended to use a controlled migration step instead of relying on application startup migrations.

## Run the API

To start the API, use the following command:

dotnet run

The API URL will be displayed in the terminal. In Development, Swagger is available at:

https://localhost:<port>/swagger


## Authentication flow

1. **Register a guest account:**

   POST /api/auth/register
   Content-Type: application/json

   {
     "userName": "guest1",
     "fullName": "Guest One",
     "phone": "0900000000",
     "email": "guest@example.com",
     "password": "Use-a-strong-password-123!",
     "role": "GUEST",
     "address": "Da Nang"
   }

2. **Log in:**

   POST /api/auth/login
   Content-Type: application/json

   {
     "userNameOrEmail": "guest1",
     "password": "Use-a-strong-password-123!"
   }

3. **Copy the returned `accessToken`.**
4. In Swagger, select **Authorize** and enter:

   Bearer <accessToken>

The token is required for guest booking endpoints. Passwords are hashed before storage and must never be stored or logged as plain text.

## Main endpoints

### Authentication

- `POST /api/auth/register`
- `POST /api/auth/login`

### Public properties

- `GET /api/properties`
- `GET /api/properties/{id}`

### Authenticated guest bookings

- `POST /api/bookings`
- `GET /api/bookings`
- `GET /api/bookings/{id}`
- `PUT /api/bookings/{id}/cancel`

## Example booking request

To create a booking, send the following request:

POST /api/bookings
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "roomId": 1,
  "checkIn": "2026-10-01T00:00:00Z",
  "checkOut": "2026-10-05T00:00:00Z",
  "guestCount": 2
}

The API validates the date range, room capacity, existing bookings, unavailable calendar dates, and calculates the total price on the server.

## Project structure

The project is organized as follows:

API/
??? Controllers/       HTTP API endpoints
??? Data/              EF Core DbContext
??? DTOs/              API request and response models
??? Models/            Database entities
??? Services/          JWT and application services
??? Migrations/        EF Core migrations
??? Program.cs         Application configuration
??? appsettings*.json  Non-secret configuration

## Development guidelines

- Do not expose EF Core entities directly from controllers.
- Do not store passwords or JWT keys in source control.
- Keep authorization checks in the API; do not trust the MVC frontend.
- Use DTOs for all API requests and responses.
- Run `dotnet build` before creating a pull request.
- Add migrations when the EF Core model changes.

This revised README maintains the original structure while enhancing clarity and coherence, ensuring that users can easily follow the instructions and understand the project's functionality.