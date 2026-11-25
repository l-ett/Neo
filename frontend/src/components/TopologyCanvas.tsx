import {
  Background,
  Controls,
  ReactFlow,
  useEdgesState,
  useNodesState,
  type Connection,
  type Edge,
} from '@xyflow/react';
import { useEffect } from 'react';
import { toFlowEdges, toFlowNodes, type DeviceFlowNode } from '../topologyFlow';
import type { Topology } from '../types';
import { DeviceNode } from './DeviceNode';

const nodeTypes = { device: DeviceNode };

interface TopologyCanvasProps {
  topology: Topology;
  onConnect: (connection: Connection) => void;
  onEdgesDelete: (edges: Edge[]) => void;
}

export function TopologyCanvas({ topology, onConnect, onEdgesDelete }: TopologyCanvasProps) {
  const [nodes, setNodes, onNodesChange] = useNodesState<DeviceFlowNode>(
    toFlowNodes(topology.devices, []),
  );
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>(toFlowEdges(topology.links));

  useEffect(() => {
    setNodes((previous) => toFlowNodes(topology.devices, previous));
  }, [topology.devices, setNodes]);

  useEffect(() => {
    setEdges(toFlowEdges(topology.links));
  }, [topology.links, setEdges]);

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      nodeTypes={nodeTypes}
      onNodesChange={onNodesChange}
      onEdgesChange={onEdgesChange}
      onConnect={onConnect}
      onEdgesDelete={onEdgesDelete}
      deleteKeyCode={['Delete', 'Backspace']}
      fitView
      minZoom={0.3}
    >
      <Background />
      <Controls />
    </ReactFlow>
  );
}
