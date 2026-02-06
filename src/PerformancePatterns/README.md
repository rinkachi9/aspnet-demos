# PerformancePatterns

Demonstrates **Performance Benchmarking** using `BenchmarkDotNet`.

## Scenarios
1.  **JSON Serialization**: Compared `System.Text.Json` (High performance) vs `Newtonsoft.Json` (Feature rich but slower).
2.  **String Manipulation**: Highlighting the memory allocation cost of string concatenation vs `StringBuilder`.

## How to Run
Benchmarks **must** be run in `Release` configuration to provide valid results.

```bash
cd src/PerformancePatterns
dotnet run -c Release
```

## Expected Results
- `System.Text.Json` should be significantly faster and allocate less memory.
- `StringBuilder` should strictly outperform concatenation in loops.
