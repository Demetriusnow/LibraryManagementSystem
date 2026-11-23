FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["LibraryManagementSystem.csproj", "./"]
COPY ["Library.Core/Library.Core.csproj", "Library.Core/"]
COPY ["Library.BLL/Library.BLL.csproj", "Library.BLL/"]
COPY ["Library.DAL/Library.DAL.csproj", "Library.DAL/"]
RUN dotnet restore "./LibraryManagementSystem.csproj"

COPY . .
RUN dotnet build "./LibraryManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LibraryManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibraryManagementSystem.dll"]
