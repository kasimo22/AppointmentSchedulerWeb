# Use official .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file(s) and restore dependencies
COPY *.sln .
COPY AppointmentSchedulerWeb/*.csproj ./AppointmentSchedulerWeb/
RUN dotnet restore

# Copy the rest of the application files and build the project
COPY . .
WORKDIR /src/YourAppName
RUN dotnet publish -c Release -o /app/publish

# Use a lightweight runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AppointmentSchedulerWeb.dll"]
