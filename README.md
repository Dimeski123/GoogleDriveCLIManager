# Google Drive CLI Manager

An enterprise-grade, interactive command-line interface (CLI) application built with .NET 8 for seamless integration and synchronization with Google Drive.

This tool offerins a continuous **Interactive REPL (Read-Eval-Print Loop)** powered by `Spectre.Console`, built upon a strict **Clean Architecture** foundation.

## Features

-   **Interactive REPL UI:** Console environment with ASCII headers, color-coded tables, and real-time interactive prompts.
    
-   **Smart Synchronization:** Mirrors your Google Drive locally using `Paralel Downloading`. Synchronizes the cloud file with your local file directory by deleting, adding and updating you files/folders.
    
-   **Dynamic Search:** Queries the cloud and cross-references your local hard drive to display real-time sync statuses (`Up To Date` or `Not Downloaded`).
    
-   **Intelligent Uploads:** Upload files using human-readable paths (e.g., `upload "file.txt" -d "Projects/2026"`). If the destination folders do not exist, the CLI will automatically create them in Google Drive.
    
-   **Resilient API Calls:** Wraps all Google Drive API network traffic in `Polly` retry policies to seamlessly handle rate limits and transient network drops.
    
-   **Secure OAuth 2.0:** Authenticate once via the browser. The CLI locally encrypts and caches your token. Includes a dedicated `logout` command for shared machines.

## Setup & Installation

### 1. Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.
    
-   A Google Cloud Console account.
    

### 2. Configure Google Credentials

1.  Go to the [Google Cloud Console](https://console.cloud.google.com/).
    
2.  Enable the **Google Drive API** for your project.
    
3.  Create new **OAuth 2.0 Client IDs** (Application Type: Desktop App).
    
4.  Download the JSON credential file and rename it to `client_secret.json`.
    

### 3. Add Secrets to the Project

Place the `client_secret.json` file inside the `Secrets` folder in your Presentation project:
```
GoogleDriveCLIManager/
├── Secrets/
│   └── client_secret.json <-- Here
├── appsettings.json
└── Program.cs
```

## Application Configuration (`appsettings.json`)

The CLI's behavior can be fine-tuned without recompiling the code by editing the `appsettings.json` file located in the root directory.

```json
{
  "SyncSettings": {
    "MaxParallelDownloads": 5,
    "RetryCount": 3,
    "RetryDelaySeconds": 2
    // "DefaultDownloadPath": "C:\\MyCustomSyncFolder"
  }
}
```
#### Configuration Options:

-   **`MaxParallelDownloads`**: Controls how many files the `sync` command will attempt to download simultaneously. _Note: Setting this too high may trigger Google Drive API rate limits._
    
-   **`RetryCount`**: The number of times the Polly resilience engine will attempt to retry a failed API call before marking the file as a failed download.
    
-   **`RetryDelaySeconds`**: The base delay for the exponential backoff strategy between retries.
    
-   **`DefaultDownloadPath`**: By default, the CLI syncs files to your machine's `User\Downloads\GoogleDriveRoot` folder. To force a specific global path, uncomment this line and provide an absolute path.

## Publishing as a Standalone `.exe`

To run this application as a true desktop tool without needing Visual Studio, you can publish it into a self-contained executable.

1.  Open your terminal and navigate to the root folder (where the `.sln` or main `.csproj` lives).
    
2.  Run the following command:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
3. Navigate to the output folder:
```
\bin\Release\net8.0\win-x64\publish\
```
4. Double-click **`GoogleDriveCLIManager.exe`** to launch the interactive CLI! _(Make sure `appsettings.json` and the `Secrets` folder are present in the same directory as the `.exe`)._

## Command Reference

Once the application is running, you will be greeted by the `gdrive>` prompt. You can enter commands sequentially without the application closing.
| Command | Usage & Arguments | Options / Flags                                      | Description                                                                                                          |
|---------|-------------------|------------------------------------------------------|------
| **sync**    | `sync`              | `-e, --empty-folders <true/false>` `-p, --path "C:\Path"` | *Mirrors Google Drive to your local machine. Supports downloading empty folders and overriding the destination path.*  |   |   |   |   |   |   |
| **search**  | `search "query"`    | `-p, --path "C:\Path"`                                 | *Searches Drive and compares against your local machine to display sync status. Leave query blank to list all files.*  |   |   |   |   |   |   |
| **upload**  | `upload "filepath"` | `-d, --destination "Folder/Sub"`                       | *Uploads a local file. The destination flag auto-creates missing folders.*                                             |   |   |   |   |   |   |
| **logout**  | `logout`            |                                                      | *Revokes local Google credentials and clears the cached token.*                                                        |   |   |   |   |   |   |
| **clear**   | `clear` or `cls`      |                                                      | *Wipes the console history while preserving the application header.*                                                   |   |   |   |   |   |   |
| **help**    | `help`              | `sync -h` `upload-h` `search -h`                                                  | *Prints the interactive cheat sheet. Use the command name with `-h` at the end to get each command help*                                                                                  |   |   |   |   |   |   |
| **exit**    | `exit` or `quit`      |                                                      | *Closes the application.*                                                                                              |   |   |   |   |   |   |
|

## Architecture Overview

The application strictly follows the **Clean Architecture** principles to separate business rules from UI and framework implementations.

-   **Domain Layer:** Contains enterprise logic, Entities (`Manifest`), and strongly-typed Value Objects (`FileSize`, `Checksum`). Completely isolated from external dependencies.
    
-   **Application Layer:** Contains the `Handlers` and DTOs. Orchestrates the flow of data using interfaces.
    
-   **Infrastructure Layer:** Implements external concerns.
    
    -   `GoogleDriveClient` (API communication).
        
    -   `GoogleAuthenticator` (OAuth management).
        
    -   `LocalFileSystem` (File I/O).
        
    -   `RetryPolicyWrapper` (Polly resilience).
    -  `ManifestRepository` (Manifest to control for Local and Cloud directories)
        
-   **Presentation Layer:** The custom `Spectre.Console` CLI bridge, custom argument parser, and REPL infinite loop (`AppEngine.cs`).

## Under the Hood

### Thread-Safe Parallelism

The `sync` command utilizes `Parallel.ForEachAsync` to download multiple files concurrently. To prevent race conditions during UI updates, statistics tracking (`SyncStatistics`) leverages `Interlocked.Increment()` and concurrent collections.

### State Management & Pruning

Instead of blindly scanning the local hard drive (which risks deleting user files), the application maintains a **Local JSON Manifest** (`.drive_manifest.json`).

-   If a file exists in the Manifest but not in the Cloud, the local file is safely pruned.
    
-   If a file exists in the Cloud, its MD5 checksum and modified dates are compared against the Manifest to skip redundant downloads (Delta Syncing).
    

### Interface Segregation & Dependency Injection

The application utilizes extension methods (`AddApplication()`, `AddInfrastructure()` and `.AddPresentation()`) to keep the `Program.cs` composition root clear and easily-readable.

## Error Handling & Resilience

The application is built to be highly fault-tolerant, ensuring that transient network issues or strict Google API restrictions do not crash a long-running synchronization process.

* **Resilient API Calls (`Polly`):** All external Google Drive API calls are wrapped in a robust retry policy. Transient errors, such as sudden network drops or API rate-limiting, are automatically handled and retried with exponential backoff.
* **Non-Blocking Execution:** If a specific file fails to download or upload, the `Parallel.ForEachAsync` engine catches the exception, logs it safely via a thread-safe `ConcurrentBag`, and seamlessly continues processing the remainder of your files. 
* **Structured UI Reporting:** Failures are never buried in messy console logs. Using `Spectre.Console`, the CLI intercepts errors and renders a clean, color-coded summary table at the end of the execution, displaying the exact file names and the specific API failure reasons.
* **API Quirk Management:** Gracefully catches and explains complex Google API limitations, such as attempting to trigger standard binary downloads on dynamic Google Workspace documents (Docs, Sheets, Slides).
