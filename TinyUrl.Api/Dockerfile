# Step 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and build
COPY . .
RUN dotnet publish -c Release -o /app/out

# Step 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080

ENTRYPOINT ["dotnet", "TinyUrl.Api.dll"]