# Savchuk Dmytro IPZk-24-1
# Front-end Part

# QR Code Generator & Reader

This is the back-end part of the application for generating and reading QR codes, with additional features such as customizing appearance and uploading a logo.


*Requirements: .NET 9*

---

## ⚙️ Features

- Generate QR codes from URLs with:
    - Custom pixel size
    - Colors (foreground)
    - Bubble style (rounded modules)
    - Logo embedding
    - Transparent background support
- Read QR codes from uploaded images (with preprocessing)
- Collect QR code generation statistics
- Export statistics to Excel:
    - Summary report
    - Detailed log
- CORS support for frontend integration

---

## 🧭 Programming Principles

1. **Single Responsibility Principle (SRP)**  
   Each service has a single clear responsibility.  
   _Example:_ `QrCodeBuilder` only generates QR codes, `StatisticsExcelService` handles Excel logic.

2. **Don't Repeat Yourself (DRY)**  
   Shared logic is abstracted into reusable helpers and services.

3. **Separation of Concerns (SoC)**  
   Controllers handle routing, validation; services handle core logic.

4. **Open/Closed Principle (OCP)**  
   Easily extendable (e.g., new QR styles) without modifying existing core logic.

5. **KISS (Keep It Simple, Stupid)**  
   Clean and readable codebase — no overengineering or redundant abstractions.

---

## 🛠 Refactoring Techniques

- **Extract Method**  
  Image preprocessing moved into `ApplyMorphologicalAdapter()` for clarity.

- **Encapsulate Conditionals**  
  QR code color and transparency logic isolated into specific methods.

- **Service Decomposition**  
  Logic split into: `QrCodeBuilder`, `QrCodeReader`, `StatisticsExcelService`.

- **Consistent Naming**  
  Methods and variables named clearly (e.g., `GenerateTransparentQr()`).

- **Separation of Layers**  
  API logic (controllers) clearly separated from business logic (services).

---

## 🎯 Design Patterns

- **Builder Pattern**  
  `QrCodeBuilder` uses fluent interface for QR customization.

- **Facade Pattern**  
  `StatisticsExcelService` abstracts complex Excel logic into a simple interface.

- **Dependency Injection**  
  ASP.NET Core injects all services — enabling unit testing and modularity.

- **Helper Utilities**  
  Specialized utility classes support image preprocessing and Excel export.

---


🚀 Run Locally
Requirements:

    .NET 9 SDK
    Visual Studio / Rider / CLI


Steps:
````
git clone 
dotnet restore
dotnet run