using Neo.Domain.Model;

namespace Neo.Domain.Validation;

public sealed record Valid(Topology Topology);

public sealed record Invalid(IReadOnlyList<TopologyError> Errors);

public union ValidationResult(Valid, Invalid);
