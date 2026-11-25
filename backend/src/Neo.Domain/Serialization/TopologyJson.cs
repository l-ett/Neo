using System.Text.Json;
using System.Text.Json.Serialization;
using Neo.Domain.Model;
using Neo.Domain.Validation;

namespace Neo.Domain.Serialization;

public static class TopologyJson
{
    public static ValidationResult Parse(string json)
    {
        TopologyDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize(json, TopologyJsonContext.Default.TopologyDto);
        }
        catch (JsonException exception)
        {
            return new Invalid([new MalformedJson(exception.Message)]);
        }

        if (dto is null)
        {
            return new Invalid([new MalformedJson("expected a topology object")]);
        }

        List<Device> devices = [];
        List<TopologyError> kindErrors = [];
        foreach (var deviceDto in dto.Devices ?? [])
        {
            if (TryToDeviceKind(deviceDto, out var kind))
            {
                devices.Add(new Device(deviceDto.Id ?? "", deviceDto.Label ?? "", kind));
            }
            else
            {
                kindErrors.Add(new UnknownDeviceKind(deviceDto.Id ?? "", deviceDto.Kind ?? ""));
            }
        }

        if (kindErrors.Count > 0)
        {
            return new Invalid(kindErrors);
        }

        var links = (dto.Links ?? [])
            .Select(link => new Link(link.Id ?? "", link.From ?? "", link.To ?? "", link.CapacityMbps ?? 0))
            .ToList();

        return TopologyValidator.Validate(new Topology(dto.Name ?? "", devices, links));
    }

    public static string Serialize(Topology topology)
    {
        var dto = new TopologyDto
        {
            Name = topology.Name,
            Devices = topology.Devices.Select(ToDeviceDto).ToList(),
            Links = topology.Links.Select(link => new LinkDto
            {
                Id = link.Id,
                From = link.FromDeviceId,
                To = link.ToDeviceId,
                CapacityMbps = link.CapacityMbps,
            }).ToList(),
        };

        return JsonSerializer.Serialize(dto, TopologyJsonContext.Default.TopologyDto);
    }

    public static string ToReportJson(ValidationResult result)
    {
        var report = result switch
        {
            Valid => new ReportDto { IsValid = true },
            Invalid invalid => new ReportDto
            {
                IsValid = false,
                Errors = invalid.Errors
                    .Select(error => new ErrorDto { Code = error.Code, Message = error.Message })
                    .ToList(),
            },
        };

        return JsonSerializer.Serialize(report, TopologyJsonContext.Default.ReportDto);
    }

    public static string ValidateToReportJson(string json) => ToReportJson(Parse(json));

    private static bool TryToDeviceKind(DeviceDto device, out DeviceKind kind)
    {
        switch (device.Kind)
        {
            case "router":
                kind = new RouterSpec(device.Ports ?? 0);
                return true;
            case "switch":
                kind = new SwitchSpec(device.Ports ?? 0);
                return true;
            case "host":
                kind = new HostSpec(device.Address ?? "");
                return true;
            default:
                kind = default;
                return false;
        }
    }

    private static DeviceDto ToDeviceDto(Device device) => device.Kind switch
    {
        RouterSpec router => new DeviceDto { Id = device.Id, Label = device.Label, Kind = "router", Ports = router.Ports },
        SwitchSpec sw => new DeviceDto { Id = device.Id, Label = device.Label, Kind = "switch", Ports = sw.Ports },
        HostSpec host => new DeviceDto { Id = device.Id, Label = device.Label, Kind = "host", Address = host.Address },
    };
}

internal sealed class TopologyDto
{
    public string? Name { get; set; }

    public List<DeviceDto>? Devices { get; set; }

    public List<LinkDto>? Links { get; set; }
}

internal sealed class DeviceDto
{
    public string? Id { get; set; }

    public string? Label { get; set; }

    public string? Kind { get; set; }

    public int? Ports { get; set; }

    public string? Address { get; set; }
}

internal sealed class LinkDto
{
    public string? Id { get; set; }

    public string? From { get; set; }

    public string? To { get; set; }

    public int? CapacityMbps { get; set; }
}

internal sealed class ReportDto
{
    public bool IsValid { get; set; }

    public List<ErrorDto> Errors { get; set; } = [];
}

internal sealed class ErrorDto
{
    public string Code { get; set; } = "";

    public string Message { get; set; } = "";
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TopologyDto))]
[JsonSerializable(typeof(ReportDto))]
internal sealed partial class TopologyJsonContext : JsonSerializerContext
{
}
