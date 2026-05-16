using System.Text.Json;
using GroupDocs.Merger.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Merger.Mcp.IntegrationTests;

/// GetDocumentInfo returns file type / page count / size / per-page
/// dimensions as JSON without modifying the input.
[Collection(McpServerCollection.Name)]
public class GetDocumentInfoTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetDocumentInfoTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetDocumentInfo_SyntheticPdf_ReturnsOnePageJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SyntheticPdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Tool reported an error: " + ToolResponse.Text(response));

        var json = ToolResponse.Json(response);
        _output.WriteLine(JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));

        Assert.Equal(JsonValueKind.Object, json.ValueKind);
        Assert.True(json.TryGetProperty("fileName", out var fileName));
        Assert.Equal(SampleDocuments.SyntheticPdf, fileName.GetString());

        Assert.True(json.TryGetProperty("pageCount", out var pageCount));
        Assert.Equal(1, pageCount.GetInt32());

        Assert.True(json.TryGetProperty("size", out var size));
        Assert.True(size.GetInt64() > 0);
    }

    public static IEnumerable<object[]> RealSamples() => new[]
    {
        new object[] { SampleDocuments.SamplePdf  },
        new object[] { SampleDocuments.SampleDocx },
        new object[] { SampleDocuments.SamplePptx },
        new object[] { SampleDocuments.SampleXlsx },
        new object[] { SampleDocuments.Sample10PagesDocx },
    };

    [Theory]
    [MemberData(nameof(RealSamples))]
    public async Task GetDocumentInfo_RealSample_ReturnsValidJsonStructure(string fileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                $"Document-info lookup failed for '{fileName}': {ToolResponse.Text(response)}");

        var json = ToolResponse.Json(response);
        _output.WriteLine(JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));

        Assert.True(json.TryGetProperty("fileName", out var fileNameProp));
        Assert.Equal(fileName, fileNameProp.GetString());

        Assert.True(json.TryGetProperty("pageCount", out var pageCount));
        Assert.Equal(JsonValueKind.Number, pageCount.ValueKind);
        Assert.True(pageCount.GetInt32() >= 1);

        Assert.True(json.TryGetProperty("size", out var size));
        Assert.True(size.GetInt64() > 0);

        Assert.True(json.TryGetProperty("fileType", out _));
    }

    [Fact]
    public async Task GetDocumentInfo_TenPageDocx_ReportsTenPages()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.Sample10PagesDocx)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.Sample10PagesDocx}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.Sample10PagesDocx },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Tool reported an error: " + ToolResponse.Text(response));

        var json = ToolResponse.Json(response);
        _output.WriteLine(JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));

        Assert.True(json.TryGetProperty("pageCount", out var pageCount));
        // The fixture is named "10-pages" — assert the engine agrees.
        Assert.Equal(10, pageCount.GetInt32());
    }
}
