# =============================
# 🧱 Build stage
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish CheckLiveBot.csproj -c Release -o /app

# =============================
# 🚀 Runtime stage
# =============================
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

# Fake port để Render giữ container sống
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CheckLiveBot.dll"]
