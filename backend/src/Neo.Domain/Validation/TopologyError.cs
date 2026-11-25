namespace Neo.Domain.Validation;

public sealed record DuplicateDeviceId(string DeviceId);

public sealed record DuplicateLinkId(string LinkId);

public sealed record DuplicateAddress(string DeviceId, string Address);

public sealed record UnknownDeviceKind(string DeviceId, string Kind);

public sealed record UnknownEndpoint(string LinkId, string DeviceId);

public sealed record SelfLink(string LinkId);

public sealed record InvalidCapacity(string LinkId, int CapacityMbps);

public sealed record InvalidPortCount(string DeviceId, int Ports);

public sealed record PortsExhausted(string DeviceId, int Ports, int LinkCount);

public sealed record InvalidAddress(string DeviceId, string Address);

public sealed record HostHasMultipleLinks(string DeviceId, int LinkCount);

public sealed record MalformedJson(string Reason);

public union TopologyError(
    DuplicateDeviceId,
    DuplicateLinkId,
    DuplicateAddress,
    UnknownDeviceKind,
    UnknownEndpoint,
    SelfLink,
    InvalidCapacity,
    InvalidPortCount,
    PortsExhausted,
    InvalidAddress,
    HostHasMultipleLinks,
    MalformedJson)
{
    public string Code => this switch
    {
        DuplicateDeviceId => "duplicate-device-id",
        DuplicateLinkId => "duplicate-link-id",
        DuplicateAddress => "duplicate-address",
        UnknownDeviceKind => "unknown-device-kind",
        UnknownEndpoint => "unknown-endpoint",
        SelfLink => "self-link",
        InvalidCapacity => "invalid-capacity",
        InvalidPortCount => "invalid-port-count",
        PortsExhausted => "ports-exhausted",
        InvalidAddress => "invalid-address",
        HostHasMultipleLinks => "host-multiple-links",
        MalformedJson => "malformed-json",
    };

    public string Message => this switch
    {
        DuplicateDeviceId e => $"Device id '{e.DeviceId}' is used more than once.",
        DuplicateLinkId e => $"Link id '{e.LinkId}' is used more than once.",
        DuplicateAddress e => $"Device '{e.DeviceId}' uses address {e.Address}, which another device already has.",
        UnknownDeviceKind e => $"Device '{e.DeviceId}' has unknown kind '{e.Kind}'.",
        UnknownEndpoint e => $"Link '{e.LinkId}' references unknown device '{e.DeviceId}'.",
        SelfLink e => $"Link '{e.LinkId}' connects a device to itself.",
        InvalidCapacity e => $"Link '{e.LinkId}' has capacity {e.CapacityMbps} Mbps; it must be positive.",
        InvalidPortCount e => $"Device '{e.DeviceId}' has {e.Ports} ports; it must have between 1 and {TopologyValidator.MaxPorts}.",
        PortsExhausted e => $"Device '{e.DeviceId}' has {e.LinkCount} links but only {e.Ports} ports.",
        InvalidAddress e => $"Host '{e.DeviceId}' has invalid IPv4 address '{e.Address}'.",
        HostHasMultipleLinks e => $"Host '{e.DeviceId}' has {e.LinkCount} links; hosts may have at most one.",
        MalformedJson e => $"Topology JSON is malformed: {e.Reason}",
    };
}
