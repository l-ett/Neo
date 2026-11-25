import type { Topology, ValidationReport } from './types';

// Shape of the [JSExport] surface generated for Neo.Domain.Wasm.DomainExports.
interface DomainExports {
  Neo: {
    Domain: {
      Wasm: {
        DomainExports: {
          Validate(topologyJson: string): string;
        };
      };
    };
  };
}

export type TopologyValidator = (topology: Topology) => ValidationReport;

// Published by `dotnet publish backend/src/Neo.Domain -c Release -f net11.0-browser` into public/domain.
// The URL is built at runtime: Vite must not treat the public bundle as a source import.
const dotnetModuleUrl = () => new URL('domain/_framework/dotnet.js', document.baseURI).href;

let validator: Promise<TopologyValidator> | undefined;

/** Starts the C# domain (WebAssembly) once and returns a validator backed by it. */
export function loadDomainValidator(): Promise<TopologyValidator> {
  validator ??= (async () => {
    const { dotnet } = await import(/* @vite-ignore */ dotnetModuleUrl());
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const exports: DomainExports = await getAssemblyExports(getConfig().mainAssemblyName);

    return (topology) =>
      JSON.parse(exports.Neo.Domain.Wasm.DomainExports.Validate(JSON.stringify(topology)));
  })();

  return validator;
}
