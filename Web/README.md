# Stayly web client

# Stayly ASP.NET Core MVC web

This project replaces the old .NET Framework web shell with an ASP.NET Core MVC application targeting .NET 8. It renders Razor views and consumes the existing `API` project through `HomestayApiClient`.

It includes:

- Destination search and stay cards from `GET /api/properties`
- Property and room details from `GET /api/properties/{id}`
- Guest registration and login
- Authenticated booking creation
- Authenticated trip history
- Responsive, mobile-friendly layout

## Run locally

1. Start the API from the `API` directory:

   ```powershell
   dotnet run --launch-profile http
   ```

2. Start the MVC project:

   ```powershell
   cd ..\Web\Web
   dotnet run --launch-profile http
   ```

3. Open `http://localhost:5280`.

The API URL is configured in `Web/Web/appsettings.json` under `HomestayApi:BaseUrl`. The MVC app keeps the API JWT in server-side session and provides property browsing, login, registration, room booking, and trip history.

