# 🤖 CheckLiveBot - Facebook UID Tracker

Bot Telegram chuyên nghiệp để theo dõi trạng thái LIVE/DEAD của Facebook UID. Giúp bạn quản lý và giám sát danh sách UID Facebook một cách hiệu quả.

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![Telegram Bot](https://img.shields.io/badge/Telegram-Bot-blue)
![SQLite](https://img.shields.io/badge/Database-SQLite-green)
![C#](https://img.shields.io/badge/Language-C%23-orange)

## 🎯 Tổng quan

**CheckLiveBot** là bot Telegram được phát triển bằng C# (.NET 8) với kiến trúc modular, giúp người dùng:

- ✅ **Theo dõi trạng thái**: Kiểm tra LIVE/DEAD của Facebook UID real-time
- 📊 **Quản lý danh sách**: Thêm UID với ghi chú và giá cả, xem danh sách theo dõi
- 📈 **Thống kê chi tiết**: Xem số liệu thống kê và lịch sử kiểm tra
- 👥 **Quản lý người dùng**: Hệ thống trial 5 ngày, phân quyền truy cập
- 🛡️ **Bảo mật**: Xử lý lỗi toàn diện, rate limiting, logging chi tiết

## 🚀 Tính năng chính

### 🔍 Quản lý UID Facebook
- **Thêm UID đơn lẻ**: `/add 100012345678901 VIP khách | 50000`
- **Thêm danh sách UID**: Import nhiều UID cùng lúc
- **Xem danh sách**: Hiển thị tất cả UID đang theo dõi với trạng thái LIVE/DEAD
- **Kiểm tra real-time**: Sử dụng Facebook Graph API để kiểm tra ngay lập tức

### 📊 Thống kê và báo cáo
- Tổng số UID đang theo dõi
- Thông tin thời hạn sử dụng bot
- Lịch sử kiểm tra gần đây
- Menu tương tác với Inline Keyboard

### 👤 Hệ thống người dùng
- **Trial**: Người dùng mới được 5 ngày dùng thử
- **Authorization**: Kiểm tra thời hạn trước mỗi lệnh
- **Auto-creation**: Tự động tạo user khi lần đầu sử dụng

## 🏗️ Cấu trúc dự án

```
CheckLiveBot/
├── 📁 Database/
│   └── AppDbContext.cs              # Entity Framework DbContext
├── 📁 Handlers/
│   ├── MessageHandler.cs            # Xử lý tin nhắn từ user
│   ├── CallbackQueryHandler.cs      # Xử lý button callbacks
│   └── CommandHandler.cs            # Các command handlers
├── 📁 Models/
│   ├── User.cs                      # User model
│   └── TrackedUid.cs               # TrackedUid model
├── 📁 Services/
│   ├── DatabaseService.cs          # Database operations
│   └── AuthorizationService.cs     # User authorization
├── 📁 Utils/
│   └── CheckLiveUid.cs             # Facebook UID checker
├── 📄 Program.cs                   # Main entry point
├── 📄 CheckLiveBot.csproj          # Project file
└── 📄 README.md                    # This file
```

### 🔧 Kiến trúc Component

#### **Core Services**
- **DatabaseService**: Quản lý tất cả operations với SQLite database
- **AuthorizationService**: Xử lý phân quyền và kiểm tra thời hạn user
- **CheckLiveUid**: Kiểm tra trạng thái Facebook UID qua Graph API

#### **Handlers Pattern**
- **MessageHandler**: Xử lý tin nhắn text từ người dùng
- **CallbackQueryHandler**: Xử lý tương tác với Inline Keyboard
- **CommandHandler**: Tập hợp các static methods cho commands

#### **Data Models**
- **User**: Quản lý thông tin người dùng Telegram (ID, username, thời hạn)
- **TrackedUid**: Lưu trữ UID Facebook được theo dõi (UID, ghi chú, giá, trạng thái)

### ⚙️ Công nghệ Stack

| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|----------|
| **.NET** | 8.0 | Framework chính |
| **C#** | 12.0 | Ngôn ngữ lập trình |
| **Telegram.Bot** | 22.6.1-dev.2 | Telegram Bot API |
| **Entity Framework Core** | 9.0.8 | ORM cho database |
| **SQLite** | Latest | Database nhẹ |

### 🔍 Cách hoạt động

**Kiểm tra Facebook UID:**
```csharp
// Sử dụng Facebook Graph API
var response = await httpClient.GetAsync($"https://graph.facebook.com/{uid}/picture?redirect=false");
var result = await response.Content.ReadAsStringAsync();

// UID LIVE nếu response chứa "height" và "width"
return result.Contains("height") && result.Contains("width") ? "live" : "die";
```

**User Flow:**
1. User gửi `/start` → Hiển thị menu chính
2. Chọn "Thêm UID" → Nhập theo format `/add UID ghi_chú | giá`
3. Bot kiểm tra UID qua Facebook API
4. Lưu vào database với trạng thái LIVE/DEAD
5. User có thể xem danh sách, thống kê, lịch sử

## 📞 Liên hệ

### 👨‍💻 Developer


### 🆘 Support
Nếu bạn cần hỗ trợ:


### 💼 Business Inquiry


---

<div align="center">


*Nếu project này hữu ích với bạn, hãy cho một ⭐ star nhé!*

</div>
