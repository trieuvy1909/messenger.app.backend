FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MessengerApplication/MessengerApplication.csproj", "MessengerApplication/"]
RUN dotnet restore "MessengerApplication/MessengerApplication.csproj"
COPY . .
WORKDIR "/src/MessengerApplication"
RUN dotnet build "MessengerApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 3000

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MessengerApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessengerApplication.dll"]