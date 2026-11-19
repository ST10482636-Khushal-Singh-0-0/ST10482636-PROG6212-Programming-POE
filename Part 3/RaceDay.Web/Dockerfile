# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RaceDay.Web.csproj", "./"]
RUN dotnet restore "RaceDay.Web.csproj"

COPY . .
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]