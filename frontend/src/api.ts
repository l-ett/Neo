import type { Topology, ValidationReport } from './types';

export async function fetchTopology(): Promise<Topology> {
  const response = await fetch('/api/topology');
  if (!response.ok) {
    throw new Error(`GET /api/topology failed with HTTP ${response.status}`);
  }
  return response.json();
}

export type SaveResult =
  | { ok: true; topology: Topology }
  | { ok: false; report: ValidationReport };

export async function saveTopology(topology: Topology): Promise<SaveResult> {
  const response = await fetch('/api/topology', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(topology),
  });

  if (response.ok) {
    return { ok: true, topology: await response.json() };
  }
  if (response.status === 422) {
    return { ok: false, report: await response.json() };
  }
  throw new Error(`PUT /api/topology failed with HTTP ${response.status}`);
}
