namespace Neo.Domain.Model;

public sealed record RouterSpec(int Ports);

public sealed record SwitchSpec(int Ports);

public sealed record HostSpec(string Address);

public union DeviceKind(RouterSpec, SwitchSpec, HostSpec);
