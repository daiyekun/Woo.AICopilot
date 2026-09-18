using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("pg-password", secret: true);

var postgresdb = builder.AddPostgres("postgres", password: password)
    .WithHostPort(5432)
    .WithDataVolume("postgres-aicopilot")
    .WithPgWeb(pgAdmin => pgAdmin.WithHostPort(5050))
    // 挂载初始化脚本：容器启动时会自动执行该目录下的 .sql 文件创建 erp_demo
    .WithBindMount("./sql", "/docker-entrypoint-initdb.d")
    .AddDatabase("ai-copilot");

var rabbitmq = builder.AddRabbitMQ("eventbus")
    .WithManagementPlugin(5672)
    .WithLifetime(ContainerLifetime.Persistent);

var qdrant = builder.AddQdrant("qdrant")
    .WithDataVolume("qdrant-data"); // 持久化数据

var migration = builder.AddProject<Woo_AICopilot_MigrationWorkApp>("aicopilot-migration")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var httpapi = builder.AddProject<Woo_AICopilot_HttpApi>("aicopilot-httpapi")
    .WithUrl("swagger")
    .WaitFor(postgresdb)
    .WaitFor(rabbitmq)
    .WaitFor(qdrant)
    .WithReference(postgresdb)
    .WithReference(rabbitmq)
    .WithReference(qdrant)
    .WithReference(migration)
    .WaitForCompletion(migration);

builder.AddProject<Woo_AICopilot_RagWorker>("rag-worker")
    .WithReference(postgresdb) // 注入数据库连接
    .WithReference(rabbitmq) // 注入 RabbitMQ 连接
    .WaitFor(qdrant)
    .WaitFor(postgresdb) // 等待数据库启动
    .WaitFor(rabbitmq) // 等待 MQ 启动
    .WithReference(qdrant);

builder.AddViteApp("aicopilot-webui", "../Woo.AICopilot.Web")
    .WithEndpoint("http", endpointAnnotation =>
    {
        endpointAnnotation.Port = 5173;
    })
    .WaitFor(httpapi)
    .WithReference(httpapi);

builder.Build().Run();