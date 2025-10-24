# =============================
# 🧱 Giai đoạn build
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ project vào container
COPY . .

# Khôi phục các package NuGet
RUN dotnet restore

# Build và publish ra thư mục /app (Release)
RUN dotnet publish CheckLiveBot.csproj -c Release -o /app

# =============================
# 🚀 Giai đoạn runtime
# =============================
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Copy từ giai đoạn build sang runtime
COPY --from=build /app .

# ⚠️ Đây là tên file DLL đúng với project của bạn
ENTRYPOINT ["dotnet", "CheckLiveBot.dll"]
