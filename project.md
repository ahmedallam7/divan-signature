# UUNATEK Printer API - Project Knowledge Base

## Project Overview

This is a **handwriting pen plotter/printer control system** for the UUNATEK device. The project provides two interfaces to control a physical pen plotter:
- A **REST API** for programmatic control
- A **Windows Forms desktop application** with a graphical interface

The system converts SVG (Scalable Vector Graphics) files into G-code commands and sends them to a pen plotter via serial communication (COM port) to physically draw/write on paper.

---

## Project Structure & Architecture

The solution follows a **clean architecture** pattern with four projects:

```
divan-signature/
├── UUNATEK.API/                    # ASP.NET Core Web API
├── UUNATRK.Application/            # Core business logic library
├── UUNATEK.Domain/                 # Domain entities and enums
├── UUNATEK.WindowsForm/            # Windows Forms desktop client
├── UUNATEK.API.slnx                # Solution file
├── UUNATEK.API.postman_collection.json  # API testing collection
└── Documentation files (DOCS.doc, UUNATEK Documentation.docx)
```

**Architecture Pattern:**
- **Presentation Layer:** API controllers + Windows Forms UI
- **Application Layer:** Business logic, services, models, repositories
- **Domain Layer:** Entities and enums
- **Data Layer:** Entity Framework Core with SQL Server LocalDB

---

## Technology Stack

**Framework & Runtime:**
- **.NET 10.0** (latest version)
- C# with nullable reference types enabled
- Implicit usings enabled

**Key Technologies:**

**UUNATEK.API (Web API):**
- ASP.NET Core Web API
- Microsoft.AspNetCore.OpenApi (10.0.1) - for OpenAPI/Swagger documentation
- Microsoft.EntityFrameworkCore.Tools (10.0.5) - for database migrations
- Dependency Injection

**UUNATEK.Domain (Domain Layer):**
- No external dependencies - pure domain models

**UUNATRK.Application (Core Library):**
- System.IO.Ports (10.0.0) - for serial port communication
- Microsoft.EntityFrameworkCore (10.0.5) - ORM
- Microsoft.EntityFrameworkCore.SqlServer (10.0.5) - SQL Server provider
- Microsoft.Extensions.Configuration.Abstractions (10.0.5)
- Microsoft.Extensions.DependencyInjection.Abstractions (10.0.5)
- Microsoft.Extensions.Options.ConfigurationExtensions (10.0.5)

**UUNATEK.WindowsForm (Desktop Client):**
- Windows Forms (.NET 10.0-windows)
- Microsoft.Extensions.DependencyInjection (10.0.5)
- Svg library (3.4.7) - for SVG rendering/preview

**Communication Protocol:**
- Serial Port (RS-232) communication
- G-code command language (standard CNC/3D printer control protocol)
- Default: COM5, 250000 baud rate

---

## Main Features & Functionality

**Core Features:**

1. **Serial Port Connection Management**
   - Connect/disconnect to printer via COM port
   - Configurable port and baud rate
   - Connection status monitoring

2. **SVG to G-code Conversion**
   - Comprehensive SVG parser supporting all SVG elements:
     - Paths (with all commands: M, L, H, V, C, S, Q, T, A, Z)
     - Lines, rectangles, circles, ellipses
     - Polylines and polygons
   - Bezier curve flattening (cubic and quadratic)
   - Arc segment approximation
   - Text must be converted to paths in Inkscape first

3. **Print Transformation Controls**
   - Scale multiplier (minimum 1x)
   - Rotation (0-360 degrees)
   - X/Y position offset on paper
   - X/Y axis inversion (mirroring)
   - Multiple paper size presets

4. **Printing Capabilities**
   - Single copy printing
   - Bulk printing (1-100 copies)
   - Automatic paper insertion handshake
   - Automatic paper ejection after printing
   - Real-time print status tracking

5. **Paper Size Support**
   - Standard sizes: A3, A4, A5, A6
   - ISO B-series: B4, B5
   - US sizes: Letter, Legal, Tabloid
   - Envelopes: #10, #9, C5, C6, DL
   - Cards: 4x6", 5x7"
   - Custom dimensions support

6. **Windows Forms Features**
   - Live preview simulation of print output
   - Visual paper representation with SVG overlay
   - G-code preview before printing
   - Real-time transformation preview

7. **Image-Based Print Workflow with Approval** (New Feature)
   - Receive paper image (camera capture) from external applications
   - Send image to approval service for validation
   - Support both file upload and Base64 image formats
   - Automatic void print (paper eject without printing) on rejection
   - Complete request logging to SQL Server database
   - Retry logic for failed print operations (configurable max retries)

---

## Project Components Detailed Analysis

### UUNATRK.Application (Core Business Logic)

#### Models (`Models/`)

**1. PrintRequest.cs**
- Input model for print jobs
- Properties:
  - `Paper?` - Paper size enum (optional)
  - `Width`, `Height` - Custom dimensions (default: 210mm x 297mm)
  - `XPosition`, `YPosition` - Offset on paper (default: 50mm x 50mm)
  - `Scale` - Scale multiplier (default: 1)
  - `Rotation` - Rotation in degrees (default: 0)
  - `InvertX`, `InvertY` - Mirroring flags (InvertY default: true)

**2. PrintResponse.cs**
- Output model for print operations
- Properties: Message, CommandsSent, Copies, TotalCommandsSent

**3. PrinterStatus.cs**
- Status model
- Properties: IsOpen, PortName, IsPrinting

**4. PrinterSettings.cs**
- Configuration model
- Properties: ComPort (default: COM5), BaudRate (default: 250000)

**5. PrintWithApprovalRequest.cs** (New)
- Wraps PrintRequest with approval control
- Properties:
  - `PrintSettings` - Nested PrintRequest object (optional)
  - `ShouldApprove` - Test flag to control approval simulation

**6. PrintWithApprovalResponse.cs** (New)
- Response for print-with-approval workflow
- Properties: RequestId, Status, WasApproved, WasPrinted, Message, CommandsSent

**7. FileStorageSettings.cs** (New)
- Configuration for file uploads
- Properties: UploadPath, MaxImageSizeMB, MaxSvgSizeMB

**8. PrintRetrySettings.cs** (New)
- Configuration for print retry logic
- Properties: MaxRetries (default: 3), RetryDelayMs (default: 1000)

#### Enums (`Enums/`)

**Paper.cs**
- Paper size enumeration with 17 predefined sizes
- `PaperSizes` static class with `GetSizeMm()` method returning (width, height) tuples

---

### UUNATEK.Domain (Domain Layer) - New

#### Entities (`Entities/`)

**RequestLog.cs**
- Main entity for tracking print requests
- Properties:
  - `Id` - Primary key (Guid)
  - `RequestId` - Business identifier (Guid, indexed)
  - `PaperImagePath` - Path to uploaded paper image (optional)
  - `SignatureSvgPath` - Path to uploaded signature SVG (optional)
  - `Status` - Current request status (enum, indexed)
  - `ApprovalResponse` - Response message from approval service (optional)
  - `ErrorMessage` - Error details if failed (optional)
  - `CreatedAt` - Creation timestamp (indexed)
  - `UpdatedAt` - Last update timestamp
  - `CompletedAt` - Completion timestamp (optional)

#### Enums (`Enums/`)

**RequestStatus.cs**
- Workflow status enumeration
- Values: New, WaitingForApproval, Approved, Rejected, Printing, Printed, Voided, Completed, Failed

---

### UUNATRK.Application - Data Layer (New)

#### DbContext (`Data/`)

**ApplicationDbContext.cs**
- Entity Framework Core context
- DbSet: RequestLogs
- Configuration: Indexes on RequestId, Status, and CreatedAt

#### Repositories (`Repositories/`)

**IRequestLogRepository.cs / RequestLogRepository.cs**
- Repository pattern for RequestLog entity
- Methods:
  - `CreateAsync()` - Create new request log
  - `GetByIdAsync()` - Get by primary key
  - `GetByRequestIdAsync()` - Get by business identifier
  - `UpdateAsync()` - Update existing record
  - `GetRecentAsync()` - Get recent requests (paginated)

#### Services (`Services/Approval/`)

**IApprovalService.cs**
- Abstraction for approval service integration
- Method: `RequestApprovalAsync(imagePath, requestId)` → ApprovalResponse

**MockApprovalService.cs**
- Mock implementation of approval service
- Simulates 500ms API delay
- Always returns approved (for testing)
- Configuration via ApprovalServiceSettings

**ApprovalServiceSettings.cs**
- Configuration for approval service
- Properties: Endpoint, ApiKey, TimeoutSeconds, UseMockService

--- Services (`Services/Printer/`)

**1. PrinterService.cs** - Main printer control service

**Serial Communication:**
- Opens/closes serial port with proper configuration
- 8 data bits, no parity, 1 stop bit, ASCII encoding
- DTR/RTS enabled, configurable timeouts

**Print Workflow:**
```
1. Send M998R handshake command
2. Wait for "paper ready" response (60s timeout)
3. Initialize printer (G92, G21, G90, G1 commands)
4. Send all G-code commands
5. Eject paper (pen up, move to eject position, motor control)
```

**Key Methods:**
- `OpenPort()` - Establish serial connection
- `ClosePort()` - Disconnect
- `Print()` - Single print job
- `BulkPrint()` - Multiple copies
- `VoidPrint()` - Void print cycle (paper eject without printing) (New)
- `GetStatus()` - Current printer state
- `ExecutePrintCycle()` - Complete print workflow
- `ExecuteVoidCycle()` - Void print workflow (pen stays at E0.0) (New)
- `EjectPaper()` - Safe paper ejection sequence

**2. SvgConverter.cs** - SVG to G-code conversion engine

**SVG Parsing:**
- ViewBox and dimension handling
- Unit conversion (px, mm, cm, in, pt) to millimeters
- Standard 96 DPI conversion (1px = 25.4/96 mm)
- Recursive element extraction

**Path Data Parser:**
- Full SVG path command support
- Tokenization with regex
- Relative/absolute coordinate handling
- Implicit command repetition

**Curve Flattening:**
- Cubic Bezier: Adaptive recursive subdivision (max depth 12)
- Quadratic Bezier: Conversion to cubic then flattening
- Arc: Endpoint-to-center parameterization with segment approximation
- Flatness tolerance: 0.5mm
- Arc segments: 72 per full circle

**Transformation Pipeline:**
```
1. Remove viewBox min offset
2. Apply scale (SVG units → mm)
3. Apply X/Y inversions (within scaled bounds)
4. Apply rotation (around center)
5. Add user offset (final paper position)
```

**G-code Generation:**
- Pen movements: E0.0 (full up), E4.0 (intermediate), E8.0 (down)
- Rapid moves: G0 commands at F6000 (6000 mm/min)
- Draw moves: G1 commands at F5000 (5000 mm/min)
- Continuous strokes: pen down once per polyline
- Stroke breaks: intermediate lift between paths

#### Dependency Injection (`DependencyInjection.cs`)

- Extension method: `AddApplicationServices()`
- Registers `PrinterService` as singleton
- Binds `PrinterSettings` from configuration

---

### UUNATEK.API (REST API)

#### Entry Point (`Program.cs`)

- Minimal API setup
- Registers application services
- Adds controllers and OpenAPI support
- OpenAPI endpoint only in Development mode
- Runs on http://localhost:5100 (dev) and https://localhost:7086 (prod)

#### Controllers (`Controllers/`)

**PrinterController.cs** - Main API controller
- Route: `/printer`

**Endpoints:**

1. **Connection Management:**
   - `POST /printer/connect` - Connect to printer
     - Query params: comPort (optional), baudRate (optional)
     - Returns: Success/error message
     - Conflict if already connected
   
   - `POST /printer/disconnect` - Disconnect from printer
     - Returns: Success message
     - Conflict if printing or not connected

2. **Status:**
   - `GET /printer/status` - Get current printer status
     - Returns: PrinterStatus object (IsOpen, PortName, IsPrinting)

3. **G-code Generation:**
   - `POST /printer/generate` - Convert SVG to G-code without printing
     - Accepts: multipart/form-data with SVG file + PrintRequest
     - Returns: G-code array and command count
     - Validates: scale ≥1, rotation 0-360

4. **Printing:**
   - `POST /printer/print` - Print single copy
     - Accepts: multipart/form-data with SVG file + PrintRequest
     - Returns: PrintResponse
     - Requires: connection established, printer not busy
   
   - `POST /printer/print/bulk` - Print multiple copies (1-100)
     - Accepts: multipart/form-data with SVG file + copies + PrintRequest
     - Returns: PrintResponse with total commands sent
     - Validates: 1 ≤ copies ≤ 100

5. **Print with Approval (New):**
   - `POST /printer/print-with-approval` - Image-based print workflow with approval
     - Accepts: multipart/form-data with paperImage (file or base64), signatureSvg file, printRequestJson
     - Workflow:
       1. Saves paper image and signature SVG to storage
       2. Logs request to database with status tracking
       3. Sends paper image to approval service
       4. If approved: converts SVG and prints signature
       5. If rejected: performs void print (ejects paper without printing)
     - Returns: PrintWithApprovalResponse with RequestId, Status, WasApproved, WasPrinted
     - Retry logic: Automatically retries failed prints (configurable max retries)
   
   - `GET /printer/requests/{requestId}` - Get request log by ID
     - Returns: RequestLog entity with full workflow history

#### Configuration

- `appsettings.json` - Contains multiple configuration sections:
  - `Printer` - ComPort and BaudRate
  - `ConnectionStrings` - DefaultConnection to SQL Server LocalDB
  - `ApprovalService` - Endpoint, ApiKey, TimeoutSeconds, UseMockService
  - `FileStorage` - UploadPath, MaxImageSizeMB, MaxSvgSizeMB
  - `PrintRetry` - MaxRetries, RetryDelayMs
- `launchSettings.json` - HTTP (5100) and HTTPS (7086, 5100) profiles
- Both profiles have launchBrowser disabled

---

### UUNATEK.WindowsForm (Desktop Application)

#### Entry Point (`Program.cs`)

- STAThread for COM compatibility
- Dependency injection setup
- Creates service provider
- Launches Form1 with PrinterService

#### Main Form (`Form1.cs`, `Form1.Designer.cs`)

**UI Sections:**

1. **Serial Connection (grpConnection):**
   - COM port text box (default: COM5)
   - Baud rate text box (default: 250000)
   - Connect button
   - Connection state feedback

2. **Printer Status (grpStatus):**
   - IsOpen label
   - PortName label
   - IsPrinting label
   - Refresh button

3. **Simulation Panel (grpSimulation):**
   - Paper size dropdown (all Paper enum values except Custom)
   - Browse button for SVG file selection
   - File name display
   - PictureBox for live preview:
     - Gray background
     - White paper rectangle (scaled to fit)
     - SVG rendering with transformations applied
     - Real-time update on settings change
     - Clipping to paper bounds

4. **Print Settings Panel (grpPrintSettings):**
   - X Position textbox (default: 50mm)
   - Y Position textbox (default: 50mm)
   - Scale numeric up/down (minimum: 1)
   - Rotation numeric up/down (0-360)
   - Invert X checkbox
   - Invert Y checkbox (default: checked)
   - G-code preview textbox (readonly, scrollable, Consolas font)
   - Generate G-code button
   - Print button (bold, larger)
   - Result label (color-coded: green=success, red=error, orange=warning)

**Key Features:**
- **Live Preview:** Uses Svg library to render SVG with all transformations
- **Settings Synchronization:** All controls trigger `UpdateSimulation()` on change
- **Responsive Layout:** Anchored controls, minimum size 750x550
- **Error Handling:** MessageBox dialogs for connection and print errors
- **Async Printing:** `btnPrint_Click` is async for non-blocking UI

---

## API Endpoints Summary

| Method | Endpoint | Purpose | Input | Output |
|--------|----------|---------|-------|--------|
| POST | `/printer/connect` | Connect to printer | Query: comPort?, baudRate? | Success message |
| POST | `/printer/disconnect` | Disconnect from printer | None | Success message |
| GET | `/printer/status` | Get printer status | None | PrinterStatus |
| POST | `/printer/generate` | Generate G-code preview | FormData: svg file + PrintRequest | G-code array |
| POST | `/printer/print` | Print single copy | FormData: svg file + PrintRequest | PrintResponse |
| POST | `/printer/print/bulk` | Print multiple copies | FormData: svg file + copies + PrintRequest | PrintResponse |
| POST | `/printer/print-with-approval` | Print with approval workflow | FormData: paperImage + signatureSvg + printRequestJson | PrintWithApprovalResponse |
| GET | `/printer/requests/{requestId}` | Get request log | Path: requestId | RequestLog |

---

## External Integrations

**Hardware Integration:**
- **Serial Port Communication:** Direct COM port access via System.IO.Ports
- **UUNATEK Printer:** Physical pen plotter device
- **Protocol:** G-code (CNC/3D printer standard)
- **Custom Commands:** M998R (handshake), M106/M107 (motor control), M400 (wait)

**Database Integration:** (New)
- **SQL Server LocalDB:** (localdb)\mssqllocaldb
- **Database:** UunatekPrinter
- **ORM:** Entity Framework Core 10.0.5
- **Tables:** RequestLogs with indexes on RequestId, Status, CreatedAt

**External Services:** (New)
- **Approval Service:** Configurable HTTP endpoint for paper image validation
- **Mock Implementation:** Available for testing (500ms simulated delay)
- **Future Integration:** Real approval service endpoint configurable in appsettings.json

---

## Configuration & Settings

**API Configuration (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UunatekPrinter;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Printer": {
    "ComPort": "COM5",
    "BaudRate": 250000
  },
  "ApprovalService": {
    "Endpoint": "https://future-approval-api.example.com/approve",
    "ApiKey": "",
    "TimeoutSeconds": 30,
    "UseMockService": true
  },
  "FileStorage": {
    "UploadPath": "wwwroot/uploads",
    "MaxImageSizeMB": 10,
    "MaxSvgSizeMB": 5
  },
  "PrintRetry": {
    "MaxRetries": 3,
    "RetryDelayMs": 1000
  }
}
```

**Launch Profiles:**
- Development: http://localhost:5100
- Production: https://localhost:7086; http://localhost:5100
- OpenAPI available in Development mode only

**Windows Form Defaults:**
- COM Port: COM5
- Baud Rate: 250000
- Paper: A4
- Position: 50mm, 50mm
- Scale: 1
- Rotation: 0°
- Invert Y: true (default)

---

## Key Algorithms & Logic

### SVG Parsing Algorithm

1. Load and parse XML document
2. Extract viewBox or width/height attributes
3. Determine unit-to-millimeter conversion factor
4. Recursively traverse all elements
5. Convert each element to polylines (point lists)
6. Apply transformations to all points
7. Generate G-code with pen up/down commands

### Print Cycle Workflow

```
START
  ↓
Send M998R handshake
  ↓
Wait for "paper ready" (timeout: 60s)
  ↓
Initialize coordinates & units (G92, G21, G90, G1)
  ↓
For each polyline:
  • Pen up (if needed)
  • Rapid move to start point (G0)
  • Pen down (E8.0)
  • Draw line segments (G1)
  ↓
Eject paper:
  • Pen up (E0.0)
  • Move X to 215mm
  • Start motor (M106)
  • Push paper Y to 500mm
  • Wait (M400)
  • Stop motor (M107)
  ↓
END
```

### Transformation Order (Critical for Correct Output)

1. ViewBox offset removal
2. Scale application (SVG units → mm)
3. Inversions (within scaled bounds)
4. Rotation (around scaled center)
5. User offset (final paper positioning)

### Print with Approval Workflow (New Feature)

```
START
  ↓
Save paper image and signature SVG to storage
  ↓
Create RequestLog (Status: New)
  ↓
Update Status: WaitingForApproval
  ↓
Send paper image to approval service
  ↓
Check ShouldApprove flag and approval response
  ↓
Update Status: Approved or Rejected
  ↓
IF REJECTED:
  • Update Status: Voided
  • Execute void print cycle:
    - Send M998R handshake
    - Wait for "paper ready"
    - Keep pen at E0.0 (up) entire time
    - Eject paper without printing
  • Update Status: Completed
  • Return response (WasApproved: false, WasPrinted: false)
  ↓
IF APPROVED:
  • Convert SVG to G-code
  • Update Status: Printing
  • Execute print with retry logic:
    - Attempt 1: Try print
    - If fails and retries remaining:
      * Wait RetryDelayMs
      * Attempt 2, 3, etc. (up to MaxRetries)
    - If all attempts fail:
      * Update Status: Failed
      * Log error message
      * Throw exception
  • Update Status: Printed
  • Update Status: Completed
  • Return response (WasApproved: true, WasPrinted: true)
  ↓
END
```

---

## Testing & Documentation

**API Testing:**
- Postman collection included: `UUNATEK.API.postman_collection.json`
- Contains all 7 endpoints with examples
- Variable: `{{baseUrl}}` = http://localhost:5000 (note: should be 5100)
- Includes detailed descriptions and parameter documentation

**Documentation Files:**
- `UUNATEK Documentation.docx` - User documentation (25KB)
- `DOCS.doc` - Additional documentation (297KB)
- HTTP test file: `UUNATEK.API.http` (VS Code REST Client format)

---

## Deployment & Build

**Build Configuration:**
- All projects target .NET 10.0
- Release and Debug configurations
- Output: bin/Debug or bin/Release folders
- No Docker or containerization setup
- Windows-only deployment (due to Windows Forms and COM port requirements)

**Dependencies Installation:**
```bash
dotnet restore
dotnet build
```

**Run API:**
```bash
cd UUNATEK.API
dotnet run
```

**Run Windows Form:**
```bash
cd UUNATEK.WindowsForm
dotnet run
```

**Database Setup:** (New)
```bash
# Initial migration already created
# To apply migrations (database created automatically on first run):
cd UUNATEK.API
dotnet ef database update

# To create new migration (if needed):
dotnet ef migrations add MigrationName --project ../UUNATRK.Application
```

---

## Code Quality & Patterns

**Design Patterns Used:**
- **Dependency Injection:** Services registered and injected
- **Singleton Pattern:** PrinterService is singleton (single hardware connection)
- **Repository Pattern:** Used for data access (RequestLogRepository) (New)
- **Clean Architecture:** Separation of concerns across projects (Domain, Application, API layers)

**Code Characteristics:**
- **Modern C# Features:** Records, nullable reference types, pattern matching, switch expressions
- **Error Handling:** Try-catch blocks with meaningful error messages
- **Logging:** Console.WriteLine for debugging (no structured logging framework)
- **Thread Safety:** Printing flag prevents concurrent operations
- **Async/Await:** Used in print operations for non-blocking execution

---

## Database Models & Entities

**Database:** SQL Server LocalDB - UunatekPrinter

**Entity Framework Core Configuration:**
- DbContext: `ApplicationDbContext`
- Connection: (localdb)\mssqllocaldb
- Provider: Microsoft.EntityFrameworkCore.SqlServer 10.0.5

**Entities:**

1. **RequestLog** (Table: RequestLogs)
   - Primary Key: `Id` (Guid)
   - Business Key: `RequestId` (Guid, indexed)
   - `PaperImagePath` (string?, nullable) - File path to uploaded paper image
   - `SignatureSvgPath` (string?, nullable) - File path to uploaded signature SVG
   - `Status` (RequestStatus enum, indexed) - Current workflow status
   - `ApprovalResponse` (string?, nullable) - Message from approval service
   - `ErrorMessage` (string?, nullable) - Error details if failed
   - `CreatedAt` (DateTime, indexed) - Request creation time
   - `UpdatedAt` (DateTime) - Last modification time
   - `CompletedAt` (DateTime?, nullable) - Completion time

**Enums:**

1. **RequestStatus**
   - New - Request created
   - WaitingForApproval - Sent to approval service
   - Approved - Approval service approved
   - Rejected - Approval service rejected
   - Printing - Currently printing
   - Printed - Print completed
   - Voided - Paper ejected without printing
   - Completed - Workflow finished successfully
   - Failed - Workflow failed with error

**Indexes:**
- `IX_RequestLogs_RequestId` (RequestId)
- `IX_RequestLogs_Status` (Status)
- `IX_RequestLogs_CreatedAt` (CreatedAt)

**File Storage:**
- Images: `wwwroot/uploads/images/{requestId}.jpg`
- SVGs: `wwwroot/uploads/svgs/{requestId}.svg`

---

## Potential Improvements & Observations

**Observations:**
1. No authentication/authorization on API endpoints
2. No structured logging (Serilog, NLog, etc.)
3. Single printer connection (cannot manage multiple printers)
4. Postman collection baseUrl mismatch (5000 vs 5100)
5. No unit tests or integration tests
6. Hard-coded timeout values
7. ~~No retry logic for serial communication failures~~ - **Retry logic added** for print operations (configurable) ✅
8. No configuration validation
9. Simulation uses Svg library for rendering (good UX)
10. Comprehensive SVG support (excellent parser implementation)
11. **Database integration added** for request logging and tracking ✅
12. **Approval workflow implemented** with mock service for testing ✅
13. **Void print functionality** for rejected requests (paper eject without printing) ✅

**Security Considerations:**
- API is completely open (no authentication)
- File upload with configurable size limits (MaxImageSizeMB, MaxSvgSizeMB)
- File validation by extension only (jpg for images, svg for signatures)
- No rate limiting on endpoints
- Database uses LocalDB (development/local environment)
- Suitable for local network or trusted environments only

---

## G-code Commands Reference

**Movement Commands:**
- `G0 X{x} Y{y} F6000` - Rapid positioning at 6000 mm/min
- `G1 X{x} Y{y} F5000` - Linear move (drawing) at 5000 mm/min
- `G21` - Set units to millimeters
- `G90` - Absolute positioning mode
- `G92 X0 Y0 E0` - Set current position as origin

**Pen Control (Extruder Commands):**
- `E0.0` - Pen fully up (travel position)
- `E4.0` - Pen intermediate position
- `E8.0` - Pen down (drawing position)

**Motor & Paper Control:**
- `M998R` - Paper insertion handshake command
- `M106` - Start paper feed motor
- `M107` - Stop paper feed motor
- `M400` - Wait for moves to finish

---

## File Locations Reference

**Domain Layer:** (New)
- `UUNATEK.Domain/Entities/RequestLog.cs` - Main entity for request tracking
- `UUNATEK.Domain/Enums/RequestStatus.cs` - Workflow status enumeration

**Core Business Logic:**
- `UUNATRK.Application/Services/Printer/PrinterService.cs` - Serial communication and print management
- `UUNATRK.Application/Services/Printer/SvgConverter.cs` - SVG parsing and G-code generation
- `UUNATRK.Application/Services/Approval/IApprovalService.cs` - Approval service interface (New)
- `UUNATRK.Application/Services/Approval/MockApprovalService.cs` - Mock approval implementation (New)
- `UUNATRK.Application/Services/Approval/ApprovalServiceSettings.cs` - Approval configuration (New)
- `UUNATRK.Application/Repositories/IRequestLogRepository.cs` - Repository interface (New)
- `UUNATRK.Application/Repositories/RequestLogRepository.cs` - Repository implementation (New)
- `UUNATRK.Application/Data/ApplicationDbContext.cs` - EF Core context (New)
- `UUNATRK.Application/Migrations/` - Database migrations (New)
- `UUNATRK.Application/Models/` - Data transfer objects
- `UUNATRK.Application/Enums/Paper.cs` - Paper size definitions

**API:**
- `UUNATEK.API/Controllers/PrinterController.cs` - REST API endpoints
- `UUNATEK.API/Program.cs` - API startup configuration
- `UUNATEK.API/appsettings.json` - API configuration
- `UUNATEK.API/wwwroot/uploads/images/` - Uploaded paper images (New)
- `UUNATEK.API/wwwroot/uploads/svgs/` - Uploaded signature SVGs (New)

**Desktop Application:**
- `UUNATEK.WindowsForm/Form1.cs` - Main UI logic
- `UUNATEK.WindowsForm/Form1.Designer.cs` - UI layout and controls
- `UUNATEK.WindowsForm/Program.cs` - Application entry point

---

## Summary

This is a **well-architected hardware control application** for a pen plotter device with comprehensive workflow management. The codebase demonstrates:

✅ **Clean separation of concerns** (Domain, Application, API, UI layers)  
✅ **Comprehensive SVG parsing** with full path command support  
✅ **Dual interface** (API + Desktop) for different use cases  
✅ **Modern .NET features** (.NET 10, nullable types, DI, EF Core)  
✅ **Production-ready serial communication** handling  
✅ **Excellent user experience** in Windows Forms with live preview  
✅ **Image-based approval workflow** with database tracking (New)  
✅ **Robust error handling** with automatic retry logic (New)  
✅ **Void print capability** for rejected requests (New)

The system is designed for **local/lab environments** where a physical printer is connected via USB/Serial. It successfully bridges the gap between vector graphics (SVG) and physical pen plotting through G-code translation, while providing comprehensive tracking and approval workflows for production scenarios.
