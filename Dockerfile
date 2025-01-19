# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything into the container
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o out

# Use the runtime image for the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port your app runs on (default 80)
EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "AppointmentSchedulerWeb.dll"]
