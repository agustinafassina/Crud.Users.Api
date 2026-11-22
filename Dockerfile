FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY UsersApi.csproj ./
RUN dotnet restore UsersApi.csproj

COPY . .
RUN dotnet publish UsersApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8 images listen on 8080 by default
EXPOSE 8080
ENTRYPOINT ["dotnet", "UsersApi.dll"]
