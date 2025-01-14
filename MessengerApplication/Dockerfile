# Use the .NET SDK 9.0 image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file into the container (ensure the correct location of csproj file)
COPY MessengerApplication.csproj .

# Restore any dependencies (via 'dotnet restore')
RUN dotnet restore "MessengerApplication.csproj"

# Copy the entire project into the container
COPY . .

# Set the working directory to the folder containing the project file
WORKDIR "/src"

# Build the project
RUN dotnet build "MessengerApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Use the .NET runtime 9.0 image for the base stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 3000

# Publish the application with Release configuration
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MessengerApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the .NET runtime image for the final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessengerApplication.dll"]
