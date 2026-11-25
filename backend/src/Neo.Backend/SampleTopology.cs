using Neo.Domain.Model;

namespace Neo.Backend;

internal static class SampleTopology
{
    public static Topology Create() => new(
        "Campus lab",
        [
            new Device("core-1", "Core router", new RouterSpec(4)),
            new Device("edge-1", "Edge router", new RouterSpec(4)),
            new Device("sw-a", "Access switch A", new SwitchSpec(8)),
            new Device("sw-b", "Access switch B", new SwitchSpec(8)),
            new Device("pc-1", "Workstation 1", new HostSpec("10.0.10.11")),
            new Device("pc-2", "Workstation 2", new HostSpec("10.0.10.12")),
            new Device("pc-3", "Workstation 3", new HostSpec("10.0.20.11")),
            new Device("srv-1", "File server", new HostSpec("10.0.20.50")),
        ],
        [
            new Link("core-edge", "core-1", "edge-1", 10_000),
            new Link("core-sw-a", "core-1", "sw-a", 1_000),
            new Link("edge-sw-b", "edge-1", "sw-b", 1_000),
            new Link("sw-a-pc-1", "sw-a", "pc-1", 100),
            new Link("sw-a-pc-2", "sw-a", "pc-2", 100),
            new Link("sw-b-pc-3", "sw-b", "pc-3", 100),
            new Link("sw-b-srv-1", "sw-b", "srv-1", 1_000),
        ]);
}
