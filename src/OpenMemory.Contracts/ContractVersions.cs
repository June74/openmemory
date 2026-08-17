// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

namespace OpenMemory.Contracts;

/// <summary>
/// Version integers for OpenMemory's wire and storage contracts, as fixed by
/// docs/COMPATIBILITY.md. Each is an independent integer rather than part of
/// the product's SemVer, per decision D-090. All remain unfrozen until Stage 2.
/// </summary>
public static class ContractVersions
{
    /// <summary>MCP protocol version, negotiated per connection.</summary>
    public const int McpProtocol = 1;

    /// <summary>Named-pipe framing and capability envelope version.</summary>
    public const int PipeEnvelope = 1;

    /// <summary>Database schema migration number. Forward-only.</summary>
    public const int DatabaseSchema = 1;

    /// <summary>Normalized, client-neutral event envelope version.</summary>
    public const int EventEnvelope = 1;

    /// <summary>Markdown projection protocol version.</summary>
    public const int ProjectionProtocol = 1;

    /// <summary>Portable export format version. Support is permanent.</summary>
    public const int PortableExportFormat = 1;
}
