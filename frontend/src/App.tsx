import { useEffect, useMemo, useState } from 'react';
import type { Connection, Edge } from '@xyflow/react';
import { fetchTopology, saveTopology } from './api';
import { loadDomainValidator, type TopologyValidator } from './domain';
import { TopologyCanvas } from './components/TopologyCanvas';
import { ValidationPanel, type Notice, type Timing } from './components/ValidationPanel';
import type { Topology, ValidationReport } from './types';

export function App() {
  const [topology, setTopology] = useState<Topology | null>(null);
  const [loadError, setLoadError] = useState<string>();
  const [validator, setValidator] = useState<TopologyValidator>();
  const [domainError, setDomainError] = useState<string>();
  const [notice, setNotice] = useState<Notice>();
  const [saving, setSaving] = useState(false);
  const [timing, setTiming] = useState<Timing>();

  useEffect(() => {
    fetchTopology().then(setTopology).catch((error: unknown) => setLoadError(String(error)));
    loadDomainValidator()
      // Wrapped: React would otherwise call the validator as a state updater.
      .then((loaded) => setValidator(() => loaded))
      .catch((error: unknown) => setDomainError(String(error)));
  }, []);

  const report = useMemo(
    () => (topology && validator ? validator(topology) : undefined),
    [topology, validator],
  );

  if (loadError) {
    return <p className="status status--error">Could not load the topology: {loadError}</p>;
  }
  if (!topology) {
    return <p className="status">Loading topology…</p>;
  }

  const handleConnect = (connection: Connection) => {
    setTopology((current) => current && {
      ...current,
      links: [
        ...current.links,
        {
          id: `link-${crypto.randomUUID().slice(0, 8)}`,
          from: connection.source,
          to: connection.target,
          capacityMbps: 1000,
        },
      ],
    });
  };

  const handleEdgesDelete = (deleted: Edge[]) => {
    const removed = new Set(deleted.map((edge) => edge.id));
    setTopology((current) => current && {
      ...current,
      links: current.links.filter((link) => !removed.has(link.id)),
    });
  };

  const handleSave = async () => {
    setSaving(true);
    setNotice(undefined);
    try {
      const result = await saveTopology(topology);
      if (result.ok) {
        setTopology(result.topology);
        setNotice({ tone: 'ok', text: 'Saved. The backend validated it with the same domain code.' });
      } else {
        setNotice({
          tone: 'error',
          text: `The backend rejected the topology with ${result.report.errors.length} error(s).`,
        });
      }
    } catch (error) {
      setNotice({ tone: 'error', text: String(error) });
    } finally {
      setSaving(false);
    }
  };

  const handleReload = async () => {
    try {
      setTopology(await fetchTopology());
      setNotice({ tone: 'ok', text: 'Reloaded from the API.' });
    } catch (error) {
      setNotice({ tone: 'error', text: String(error) });
    }
  };

  const handleTimeValidation = async () => {
    if (!topology || !validator) return;

    const runs = 200;
    validator(topology);
    const wasmStart = performance.now();
    let wasmReport = validator(topology);
    for (let i = 1; i < runs; i++) {
      wasmReport = validator(topology);
    }
    const wasmMs = (performance.now() - wasmStart) / runs;

    try {
      const backendStart = performance.now();
      const response = await fetch('/api/topology/validate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(topology),
      });
      const backendReport: ValidationReport = await response.json();
      const backendMs = performance.now() - backendStart;
      setTiming({ wasmMs, backendMs, agree: wasmReport.isValid === backendReport.isValid });
    } catch (error) {
      setNotice({ tone: 'error', text: String(error) });
    }
  };

  return (
    <div className="app">
      <header className="app__header">
        <h1>{topology.name}</h1>
        <span>React Flow · validated by the shared C# domain (WebAssembly)</span>
      </header>
      <main className="app__body">
        <div className="app__canvas">
          <TopologyCanvas
            topology={topology}
            onConnect={handleConnect}
            onEdgesDelete={handleEdgesDelete}
          />
        </div>
        <ValidationPanel
          topology={topology}
          report={report}
          domainError={domainError}
          notice={notice}
          saving={saving}
          onSave={handleSave}
          onReload={handleReload}
          timing={timing}
          canTime={validator !== undefined}
          onTimeValidation={handleTimeValidation}
        />
      </main>
    </div>
  );
}
