# Todo Lists (.NET MAUI, Android)

Prerequisites: .NET 8 SDK + MAUI workload  ->  dotnet workload install maui-android

Run on emulator/device:   dotnet build -t:Run -f net8.0-android
Build APK:                dotnet publish -f net8.0-android -c Release

Data is stored in data.json inside the app's private data directory (FileSystem.AppDataDirectory).
