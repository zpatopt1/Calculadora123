# Stage 1: Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the project file and restore dependencies
COPY GoogleAuthDemo.csproj .
RUN dotnet restore

# Copy the remaining source code
COPY . .

# Build the application (publish to "out" directory)
RUN dotnet publish -c Release -o out

# Stage 2: Use the official .NET ASP.NET runtime image as the base for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published app from build environment
COPY --from=build-env /app/out .

# Expose port 80 (optional: specify the port your app listens on)
EXPOSE 8081

# Command to run the application
ENTRYPOINT ["dotnet", "GoogleAuthDemo.dll"]

