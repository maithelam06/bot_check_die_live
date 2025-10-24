# =============================
# 🧱 Build stage
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy tất cả file
COPY . .

# Restore packages
RUN dotnet restore ./CheckLiveBot.csproj

# Publish
RUN dotnet publish ./CheckLiveBot.csproj -c Release -o /app

# =============================
# 🚀 Runtime stage
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render yêu cầu port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CheckLiveBot.dll"]
