# 🕵️ THE UNRAVELLER - BACKEND SYSTEM

Hệ thống BackEnd cho dự án **The Unraveller**, được xây dựng trên nền tảng .NET 9 với kiến trúc 3 lớp (3-Layer Architecture) nhằm tối ưu hóa khả năng mở rộng và bảo trì.

## 🛠 Công Nghệ Sử Dụng (Tech Stack)

- **Language:** C#
- **Framework:** .NET 9 Web API
- **Database:** SQLite (Dễ dàng triển khai và di động)
- **ORM:** Entity Framework Core (EF Core)
- **API Documentation:** Swagger UI / Swashbuckle
- **Pattern:** Repository Pattern & Dependency Injection

## 🏗 Kiến Trúc Dự Án (Project Structure)

Dự án được chia thành 4 project thành phần theo chuẩn 3 lớp:

1.  **TheUnraveller.Core:**
    - Chứa các thực thể chính (**Entities**): `User`, `Npc`, `Mission`, `Dialogue`, `UserProgress`.
    - Định nghĩa các **Interfaces** cho Repository và Service.
2.  **TheUnraveller.Infrastructure:**
    - Quản lý cơ sở dữ liệu (**AppDbContext**).
    - Triển khai thực tế các Repositories (**Repositories Implementation**).
3.  **TheUnraveller.Service:**
    - Xử lý logic nghiệp vụ chính (**Business Logic**).
    - **GameEngineService**: Tính toán độ nghi ngờ (Suspicion Meter) và phản hồi từ AI.
    - Chứa các **DTOs** (Data Transfer Objects).
4.  **TheUnraveller.API:**
    - Lớp giao tiếp chính với Frontend.
    - Chứa các **Controllers** và cấu hình hệ thống (**Program.cs**).

## 🚀 Hướng Dẫn Cài Đặt (Installation)

1.  **Yêu cầu:** Đã cài đặt [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).
2.  **Di chuyển vào thư mục:** `cd BackEnd`
3.  **Xây dựng dự án:** `dotnet build`
4.  **Chạy dự án:** `dotnet run --project TheUnraveller.API`
5.  **Truy cập Swagger:** Mở trình duyệt tại `http://localhost:<port>/swagger` để xem tài liệu API.

## 🔑 Các Tính Năng Chính

- **Hệ thống nhiệm vụ:** Quản lý các kịch bản tương tác khác nhau.
- **Tính toán độ nghi ngờ:** Logic tự động tăng/giảm độ nghi ngờ dựa trên nội dung hội thoại.
- **Lịch sử hội thoại:** Lưu trữ toàn bộ tương tác để phân tích lỗi ngôn ngữ.
