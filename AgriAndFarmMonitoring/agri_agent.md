# AgriAndFarmMonitoring Agent Context

## 1. Project Overview
**AgriAndFarmMonitoring** is a comprehensive farm management system with two main panels:
- **Admin Panel**: For farm owners to manage fields, crops, workers, tasks, sensors, weather alerts, harvests, yield reports, and quality checks.
- **Worker Panel**: For farm workers to view their assigned fields, manage their assigned tasks, and submit field observations.

## 2. Technology Stack
### Backend (AgriculturePlatform)
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Database**: PostgreSQL (with JSONB support for unstructured data like image paths)
- **ORM**: Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication**: JWT Bearer Authentication (Admin & Worker distinct roles, managed via custom claims and `[AuthorizeAdmin]` / `[AuthorizeWorkerFarm]` filters).
- **Validation**: FluentValidation (intercepted globally or per DTO)
- **Mapping**: AutoMapper
- **Logging**: Serilog, with custom Audit Logging table (`AuditLogs`).

### Frontend (UI)
- **Framework**: Angular 17/18 (Standalone Components)
- **UI Library**: Angular Material (`@angular/material`)
- **Styling**: SCSS (Custom theme, `.container` based layouts, grid layouts)
- **State/API Management**: RxJS, HttpClient with token interceptors.
- **Routing**: Lazy-loaded routes for Admin (`admin.routes.ts`) and Worker (`worker.routes.ts`) panels.

## 3. Core Entities & Features

### Crop & Field Management
- **Field**: Physical land area for farming. Includes boundaries, locations, and soil types.
- **CropCycle**: The lifecycle of a crop planted in a field (from planting to harvest).
- **Sensor**: Devices attached to fields (Soil Moisture, Temperature, Humidity).
- **Observation**: Crop health observations made by workers. 
  - **Recent Change**: Added validation workflows (Pending, Verified, Questioned, Invalid) where admins review worker observations. Supports multiple image uploads (stored as string paths for future Azure Blob migration).

### Task & Resource Management
- **Worker**: Farm employees. Assigned to fields via `WorkerFieldAssignment`.
- **WorkerTask (Task.cs)**: Specific jobs (e.g., Irrigation, Fertilizing, Pest Control) assigned to workers.
  - **Enums**: `TaskTypeEnum`, `TaskPriorityEnum`, `TaskStatusEnum` (PENDING, IN_PROGRESS, COMPLETED, CANCELLED, OVERDUE).

### Harvest & Yield
- **Harvest**: Records of crop harvesting.
- **YieldReport**: Comprehensive reports combining harvest data with environmental/sensor data.
- **QualityCheck**: Quality assurance records for harvested crops.

### Weather
- **WeatherAlert**: System or external weather alerts for a farm (Storms, Frost, Drought).

## 4. Key Developer Guidelines

### Backend Rules
- **Migrations**: Always run `dotnet ef migrations add <Name> --project AgriculturePlatform.Infrastructure --startup-project AgriculturePlatform.API` when modifying `AgriculturePlatform.Domain.Entities`.
- **DTOs**: Always map entities to DTOs in Application layer. Never expose Entities directly.
- **Audit Logging**: Use `IAuditLogService` in Service methods when performing CUD (Create/Update/Delete) operations.
- **Authorization**: Ensure controllers have `[AuthorizeAdmin]` or `[AuthorizeWorkerFarm]` to prevent cross-tenant/cross-role data access. Ensure queries filter by `FarmId`.

### Frontend Rules
- **Standalone Components**: Always use `standalone: true`. Import necessary Material modules directly in the component's `imports: []` array.
- **Material Components**: Rely on `mat-card`, `mat-table`, `mat-form-field`, `mat-select`, and `mat-chip-set` for UI consistency.
- **Services**: Inject `HttpClient` and use `environment.apiUrl`. 
- **Forms**: Use Reactive Forms (`FormGroup`, `FormBuilder`) for all data entry.

## 5. Recent Architectural Decisions
- **Observations Image Handling**: Images are currently stored as URL strings/paths in PostgreSQL (`jsonb` for additional images) to prepare for Azure Blob Storage integration.
- **Task Status Flexibility**: Workers can freely transition tasks between `PENDING`, `IN_PROGRESS`, and `COMPLETED` without forced sequential progression, allowing flexible field operations.

*Note: Update this file as new major features or architectural patterns are introduced.*
