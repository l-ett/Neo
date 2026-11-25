# Neo

A topology prototype where the domain is written once in C# 15 and runs in both places:

- **Backend** (`backend/src/Neo.Backend`): ASP.NET Core on .NET 11 validates topologies on save.
- **Frontend** (`frontend/`): React + React Flow. The same domain code is compiled to WebAssembly (`backend/src/Neo.Domain`) and validates every edit in the browser.

## Layout

| Path | Purpose |
| --- | --- |
| `backend/Neo.slnx` | Solution containing all .NET projects |
| `backend/src/Neo.Domain` | Shared domain: `union` types for device kinds and validation errors, the validator, and the JSON contract. Targets `net11.0` (backend, tests) and `net11.0-browser` (browser bundle; `Browser/` holds the `[JSExport]` surface and its entry point) |
| `backend/src/Neo.Backend` | Minimal API: `GET /api/topology`, `PUT /api/topology` (422 on invalid), `POST /api/topology/validate` |
| `backend/tests/Neo.Domain.Tests` | xUnit tests for the validation rules and the JSON contract |
| `frontend/` | Vite + React + `@xyflow/react` |

## Prerequisites

- .NET 11 SDK (11.0.100 or later, see `global.json` at the repo root)
- `dotnet workload install wasm-tools`
- Node.js `^20.19.0` or `>=22.12.0`

## Run

```bash
dotnet test backend/Neo.slnx                            # domain tests
dotnet run --project backend/src/Neo.Backend            # API on http://localhost:5080
dotnet publish backend/src/Neo.Domain -c Release -f net11.0-browser   # browser bundle copied to frontend/public/domain
cd frontend && npm install && npm run dev               # UI on http://localhost:5173
```

Publish projects individually: `dotnet publish backend/Neo.slnx` fails because the domain targets two frameworks (NETSDK1129), so the browser bundle needs `-f net11.0-browser`.

Re-run the `dotnet publish` step after changing the domain. The React app loads the bundle at runtime, so the frontend picks up the new build on reload.

## Bundle size

The browser build is non-AOT (`RunAOTCompilation=false` in `backend/src/Neo.Domain/Neo.Domain.csproj`). Measurements for the domain bundle:

| Build | Raw | Brotli | Per validation (Node) | Per validation (headless Chrome) |
| --- | --- | --- | --- | --- |
| Non-AOT (current) | 3.1 MB | 0.99 MB | 0.18 ms | 0.74 ms |
| AOT | 7.8 MB | 2.0 MB | 0.04 ms | 0.10–0.13 ms |
| AOT, `WasmEnableSIMD=false` | 6.7 MB | 1.78 MB | 0.04 ms | not measured |

AOT moves compiled managed code into `dotnet.native.wasm` (5.8 MB versus 1.1 MB). The managed modules are the same size in both builds.

These made no meaningful difference: `UseSystemResourceKeys`, `DebuggerSupport=false`, `StackTraceSupport=false`, and `AOTMode=LLVMOnly`. The `LLVMOnly` bundle was identical, so the interpreter still ships.

Precompressed `.br` and `.gz` files are not copied to `frontend/public/domain`, so the server must compress `.wasm` and `.js`.

Each timing is a single run, so treat the numbers as rough. To return to AOT, set `RunAOTCompilation` to `true`.

## Topology JSON

```json
{
  "name": "Campus lab",
  "devices": [
    { "id": "core-1", "label": "Core router", "kind": "router", "ports": 4 },
    { "id": "pc-1", "label": "Workstation", "kind": "host", "address": "10.0.10.11" }
  ],
  "links": [
    { "id": "l1", "from": "core-1", "to": "pc-1", "capacityMbps": 100 }
  ]
}
```

Kinds are `router`, `switch` (both with `ports`) and `host` (with an IPv4 `address`). The validator reports unknown endpoints, self-links, duplicate ids and addresses, invalid ports, invalid capacity, exhausted ports, and hosts with more than one link.
