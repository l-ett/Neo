using Neo.Domain.Model;

namespace Neo.Backend;

internal sealed class TopologyStore(Topology initial)
{
    private readonly Lock _gate = new();
    private Topology _current = initial;

    public Topology Current
    {
        get
        {
            lock (_gate)
            {
                return _current;
            }
        }
    }

    public void Replace(Topology topology)
    {
        lock (_gate)
        {
            _current = topology;
        }
    }
}
