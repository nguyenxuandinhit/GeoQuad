# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY GeoQuad.Web.csproj ./
RUN dotnet restore GeoQuad.Web.csproj
COPY . ./
RUN dotnet publish GeoQuad.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Chạy
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GeoQuad.Web.dll"]
