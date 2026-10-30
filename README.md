# Monopoly
Game Monopoly viết bằng C# và Windows Forms trên nền tảng .NET 9. Ứng dụng hỗ
trợ chơi cục bộ và các chức năng chơi qua mạng được triển khai trong project.
## Yêu cầu
- Windows 10 hoặc Windows 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Một trong các lựa chọn:
  - Visual Studio 2022 (khuyến nghị) với workload **.NET desktop development**
  - .NET CLI trong PowerShell hoặc Command Prompt
> Đây là ứng dụng Windows Forms (`net9.0-windows`), vì vậy không chạy trực tiếp
> trên Linux hoặc macOS.
## Clone project
Mở PowerShell hoặc Command Prompt và chạy:
```bash
git clone https://github.com/VuVjp/Monopoly.git
cd Monopoly
```
## Chạy bằng Visual Studio
1. Mở file `Monopoly.sln` bằng Visual Studio 2022.
2. Chờ Visual Studio khôi phục các package NuGet.
3. Chọn cấu hình **Debug** hoặc **Release**.
4. Nhấn `F5` hoặc chọn **Debug > Start Without Debugging** để chạy game.
## Chạy bằng .NET CLI
Từ thư mục gốc của project:
```bash
dotnet restore
dotnet build
dotnet run --project Monopoly.csproj
```
Để chạy bản Release:
```bash
dotnet run --project Monopoly.csproj --configuration Release
```
## Tạo file thực thi
Tạo bản build Release:
```bash
dotnet publish Monopoly.csproj --configuration Release --output ./publish
```
Sau khi publish, chạy file:
```text
publish/Monopoly.exe
```
## Lưu ý
- Không di chuyển hoặc xóa thư mục `Data` và `Resources`; game sử dụng dữ liệu
  bàn cờ và các tài nguyên hình ảnh/âm thanh trong đó.
- File `Data/Board.json` được tự động sao chép vào thư mục build khi project
  được build.
- Các package NuGet được khôi phục tự động bằng `dotnet restore` hoặc Visual
  Studio, gồm NAudio, Newtonsoft.Json và System.Data.SQLite.Core.
## Cấu trúc chính
```text
Monopoly.csproj       # Cấu hình project
Program.cs            # Điểm khởi chạy ứng dụng
Monopoly.cs           # Form chính
Screens/              # Các màn hình của game
Scripts/              # Logic game, dữ liệu và kết nối socket
Data/Board.json       # Dữ liệu bàn cờ
Resources/            # Hình ảnh và âm thanh
```
