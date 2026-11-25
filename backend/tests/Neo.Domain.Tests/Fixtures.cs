using Neo.Domain.Model;
using Neo.Domain.Validation;

namespace Neo.Domain.Tests;

internal static class Fixtures
{
    public static Device Router(string id, int ports) => new(id, id, new RouterSpec(ports));

    public static Device Switch(string id, int ports) => new(id, id, new SwitchSpec(ports));

    public static Device Host(string id, string address) => new(id, id, new HostSpec(address));

    public static Link Connect(string from, string to, int capacityMbps = 1000) =>
        new($"{from}-{to}", from, to, capacityMbps);

    public static Topology Build(IReadOnlyList<Device> devices, IReadOnlyList<Link> links) =>
        new("test", devices, links);

    public static IReadOnlyList<TopologyError> ErrorsOf(ValidationResult result) =>
        Assert.IsType<Invalid>(result.Value).Errors;
}
