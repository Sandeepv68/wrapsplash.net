# Contributing to WrapSplash.NET

Thank you for your interest in contributing! Here's how to get started.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- An IDE (Visual Studio, Rider, or VS Code with C# extension)

## Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/your-username/wrapsplash.net.git
   cd wrapsplash.net
   ```
3. Create a branch for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development

### Build

```bash
dotnet build WrapSplash.slnx
```

### Test

```bash
dotnet test WrapSplash.slnx
```

### Project Structure

```
WrapSplash.NET/
  Configuration/   - API endpoint constants
  Http/            - HTTP client with Polly retry policies
  Models/          - Public models (options, exceptions)
  Services/        - Main WrapSplashClient class

WrapSplash.NET.Tests/
  WrapSplashClientTests.cs   - xUnit test suite
```

## Guidelines

- Follow the existing code style and conventions
- Add XML documentation comments to all public members
- Write tests for new functionality
- Keep pull requests focused on a single change
- Update documentation if your change affects the public API

## Pull Requests

1. Ensure all tests pass
2. Update `CHANGELOG.md` with a description of your changes
3. Submit your pull request with a clear title and description
4. Link any related issues

## Reporting Issues

Open an issue on GitHub with:
- A clear description of the problem
- Steps to reproduce (if applicable)
- Expected vs actual behavior
- Your .NET version and OS

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
