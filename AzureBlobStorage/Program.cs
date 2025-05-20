using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage"))
        .WithName(StorageAccountNames.BlobStorage);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/reports", async ([FromQuery] string fileName, IAzureClientFactory<BlobServiceClient> blobServiceClientFactory) =>
    {
        var blobServiceClient = blobServiceClientFactory.CreateClient(StorageAccountNames.BlobStorage);
        var containerClient = blobServiceClient.GetBlobContainerClient("reports");
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadAsync();

        return Results.File(response.Value.Content, "application/octet-stream");
    })
    .WithName("DownloadReportFile");

app.Run();

public static class StorageAccountNames
{
    public const string BlobStorage = "BlobStorage";
}