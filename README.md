# Education Center Management System (ECMS)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue)](https://dotnet.microsoft.com/)
[![Next.js Version](https://img.shields.io/badge/Next.js-15.0-black)](https://nextjs.org/)
[![React Version](https://img.shields.io/badge/React-19.0-61dafb)](https://react.dev/)

A modern, enterprise-grade Customer Relationship Management (CRM) and Management Information System (MIS) designed specifically for education centers, language schools, and training institutions. Built with a highly scalable **.NET Clean Architecture** backend and a responsive, interactive **Next.js & React** frontend.

---

## 🧭 Core Business Workflow

The system is designed not just for simple CRUD operations, but to model the real operational lifecycle of an educational institution:

```mermaid
graph TD
    %% Define Styles
    classDef lead fill:#e1f5fe,stroke:#0288d1,stroke-width:2px;
    classDef academic fill:#efebe9,stroke:#5d4037,stroke-width:2px;
    classDef billing fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px;
    classDef admin fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px;

    subgraph Admission ["1. Admission & Trials"]
        A[New Lead / Registration]:::lead --> B[Consultation & Profiling]:::lead
        B --> C[Trial Session Reservation]:::lead
    end

    subgraph Operations ["2. Academic Operations"]
        C --> D[Class Enrollment]:::academic
        D --> E[Class Schedule Assignment]:::academic
        E --> F[Session Scheduling & Calendars]:::academic
        F --> G[Attendance & Status Tracking]:::academic
        G -->|If Absent| H[Makeup Session Booking]:::academic
    end

    subgraph Finance ["3. Billing & Payments"]
        G --> I[Monthly Tuition Calculation]:::billing
        I --> J[Discount / Coupon Application]:::billing
        J --> K[VietQR Dynamic QR Generation]:::billing
        K --> L[Parent Payment via QR]:::billing
        L --> M[API Auto Settlement & Invoicing]:::billing
    end

    subgraph Analytics ["4. Reports & Administrative Payroll"]
        M --> N[Academic & Revenue Reports]:::admin
        G & M --> O[Teacher Payroll Processing]:::admin
        N & O --> P[Action Logs & System Audit]:::admin
    end
```

---

## 🏛️ System Architecture

The project follows the **Clean Architecture** pattern to guarantee maintainability, testability, and independence from external frameworks:

```
[ Presentation (Web API) ]
           │
           ▼
[ Application (Use Cases, MediatR, DTOs) ]
           │
           ▼
[ Domain (Entities, Value Objects, Domain Events) ]
           ▲
           │
[ Infrastructure (EF Core, Database, External APIs) ]
```

*   **`EducationCenter.Crm.Domain`**: Core domain logic, Entities (`Class`, `Student`, `Attendance`, `TuitionInvoice`), Value Objects, and Domain Events.
*   **`EducationCenter.Crm.Application`**: CQRS queries/commands (via MediatR), validation logic (FluentValidation), mapping, and interface definitions.
*   **`EducationCenter.Crm.Infrastructure`**: Database contexts, configurations, migrations, repository patterns, security, and integration services (e.g., Google Calendar, Email notifications).
*   **`EducationCenter.Crm.Api`**: RESTful API Controllers, middleware, authentication (JWT), and application configuration.
*   **`EducationCenter.Crm.McpServer`**: Model Context Protocol (MCP) Server implementing JSON-RPC 2.0 stdio transport to expose CRM operations to AI coding agents.
*   **`frontend`**: Next.js App Router workspace utilizing TypeScript, React hooks, and Tailwind CSS.

---

## 📊 Database Schema (Entity Relationship Diagram)

Below is the logical database design showing relationships between core domains: Identity, Academic, Admissions, and Finance.

```mermaid
erDiagram
    %% Identity Domain
    User ||--o{ UserRole : holds
    Role ||--o{ UserRole : assigns
    Role ||--o{ RolePermission : details
    Permission ||--o{ RolePermission : binds

    %% Admissions Domain
    Lead ||--o{ TrialSession : schedules
    Lead ||--o{ ParentCareLog : tracks

    %% Academic Domain
    Branch ||--o{ SchoolClass : groups
    Branch ||--o{ Student : registers
    Room ||--o{ SchoolClass : hosts
    Teacher ||--o{ SchoolClass : teaches
    SchoolClass ||--o{ Student : contains
    SchoolClass ||--o{ ClassSchedule : plans
    SchoolClass ||--o{ ScheduleOccurrence : schedules
    ClassSchedule ||--o{ ScheduleOccurrence : generates
    
    Student ||--o{ StudentParent : links
    Parent ||--o{ StudentParent : links

    ScheduleOccurrence ||--o{ Attendance : marks
    Student ||--o{ Attendance : logs

    ScheduleOccurrence ||--o{ IndividualMakeup : hosts
    Student ||--o{ IndividualMakeup : books

    %% Financial Domain
    Student ||--o{ TuitionInvoice : pays
    SchoolClass ||--o{ TuitionInvoice : charges
    DiscountCode ||--o{ TuitionInvoice : applies
    PaymentSetting ||--o{ TuitionInvoice : references
    TuitionInvoice ||--o{ InvoiceItem : details
    TuitionInvoice ||--o{ PaymentTransaction : tracks

    %% Core Entities Fields Definition (Enterprise Standard)
    Branch {
        string Id PK
        string Name
        string Address
        string PhoneNumber
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
        boolean IsDeleted
    }
    User {
        string Id PK
        string Username
        string Email
        string PasswordHash
        boolean IsActive
    }
    Student {
        string Id PK
        string BranchId FK
        string FullName
        date DateOfBirth
        string Status
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
    }
    Parent {
        string Id PK
        string FullName
        string PhoneNumber
        string Email
    }
    SchoolClass {
        string Id PK
        string BranchId FK
        string ClassName
        string TeacherId FK
        string RoomId FK
        decimal MonthlyFee
        string Status
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
    }
    ScheduleOccurrence {
        string Id PK
        string ClassId FK
        datetime StartTime
        datetime EndTime
        string Status
    }
    TuitionInvoice {
        string Id PK
        string StudentId FK
        string ClassId FK
        decimal TotalAmount
        string PaymentStatus
        string QrCodeContent
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
    }
    InvoiceItem {
        string Id PK
        string TuitionInvoiceId FK
        string Name
        decimal Amount
        int Quantity
        decimal SubTotal
    }
    PaymentTransaction {
        string Id PK
        string TuitionInvoiceId FK
        decimal Amount
        string TransactionCode
        string PaymentMethod
        string PayeeName
        datetime TransactionTimeUtc
    }
```

---

## 🛠️ Tech Stack

### Backend
*   **Framework:** .NET Core (C#)
*   **Database Access:** Entity Framework Core
*   **Messaging & Command Handling:** MediatR (CQRS Pattern)
*   **Validation:** FluentValidation
*   **Realtime Notifications:** SignalR
*   **API Documentation:** Swagger / OpenAPI

### Frontend
*   **Framework:** Next.js (React)
*   **Language:** TypeScript
*   **Styles:** TailwindCSS
*   **Testing:** Vitest

---

## 🤖 Model Context Protocol (MCP) Server

The project includes an **MCP Server** (`EducationCenter.Crm.McpServer`) implemented in C# that runs over standard input/output (`stdio`). This allows AI assistants (like Claude Desktop or Antigravity IDE) to interact directly with the CRM database to query students, check schedules, and inspect attendance records.

### Exposing Tools
- `list_students`: Queries the student directory.
- `list_classes`: Fetches all classes.
- `list_teachers`: Fetches all teachers.
- `list_schedules`: Fetches class schedules within a specified date range.
- `get_attendance`: Queries occurrence-based attendance by ID.

### Running & Deploying
To compile and publish the MCP Server as a standalone executable:
```bash
dotnet publish src/EducationCenter.Crm.McpServer/EducationCenter.Crm.McpServer.csproj -c Release -r win-x64 --self-contained
```
This generates a compiled binary at `src/EducationCenter.Crm.McpServer/bin/Release/net10.0/win-x64/publish/EducationCenter.Crm.McpServer.exe` which can be registered in your AI client's configuration file (`mcp_config.json` or `claude_desktop_config.json`).

---

## 🚀 Getting Started

### Prerequisites
*   [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0 or higher)
*   [Node.js](https://nodejs.org/) (Version 18.0 or higher)
*   SQL Server or equivalent database engine.

### 1. Database Setup & Backend Initialization
1.  Navigate to the API project folder:
    ```bash
    cd src/EducationCenter.Crm.Api
    ```
2.  Update the database connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
3.  Apply migrations and update database schema:
    ```bash
    dotnet ef database update
    ```
4.  Run the backend server:
    ```bash
    dotnet run
    ```
    The Swagger documentation will be available at `http://localhost:5000/swagger`.

### 2. Frontend Initialization
1.  Navigate to the frontend folder:
    ```bash
    cd frontend
    ```
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Configure environmental variables. Copy `.env.example` to `.env.local` and configure your API URL:
    ```bash
    NEXT_PUBLIC_API_URL=http://localhost:5000/api
    ```
4.  Run the local development server:
    ```bash
    npm run dev
    ```
    Open `http://localhost:3000` to view the application.

---

## 🧪 Testing

### Backend Unit & Integration Tests
To run C# test suites:
```bash
dotnet test
```

### Frontend Tests
To run unit and component tests via Vitest:
```bash
cd frontend
npm run test
```

---

## 📄 License & Copyright

Copyright © 2026 Hungle2910. All rights reserved.

Licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE) file for details.
