# Use the official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore

# Build the application in Release mode
RUN dotnet publish -c Release -o out

# Use the runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port and start the application
EXPOSE 80
ENTRYPOINT ["dotnet", "AppointmentSchedulerWeb.dll"]
