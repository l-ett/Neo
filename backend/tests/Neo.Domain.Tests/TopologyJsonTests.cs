using Neo.Domain.Model;
using Neo.Domain.Serialization;
using Neo.Domain.Validation;
using static Neo.Domain.Tests.Fixtures;

namespace Neo.Domain.Tests;

public class TopologyJsonTests
{
    [Fact]
    public void Serialized_topology_parses_back_to_the_same_json()
    {
        var topology = Build(
            [Router("r1", 4), Switch("s1", 8), Host("h1", "10.0.0.10")],
            [Connect("r1", "s1", 10_000), Connect("s1", "h1", 100)]);

        var json = TopologyJson.Serialize(topology);
        var parsed = Assert.IsType<Valid>(TopologyJson.Parse(json).Value).Topology;

        Assert.Equal(json, TopologyJson.Serialize(parsed));
    }

    [Fact]
    public void Unknown_device_kind_is_reported()
    {
        var json = """{"name":"t","devices":[{"id":"d1","kind":"firewall"}],"links":[]}""";

        Assert.Contains(ErrorsOf(TopologyJson.Parse(json)), error => error is UnknownDeviceKind);
    }

    [Fact]
    public void Malformed_json_is_reported()
    {
        Assert.Contains(ErrorsOf(TopologyJson.Parse("{not json")), error => error is MalformedJson);
    }

    [Fact]
    public void Valid_report_has_no_errors()
    {
        var report = TopologyJson.ValidateToReportJson(
            TopologyJson.Serialize(Build([Router("r1", 4)], [])));

        Assert.Equal("""{"isValid":true,"errors":[]}""", report);
    }

    [Fact]
    public void Invalid_report_exposes_error_codes()
    {
        var json = """{"name":"t","devices":[{"id":"r1","kind":"router","ports":4}],"links":[{"id":"l1","from":"r1","to":"r1","capacityMbps":100}]}""";

        var report = TopologyJson.ValidateToReportJson(json);

        Assert.Contains("\"isValid\":false", report);
        Assert.Contains("\"code\":\"self-link\"", report);
    }
}
