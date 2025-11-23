using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<BoomerSse_Demo>("demo-inmem", launchProfileName: null)
    .WithHttpsEndpoint(port: 50010)
    .WithEnvironment("BSSE_SCALEOUT", "InMemory");

builder
    .Build()
    .Run();