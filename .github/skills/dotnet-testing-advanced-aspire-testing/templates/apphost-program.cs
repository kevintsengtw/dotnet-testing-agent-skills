using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// 加入 PostgreSQL 資料庫 - 使用 Session 生命週期，測試結束後自動清理
var postgres = builder.AddPostgres("postgres")
                     .WithLifetime(ContainerLifetime.Session);

var postgresDb = postgres.AddDatabase("productdb");

// 加入 Redis 快取 - 使用 Session 生命週期，測試結束後自動清理
var redis = builder.AddRedis("redis")
                  .WithLifetime(ContainerLifetime.Session);

// 加入 API 服務 - 使用強型別專案參考
// Aspire 會自動處理服務發現和連線字串注入
var apiProject = builder.AddProject<Projects.MyApp_Api>("myapp-api")
                       .WithReference(postgresDb)
                       .WithReference(redis);

// 注意事項：
// 1. 不要手動配置 WithHttpEndpoint()，讓 Aspire 自動處理
// 2. ContainerLifetime.Session 確保測試結束後容器自動清理
// 3. 使用 WithReference() 建立服務間的依賴關係

builder.Build().Run();
