# Base64Exporter

Base64Exporter is a Windows Forms application for converting any file to its Base64 representation and saving the result as a `.txt` file. The conversion is performed efficiently using streaming, so even large files can be processed without excessive memory usage.

## Features

- Select any file from your computer.
- Convert the file to Base64 format.
- Save the Base64 output as a `.txt` file.
- Progress bar for conversion status.
- Efficient streaming conversion (no memory bloat).
- Error logging for troubleshooting.

## Requirements

- .NET 8.0 (Windows)
- Windows OS

## Usage

1. Launch the application.
2. Click **Select File** to choose the file you want to convert.
3. Click **Convert to Base64 and save as .txt**.
4. Choose the destination `.txt` file.
5. Wait for the conversion to complete. The progress bar will indicate status.

## Building

To build the project:

1. Ensure you have .NET 8.0 SDK installed.
2. Clone the repository.
3. Open the solution in Visual Studio.
4. Restore NuGet packages.
5. Build the solution.
6. Run the application.
7. Enjoy converting files to Base64!

## Contact

- For questions or support, please open an issue in the GitHub repository.

## Acknowledgments

- Inspired by the need for efficient large file conversion tools.