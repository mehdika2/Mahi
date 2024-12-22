<p align="center">
    <img src="/images/mahi.png" alt="Mahi logo"
     title="Mahi logo designed by Freepik.com" width="200">
</p>

# Mahi interpreter

Mahi is a Lua-based Web Application, utilizing the Fardin web server for handling HTTP requests.

The primary purpose of this program is to host and execute web pages that are dynamically interpreted using the Lua scripting language.

## Dependencies
This project depends on the following repository:
- [Fardin Web Server](https://github.com/mehdika2/fardin)
- [NLua](https://github.com/NLua/NLua)
- Newtonsoft.Json


## Prerequisites
Before you can run this project, you need to have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0.0 or later)

## Run Project

1. **Install .NET SDK**:
   - Download and install the .NET SDK from the official website: [Download .NET](https://dotnet.microsoft.com/download).

2. **Clone the Dependent Project**:
   - Clone the repository of the dependent project:
     ```bash
     git clone https://github.com/mehdika2/fardin
     ```

3. **Build the Dependent Project**:
   - Navigate to the cloned directory:
     ```bash
     cd fardin
     ```
   - Build the project to generate the DLL:
     ```bash
     dotnet build
     ```

4. **Add Reference to the DLL**:
   - After building, locate the generated DLL file (usually found in the `bin/Debug/netX.X/` directory).
   - Add a reference to this DLL in your main project (Visual studio):
     - Right-click on your project in Visual Studio > Add > Reference > Browse and select the DLL.

5. **Compile Your Project**:
   - Navigate back to your main project directory:
     ```bash
     cd ../mahi
     ```
   - Compile your project:
     ```bash
     dotnet build
     ```

6. **Create Page Files**:
   - Ensure that the page files are placed in the `wwwapp` folder:

7. **Place Required Modules**:
   - You can download modules and palce it in `modules` folder:

## Credits
- Logo designed by [Freepik](https://www.freepik.com)