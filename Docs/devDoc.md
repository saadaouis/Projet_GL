# EasySave Developer Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Development Setup](#development-setup)
3. [Architecture](#architecture)
4. [Code Structure](#code-structure)
5. [Testing](#testing)
6. [Contributing Guidelines](#contributing-guidelines)
7. [API Documentation](#api-documentation)

## Project Overview
EasySave is built using .NET Core and Avalonia UI, following the MVVM pattern. The project emphasizes clean architecture, maintainability, and extensibility.

## Development Setup
### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or JetBrains Rider
- Git

### Environment Setup
1. Clone the repository:
```bash
git clone https://github.com/saadaouis/Projet_GL.git
cd Projet_GL
```

2. Install dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run tests:
```bash
dotnet test
```

## Architecture
The application follows the MVVM (Model-View-ViewModel) pattern with the following layers:

### Models
- `ModelBackup`: Core backup functionality
- `ModelConfig`: Configuration management
- `BackupState`: State tracking
- `BackupLog`: Logging functionality

### Services
- `TranslationService`: Language management
- `CryptosoftService`: Encryption handling
- `LoggingService`: Log management
- `StateRecorder`: State persistence

### ViewModels
- `MainViewModel`: Main application logic
- `BackupViewModel`: Backup operations
- `ConfigViewModel`: Configuration UI logic

### Views
- `MainView`: Main window
- `BackupView`: Backup interface
- `ConfigView`: Settings interface

## Code Structure
```
EasySave/
├── Models/           # Data models
├── Services/         # Business logic services
├── ViewModels/       # View models
├── Views/            # UI components
├── Interfaces/       # Interface definitions
├── Converters/       # Value converters
├── Resources/        # Static resources
└── test/            # Test projects
```

## Testing
### Unit Tests
- Located in the `test` directory
- Uses xUnit framework
- Covers core functionality

### Running Tests
```bash
dotnet test
```

### Test Coverage
- Models: 90%+
- Services: 85%+
- ViewModels: 80%+

## Contributing Guidelines
1. Fork the repository
2. Create a feature branch
3. Follow coding standards
4. Write tests for new features
5. Update documentation
6. Submit pull request

### Coding Standards
- Follow C# coding conventions
- Use XML documentation
- Maintain test coverage
- Follow SOLID principles

## API Documentation

### ModelBackup
```csharp
public class ModelBackup
{
    public string SourcePath { get; set; }
    public string DestinationPath { get; set; }
    public Task<List<string>> FetchProjectsAsync(string directory);
    public Task<BackupState> GetBackupStateAsync(string projectName);
    public Task<bool> SaveProjectAsync(string projectName, bool isDifferential, IProgress progressReporter);
}
```

### TranslationService
```csharp
public interface ITranslationService
{
    string CurrentLanguage { get; }
    string GetTranslation(string key);
    void LoadTranslation();
    event EventHandler LanguageChanged;
}
```

### LoggingService
```csharp
public interface ILoggingService
{
    void Initialize();
    void Log(string message);
    void LogError(string message, Exception ex);
}
```

### StateRecorder
```csharp
public interface IStateRecorder
{
    void WriteState(Dictionary<string, object> info);
    Dictionary<string, object> ReadState();
}
```
