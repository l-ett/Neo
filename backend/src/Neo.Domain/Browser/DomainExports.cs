using System.Runtime.InteropServices.JavaScript;
using Neo.Domain.Serialization;

namespace Neo.Domain.Wasm;

public static partial class DomainExports
{
    [JSExport]
    public static string Validate(string topologyJson) => TopologyJson.ValidateToReportJson(topologyJson);
}
