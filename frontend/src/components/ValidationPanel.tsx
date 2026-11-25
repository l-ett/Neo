import type { Topology, ValidationReport } from '../types';

export interface Notice {
  tone: 'ok' | 'error';
  text: string;
}

export interface Timing {
  wasmMs: number;
  backendMs: number;
  agree: boolean;
}

interface ValidationPanelProps {
  topology: Topology;
  report: ValidationReport | undefined;
  domainError: string | undefined;
  notice: Notice | undefined;
  saving: boolean;
  onSave: () => void;
  onReload: () => void;
  timing: Timing | undefined;
  canTime: boolean;
  onTimeValidation: () => void;
}

export function ValidationPanel({
  topology,
  report,
  domainError,
  notice,
  saving,
  onSave,
  onReload,
  timing,
  canTime,
  onTimeValidation,
}: ValidationPanelProps) {
  return (
    <aside className="panel">
      <section>
        <h2>Validation</h2>
        <ValidationStatus report={report} domainError={domainError} />
        <button type="button" onClick={onTimeValidation} disabled={!canTime}>
          Time C# validation
        </button>
        {timing && (
          <ul className="timing">
            <li>C# in the browser (WASM): {timing.wasmMs.toFixed(3)} ms per call, avg of 200</li>
            <li>C# in the backend (HTTP): {timing.backendMs.toFixed(1)} ms round trip</li>
            <li>{timing.agree ? 'Both report the same result.' : 'The two results differ.'}</li>
          </ul>
        )}
      </section>

      <section>
        <h2>Topology</h2>
        <p>
          {topology.devices.length} devices · {topology.links.length} links
        </p>
        <div className="panel__actions">
          <button type="button" onClick={onSave} disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
          <button type="button" onClick={onReload} disabled={saving}>
            Reload
          </button>
        </div>
        {notice && <p className={`status status--${notice.tone}`}>{notice.text}</p>}
      </section>

      <section className="hint">
        <p>Drag from a device's bottom handle onto another device to add a link.</p>
        <p>Select a link and press Delete to remove it. Validation re-runs on every change.</p>
      </section>
    </aside>
  );
}

function ValidationStatus({
  report,
  domainError,
}: Pick<ValidationPanelProps, 'report' | 'domainError'>) {
  if (domainError) {
    return <p className="status status--error">WebAssembly domain unavailable: {domainError}</p>;
  }
  if (!report) {
    return <p className="status">Loading the WebAssembly domain…</p>;
  }
  if (report.isValid) {
    return <p className="status status--ok">Valid. Checked in the browser by the C# domain.</p>;
  }

  return (
    <>
      <p className="status status--error">
        {report.errors.length} issue{report.errors.length === 1 ? '' : 's'} found
      </p>
      <ul className="issues">
        {report.errors.map((issue, index) => (
          <li key={`${issue.code}-${index}`}>
            <code>{issue.code}</code> {issue.message}
          </li>
        ))}
      </ul>
    </>
  );
}
