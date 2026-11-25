using Neo.Domain.Model;
using Neo.Domain.Validation;
using static Neo.Domain.Tests.Fixtures;

namespace Neo.Domain.Tests;

public class TopologyValidatorTests
{
    [Fact]
    public void Well_formed_topology_is_valid()
    {
        var result = TopologyValidator.Validate(Build(
            [Router("r1", 4), Switch("s1", 8), Host("h1", "10.0.0.10")],
            [Connect("r1", "s1"), Connect("s1", "h1")]));

        Assert.IsType<Valid>(result.Value);
    }

    [Fact]
    public void Duplicate_device_ids_are_reported()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build([Router("r1", 4), Router("r1", 4)], [])));

        Assert.Contains(errors, error => error is DuplicateDeviceId);
    }

    [Fact]
    public void Links_must_reference_existing_devices_and_must_not_loop()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build(
            [Router("r1", 4)],
            [Connect("r1", "missing"), new Link("loop", "r1", "r1", 1000)])));

        Assert.Contains(errors, error => error is UnknownEndpoint);
        Assert.Contains(errors, error => error is SelfLink);
    }

    [Fact]
    public void Links_need_positive_capacity()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build(
            [Router("r1", 4), Router("r2", 4)],
            [Connect("r1", "r2", capacityMbps: 0)])));

        Assert.Contains(errors, error => error is InvalidCapacity);
    }

    [Fact]
    public void Hosts_accept_at_most_one_link()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build(
            [Switch("s1", 8), Switch("s2", 8), Host("h1", "10.0.0.10")],
            [Connect("s1", "h1"), Connect("s2", "h1")])));

        Assert.Contains(errors, error => error is HostHasMultipleLinks);
    }

    [Fact]
    public void Devices_cannot_have_more_links_than_ports()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build(
            [Router("r1", 1), Router("r2", 4), Router("r3", 4)],
            [Connect("r1", "r2"), Connect("r1", "r3")])));

        Assert.Contains(errors, error => error is PortsExhausted);
    }

    [Fact]
    public void Port_counts_must_be_between_one_and_the_maximum()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build(
            [Router("r1", 0), Switch("s1", TopologyValidator.MaxPorts + 1)],
            [])));

        Assert.Equal(2, errors.Count(error => error is InvalidPortCount));
    }

    [Theory]
    [InlineData("10.0.0")]
    [InlineData("300.1.1.1")]
    [InlineData("::1")]
    [InlineData("10.0.0.1.5")]
    public void Host_addresses_must_be_ipv4(string address)
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build([Host("h1", address)], [])));

        Assert.Contains(errors, error => error is InvalidAddress);
    }

    [Fact]
    public void Host_addresses_are_unique_after_normalisation()
    {
        var errors = ErrorsOf(TopologyValidator.Validate(Build(
            [Host("h1", "10.0.0.10"), Host("h2", "10.0.0.010")],
            [])));

        Assert.Contains(errors, error => error is DuplicateAddress);
    }
}
