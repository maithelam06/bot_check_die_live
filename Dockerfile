# Giai đoạn build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ project vào container
COPY . .

# Khôi phục gói NuGet
RUN dotnet restore

# Build và publish
RUN dotnet publish -c Release -o /app

# Giai đoạn runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

# Railway sẽ tự map cổng, nhưng bạn có thể set mặc định nếu cần
ENV ASPNETCORE_URLS=http://+:8080

# Chạy bot
ENTRYPOINT ["dotnet", "CheckLiveBot.dll"]
