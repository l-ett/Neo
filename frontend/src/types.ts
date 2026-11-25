export type DeviceKind = 'router' | 'switch' | 'host';

export interface Device {
  id: string;
  label: string;
  kind: DeviceKind;
  ports?: number;
  address?: string;
}

export interface Link {
  id: string;
  from: string;
  to: string;
  capacityMbps: number;
}

export interface Topology {
  name: string;
  devices: Device[];
  links: Link[];
}

export interface ValidationIssue {
  code: string;
  message: string;
}

export interface ValidationReport {
  isValid: boolean;
  errors: ValidationIssue[];
}
