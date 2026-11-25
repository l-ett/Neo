import { Handle, Position, type NodeProps } from '@xyflow/react';
import type { DeviceFlowNode } from '../topologyFlow';
import type { Device } from '../types';

export function DeviceNode({ data }: NodeProps<DeviceFlowNode>) {
  const { device } = data;

  return (
    <div className={`device-node device-node--${device.kind}`}>
      <Handle type="target" position={Position.Top} />
      <div className="device-node__kind">{device.kind}</div>
      <div className="device-node__label">{device.label}</div>
      <div className="device-node__detail">{describe(device)}</div>
      <Handle type="source" position={Position.Bottom} />
    </div>
  );
}

function describe(device: Device): string {
  if (device.kind === 'host') {
    return device.address ?? 'no address';
  }
  return `${device.ports ?? '?'} ports`;
}
