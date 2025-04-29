var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.NetworkAISuite_ApiService>("apiservice");

builder.AddProject<Projects.NetworkAISuite_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);
    
builder.AddContainer("trafficcollector", "trafficcollector")
    .WithImageTag("latest")
    .WithHttpEndpoint(port: 8081, targetPort: 8080);

builder.AddContainer("reportgenerator", "reportgenerator")
    .WithImageTag("latest")
    .WithHttpEndpoint(port: 8082, targetPort: 8080);

builder.AddContainer("networkanalyzer", "networkanalyzer")
    .WithImageTag("latest")
    .WithHttpEndpoint(port: 8000, targetPort: 8000);

builder.AddContainer("redis", "redis")
    .WithImageTag("latest")
    .WithEnvironment("ALLOW_EMPTY_PASSWORD", "yes")
    .WithEndpoint(name: "redis", port: 6379, targetPort: 6379);

builder.Build().Run();
