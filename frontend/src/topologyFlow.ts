import type { Edge, Node, XYPosition } from '@xyflow/react';
import type { Device, DeviceKind, Link, Topology } from './types';

export type DeviceFlowNode = Node<{ device: Device }, 'device'>;

const ROW_BY_KIND: Record<DeviceKind, number> = { router: 0, switch: 1, host: 2 };
const COLUMN_SPACING = 240;
const ROW_SPACING = 200;

export function toFlowNodes(devices: Device[], previous: DeviceFlowNode[]): DeviceFlowNode[] {
  const previousPositions = new Map(previous.map((node) => [node.id, node.position]));

  return devices.map((device) => ({
    id: device.id,
    type: 'device',
    position: previousPositions.get(device.id) ?? defaultPosition(devices, device),
    data: { device },
    deletable: false,
  }));
}

export function toFlowEdges(links: Link[]): Edge[] {
  return links.map((link) => ({
    id: link.id,
    source: link.from,
    target: link.to,
    label: formatCapacity(link.capacityMbps),
  }));
}

/** Lays devices out in rows by kind (routers, switches, hosts), centred on the x axis. */
function defaultPosition(devices: Device[], device: Device): XYPosition {
  const peers = devices.filter((candidate) => candidate.kind === device.kind);
  const column = peers.indexOf(device);
  const centre = (peers.length - 1) / 2;

  return {
    x: 400 + (column - centre) * COLUMN_SPACING,
    y: ROW_BY_KIND[device.kind] * ROW_SPACING,
  };
}

function formatCapacity(mbps: number): string {
  return mbps >= 1000 ? `${mbps / 1000} Gbps` : `${mbps} Mbps`;
}
