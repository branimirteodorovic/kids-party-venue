# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/KidsPartyVenue.API/KidsPartyVenue.API.csproj", "src/KidsPartyVenue.API/"]
COPY ["src/KidsPartyVenue.Infrastructure/KidsPartyVenue.Infrastructure.csproj", "src/KidsPartyVenue.Infrastructure/"]
COPY ["src/KidsPartyVenue.Application/KidsPartyVenue.Application.csproj", "src/KidsPartyVenue.Application/"]
COPY ["src/KidsPartyVenue.Domain/KidsPartyVenue.Domain.csproj", "src/KidsPartyVenue.Domain/"]
COPY ["src/KidsPartyVenue.Presentation/KidsPartyVenue.Presentation.csproj", "src/KidsPartyVenue.Presentation/"]
RUN dotnet restore "./src/KidsPartyVenue.API/KidsPartyVenue.API.csproj"
COPY . .
WORKDIR "/src/src/KidsPartyVenue.API"
RUN dotnet build "./KidsPartyVenue.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KidsPartyVenue.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KidsPartyVenue.API.dll"]