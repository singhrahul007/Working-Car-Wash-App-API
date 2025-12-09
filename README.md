# Backend - CarWash API
## Requirements
- .NET 7 SDK
- SQL Server (local / Docker / Azure)

## Run locally
1. Update appsettings.json ConnectionStrings:DefaultConnection
2. dotnet restore
3. dotnet ef migrations add Initial
4. dotnet ef database update
5. dotnet run

## Notes
- Auth controller implements a mock OTP flow (accepts any code). Integrate Twilio Verify in production.
- Upload presign returns mock URL. Implement AWS S3 presigned generation in production.
