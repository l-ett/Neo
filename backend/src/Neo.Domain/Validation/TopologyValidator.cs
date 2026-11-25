using System.Globalization;
using Neo.Domain.Model;

namespace Neo.Domain.Validation;

public static class TopologyValidator
{
    public const int MaxPorts = 48;

    public static ValidationResult Validate(Topology topology)
    {
        List<TopologyError> errors = [];
        var deviceIds = new HashSet<string>(StringComparer.Ordinal);
        var addresses = new HashSet<string>(StringComparer.Ordinal);

        foreach (var device in topology.Devices)
        {
            if (!deviceIds.Add(device.Id))
            {
                errors.Add(new DuplicateDeviceId(device.Id));
            }

            switch (device.Kind)
            {
                case RouterSpec router when !IsValidPortCount(router.Ports):
                    errors.Add(new InvalidPortCount(device.Id, router.Ports));
                    break;
                case SwitchSpec sw when !IsValidPortCount(sw.Ports):
                    errors.Add(new InvalidPortCount(device.Id, sw.Ports));
                    break;
                case HostSpec host:
                    if (!TryNormalizeIPv4(host.Address, out var address))
                    {
                        errors.Add(new InvalidAddress(device.Id, host.Address));
                    }
                    else if (!addresses.Add(address))
                    {
                        errors.Add(new DuplicateAddress(device.Id, host.Address));
                    }
                    break;
            }
        }

        var linkIds = new HashSet<string>(StringComparer.Ordinal);
        var linkCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var link in topology.Links)
        {
            if (!linkIds.Add(link.Id))
            {
                errors.Add(new DuplicateLinkId(link.Id));
            }

            if (link.FromDeviceId == link.ToDeviceId)
            {
                errors.Add(new SelfLink(link.Id));
            }

            foreach (var endpoint in new[] { link.FromDeviceId, link.ToDeviceId })
            {
                if (!deviceIds.Contains(endpoint))
                {
                    errors.Add(new UnknownEndpoint(link.Id, endpoint));
                }

                linkCounts[endpoint] = linkCounts.GetValueOrDefault(endpoint) + 1;
            }

            if (link.CapacityMbps <= 0)
            {
                errors.Add(new InvalidCapacity(link.Id, link.CapacityMbps));
            }
        }

        foreach (var device in topology.Devices)
        {
            var linkCount = linkCounts.GetValueOrDefault(device.Id);

            switch (device.Kind)
            {
                case HostSpec when linkCount > 1:
                    errors.Add(new HostHasMultipleLinks(device.Id, linkCount));
                    break;
                case RouterSpec router when IsValidPortCount(router.Ports) && linkCount > router.Ports:
                    errors.Add(new PortsExhausted(device.Id, router.Ports, linkCount));
                    break;
                case SwitchSpec sw when IsValidPortCount(sw.Ports) && linkCount > sw.Ports:
                    errors.Add(new PortsExhausted(device.Id, sw.Ports, linkCount));
                    break;
            }
        }

        return errors.Count == 0 ? new Valid(topology) : new Invalid(errors);
    }

    private static bool IsValidPortCount(int ports) => ports is >= 1 and <= MaxPorts;

    private static bool TryNormalizeIPv4(string text, out string normalized)
    {
        var octets = text.Split('.');
        if (octets.Length != 4 || !octets.All(IsOctet))
        {
            normalized = string.Empty;
            return false;
        }

        normalized = string.Join('.', octets.Select(octet => byte.Parse(octet, CultureInfo.InvariantCulture)));
        return true;
    }

    private static bool IsOctet(string text) =>
        byte.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _);
}
