namespace Neo.Domain.Model;

public sealed record Device(string Id, string Label, DeviceKind Kind);

public sealed record Link(string Id, string FromDeviceId, string ToDeviceId, int CapacityMbps);

public sealed record Topology(string Name, IReadOnlyList<Device> Devices, IReadOnlyList<Link> Links);
