; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
XOCLI101 | XOCLI | Error | Command implementations must implement `ICommand<TParameters>`
XOCLI111 | XOCLI | Error | Commands may not have multiple command attributes
XOCLI121 | XOCLI | Warning | Duplicate verb will be ignored
XOCLI201 | XOCLI | Error | `CommandBranchAttribute` must be applied to a custom attribute that derives from `CommandAttribute`
XOCLI211 | XOCLI | Error | Custom command attributes must have a public constructor
XOCLI212 | XOCLI | Error | Custom command attribute constructors must have a first parameter `string verb`
XOCLI221 | XOCLI | Warning | Duplicate path will be ignored
