# Use the .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY AppointmentSchedulerWeb.csproj ./
RUN dotnet restore AppointmentSchedulerWeb.csproj

# Copy the rest of the files and publish the main project
COPY . ./
RUN dotnet publish AppointmentSchedulerWeb.csproj -c Release -o out

# Use the runtime image for .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port and start the application
EXPOSE 80
ENTRYPOINT ["dotnet", "AppointmentSchedulerWeb.dll"]
