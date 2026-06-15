# API Testing Reference and Request Payloads

This document provides the routes, parameters, and payload structures for all APIs in the AgriAndFarmMonitoring (AgriculturePlatform) codebase. Use this to construct your requests for testing.

## AdminAlert Endpoints

### `GET` /api/admin/farms/{farmId}/alerts

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `AlertType` (Query, Type: `string?`)
- `Severity` (Query, Type: `string?`)
- `IsResolved` (Query, Type: `bool?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/alerts/statistics

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/alerts/unresolved

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `AlertType` (Query, Type: `string?`)
- `Severity` (Query, Type: `string?`)
- `IsResolved` (Query, Type: `bool?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `PUT` /api/admin/farms/{farmId}/alerts/{id}/resolve

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `ResolveAlertDto`)

```json
{
  "AlertId": 0,
  "ResolutionNotes": "string"
}
```

---

## AdminHarvest Endpoints

### `GET` /api/admin/farms/{farmId}/harvests

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `WorkerId` (Query, Type: `int?`)
- `ApprovalStatus` (Query, Type: `string?`)
- `QualityGrade` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/harvests/crop-cycle/{cropCycleId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `cropCycleId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/harvests/field/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/harvests/pending-approvals

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1` (Query, Type: `=`)
- `20` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/harvests/statistics/comparison

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `currentYear` (Query, Type: `int`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/harvests/statistics/yield

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/harvests/worker/{workerId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `workerId` (Type: integer/string)

---

### `DELETE` /api/admin/farms/{farmId}/harvests/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/harvests/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/harvests/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateHarvestDto`)

```json
{
  "HarvestDate": "2026-06-15T09:48:12Z",
  "QuantityKg": 0.0,
  "QualityGrade": "string",
  "HarvestMethod": "string",
  "Notes": "string",
  "PricePerKg": 0.0,
  "BatchNumber": "string"
}
```

---

### `POST` /api/admin/farms/{farmId}/harvests/{id}/approve

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `HarvestApprovalDto`)

```json
{
  "HarvestId": 0,
  "ApprovalStatus": "string",
  "RejectionReason": "string",
  "AdminNotes": "string",
  "WorkerResponse": "string"
}
```

---

## AdminObservation Endpoints

### `GET` /api/admin/farms/{farmId}/observations

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `WorkerId` (Query, Type: `int?`)
- `CropHealth` (Query, Type: `string?`)
- `PestDetected` (Query, Type: `bool?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)
- `ValidationStatus` (Query, Type: `string?`)

---

### `GET` /api/admin/farms/{farmId}/observations/crop-cycle/{cropCycleId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `cropCycleId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/observations/date-range

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/observations/field/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/observations/pending-validation

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1` (Query, Type: `=`)
- `20` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/observations/questioned

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1` (Query, Type: `=`)
- `20` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/observations/statistics/pest

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/observations/statistics/validation-summary

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/observations/worker/{workerId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `workerId` (Type: integer/string)

---

### `DELETE` /api/admin/farms/{farmId}/observations/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/observations/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/observations/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateObservationDto`)

```json
{
  "ObservationDate": "2026-06-15T09:48:12Z",
  "CropHealth": "string",
  "PestDetected": true,
  "PestType": "string",
  "Notes": "string",
  "ImageUrls": [
    "value_of_string"
  ]
}
```

---

### `POST` /api/admin/farms/{farmId}/observations/{id}/validate

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `ObservationValidationDto`)

```json
{
  "ObservationId": 0,
  "ValidationStatus": "string",
  "AdminNotes": "string",
  "FlagReason": "string",
  "WorkerResponse": "string"
}
```

---

## AdminQualityCheck Endpoints

### `GET` /api/admin/farms/{farmId}/quality-checks

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `HarvestId` (Query, Type: `int?`)
- `WorkerId` (Query, Type: `int?`)
- `ApprovalStatus` (Query, Type: `string?`)
- `FinalGrade` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/quality-checks/harvest/{harvestId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `harvestId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/quality-checks/pending-approvals

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1` (Query, Type: `=`)
- `20` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/quality-checks/statistics/quality

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/quality-checks/worker/{workerId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `workerId` (Type: integer/string)

---

### `DELETE` /api/admin/farms/{farmId}/quality-checks/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/quality-checks/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/quality-checks/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateQualityCheckDto`)

```json
{
  "CheckDate": "2026-06-15T09:48:12Z",
  "MoisturePct": 0.0,
  "DefectPct": 0.0,
  "FinalGrade": "string",
  "Notes": "string"
}
```

---

### `POST` /api/admin/farms/{farmId}/quality-checks/{id}/approve

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `QualityCheckApprovalDto`)

```json
{
  "QualityCheckId": 0,
  "ApprovalStatus": "string",
  "RejectionReason": "string",
  "AdminNotes": "string",
  "WorkerResponse": "string"
}
```

---

## AdminSensor Endpoints

### `GET` /api/admin/farms/{farmId}/sensors

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `SensorType` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `LatestOnly` (Query, Type: `bool?`)
- `GroupBy` (Query, Type: `string?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/sensors/export

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/sensors/field/{fieldId}/history

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

**Query Parameters:**

- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/sensors/latest

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/sensors/statistics

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `"day"` (Query, Type: `=`)
- `null` (Query, Type: `=`)
- `null` (Query, Type: `=`)

---

### `GET` /api/admin/farms/{farmId}/sensors/threshold-violations

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `fromDate` (Query, Type: `DateTime?`)
- `toDate` (Query, Type: `DateTime?`)

---

## AdminTasks Endpoints

### `GET` /api/admin/farms/{farmId}/tasks

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `WorkerId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `Status` (Query, Type: `string?`)
- `Priority` (Query, Type: `string?`)
- `TaskName` (Query, Type: `string?`)
- `AssignedDateFrom` (Query, Type: `DateTime?`)
- `AssignedDateTo` (Query, Type: `DateTime?`)
- `DueDateFrom` (Query, Type: `DateTime?`)
- `DueDateTo` (Query, Type: `DateTime?`)
- `IsOverdue` (Query, Type: `bool?`)
- `ActiveOnly` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `POST` /api/admin/farms/{farmId}/tasks

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `CreateTaskDto`)

```json
{
  "WorkerId": 0,
  "FieldId": 0,
  "CropCycleId": 0,
  "TaskName": "string",
  "DueDate": "2026-06-15T09:48:12Z",
  "Priority": "string",
  "Notes": "string"
}
```

---

### `GET` /api/admin/farms/{farmId}/tasks/active

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `POST` /api/admin/farms/{farmId}/tasks/bulk-assign

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `BulkAssignTaskDto`)

```json
{
  "WorkerIds": [
    "value_of_int"
  ],
  "FieldId": 0,
  "CropCycleId": 0,
  "TaskName": "string",
  "DueDate": "2026-06-15T09:48:12Z",
  "Priority": "string",
  "Notes": "string",
  "TotalRequests": 0,
  "SuccessCount": 0,
  "FailedCount": 0,
  "Errors": [
    "value_of_BulkAssignError"
  ],
  "RowNumber": 0,
  "WorkerId": 0,
  "ErrorMessage": "string"
}
```

---

### `POST` /api/admin/farms/{farmId}/tasks/bulk-assign-excel

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1024` (Query, Type: `*`)

**Request Body:** None (Empty)

---

### `POST` /api/admin/farms/{farmId}/tasks/bulk-reassign

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `BulkReassignDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

### `POST` /api/admin/farms/{farmId}/tasks/bulk-reassign-excel

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1024` (Query, Type: `*`)

**Request Body:** None (Empty)

---

### `POST` /api/admin/farms/{farmId}/tasks/bulk-status

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `BulkStatusUpdateDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

### `POST` /api/admin/farms/{farmId}/tasks/bulk-status-excel

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1024` (Query, Type: `*`)

**Request Body:** None (Empty)

---

### `GET` /api/admin/farms/{farmId}/tasks/completion-history

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `fromDate` (Query, Type: `DateTime?`)
- `toDate` (Query, Type: `DateTime?`)

---

### `GET` /api/admin/farms/{farmId}/tasks/field/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/overdue

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/statistics

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/templates/bulk-assign

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/templates/reassign

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/templates/status-update

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/worker/{workerId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `workerId` (Type: integer/string)

---

### `DELETE` /api/admin/farms/{farmId}/tasks/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/tasks/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/tasks/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateTaskDto`)

```json
{
  "WorkerId": 0,
  "FieldId": 0,
  "CropCycleId": 0,
  "TaskName": "string",
  "DueDate": "2026-06-15T09:48:12Z",
  "Status": "string",
  "Priority": "string",
  "Notes": "string"
}
```

---

### `PUT` /api/admin/farms/{farmId}/tasks/{id}/reassign

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `ReassignTaskDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

### `PUT` /api/admin/farms/{farmId}/tasks/{id}/status

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateTaskStatusDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

## AdminTest Endpoints

### `POST` /api/admin/farms/{farmId}/test/generate-critical-alerts

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body:** None (Empty)

---

### `POST` /api/admin/farms/{farmId}/test/generate-random-severity

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body:** None (Empty)

---

### `POST` /api/admin/farms/{farmId}/test/send-test-email

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `email` (Query, Type: `string`)

**Request Body:** None (Empty)

---

## AdminWeather Endpoints

### `GET` /api/admin/farms/{farmId}/weather/alerts

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/weather/current/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/weather/debug-fields

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/weather/forecast/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/weather/history

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)

---

### `POST` /api/admin/farms/{farmId}/weather/manual

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `ManualWeatherEntryDto`)

```json
{
  "FieldId": 0,
  "Temperature": 0.0,
  "Humidity": 0.0,
  "RainfallMm": 0.0,
  "WindSpeed": 0.0,
  "Condition": "string",
  "RecordedAt": "2026-06-15T09:48:12Z",
  "Notes": "string"
}
```

---

### `POST` /api/admin/farms/{farmId}/weather/refresh-all

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body:** None (Empty)

---

### `POST` /api/admin/farms/{farmId}/weather/refresh/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

**Request Body:** None (Empty)

---

### `GET` /api/admin/farms/{farmId}/weather/settings

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/weather/settings

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `WeatherApiSettingsDto`)

```json
{
  "ApiProvider": "string",
  "ApiKey": "string",
  "BaseUrl": "string",
  "UpdateIntervalMinutes": 0,
  "AutoUpdateEnabled": true
}
```

---

### `DELETE` /api/admin/farms/{farmId}/weather/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/weather/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `ManualWeatherEntryDto`)

```json
{
  "FieldId": 0,
  "Temperature": 0.0,
  "Humidity": 0.0,
  "RainfallMm": 0.0,
  "WindSpeed": 0.0,
  "Condition": "string",
  "RecordedAt": "2026-06-15T09:48:12Z",
  "Notes": "string"
}
```

---

## AdminWorkerField Endpoints

### `GET` /api/admin/farms/{farmId}/worker-fields

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `WorkerId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `IsActive` (Query, Type: `bool?`)
- `AssignedDateFrom` (Query, Type: `DateTime?`)
- `AssignedDateTo` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `POST` /api/admin/farms/{farmId}/worker-fields

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `AssignFieldToWorkerDto`)

```json
{
  "WorkerId": 0,
  "FieldId": 0,
  "AssignedDate": "2026-06-15T09:48:12Z",
  "EndDate": "2026-06-15T09:48:12Z",
  "Notes": "string"
}
```

---

### `GET` /api/admin/farms/{farmId}/worker-fields/active

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `WorkerId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `IsActive` (Query, Type: `bool?`)
- `AssignedDateFrom` (Query, Type: `DateTime?`)
- `AssignedDateTo` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/worker-fields/field/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

**Query Parameters:**

- `WorkerId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `IsActive` (Query, Type: `bool?`)
- `AssignedDateFrom` (Query, Type: `DateTime?`)
- `AssignedDateTo` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/worker-fields/worker/{workerId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `workerId` (Type: integer/string)

**Query Parameters:**

- `WorkerId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `IsActive` (Query, Type: `bool?`)
- `AssignedDateFrom` (Query, Type: `DateTime?`)
- `AssignedDateTo` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `DELETE` /api/admin/farms/{farmId}/worker-fields/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/worker-fields/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/worker-fields/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `AssignFieldToWorkerDto`)

```json
{
  "WorkerId": 0,
  "FieldId": 0,
  "AssignedDate": "2026-06-15T09:48:12Z",
  "EndDate": "2026-06-15T09:48:12Z",
  "Notes": "string"
}
```

---

## AdminYieldReport Endpoints

### `GET` /api/admin/farms/{farmId}/yield-reports

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `CropCycleId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `ReportType` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IsScheduled` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/admin/farms/{farmId}/yield-reports/by-crop-type/{cropType}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `cropType` (Type: integer/string)

**Query Parameters:**

- `year` (Query, Type: `int?`)

---

### `GET` /api/admin/farms/{farmId}/yield-reports/by-season/{season}/{year}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `season` (Type: integer/string)
- `year` (Type: integer/string)

---

### `POST` /api/admin/farms/{farmId}/yield-reports/generate

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `GenerateYieldReportDto`)

```json
{
  "CropCycleId": 0,
  "FieldId": 0,
  "StartDate": "2026-06-15T09:48:12Z",
  "EndDate": "2026-06-15T09:48:12Z",
  "ReportName": "string",
  "ExportFormat": "string",
  "FieldComparisons": [
    "value_of_FieldComparisonDto"
  ],
  "SeasonalComparisons": [
    "value_of_SeasonalComparisonDto"
  ],
  "Summary": "<ComparisonSummaryDto>",
  "FieldName": "string",
  "CurrentYield": 0.0,
  "PreviousYield": 0.0,
  "ChangePercentage": 0.0,
  "Trend": "string",
  "Season": "string",
  "Year": 0,
  "TotalYield": 0.0,
  "AveragePrice": 0.0,
  "TotalValue": 0.0,
  "OverallChangePercentage": 0.0,
  "BestPerformingField": "string",
  "BestYield": 0.0,
  "WorstPerformingField": "string",
  "WorstYield": 0.0,
  "Recommendation": "string"
}
```

---

### `POST` /api/admin/farms/{farmId}/yield-reports/schedule

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `CreateYieldReportDto`)

```json
{
  "ReportName": "string",
  "ReportType": "string",
  "CropCycleId": 0,
  "FieldId": 0,
  "StartDate": "2026-06-15T09:48:12Z",
  "EndDate": "2026-06-15T09:48:12Z",
  "IsScheduled": true,
  "ScheduleCron": "string"
}
```

---

### `GET` /api/admin/farms/{farmId}/yield-reports/statistics/comparison

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `fieldId` (Query, Type: `int?`)
- `currentYear` (Query, Type: `int`)
- `previousYear` (Query, Type: `int?`)

---

### `GET` /api/admin/farms/{farmId}/yield-reports/statistics/summary

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `fromDate` (Query, Type: `DateTime?`)
- `toDate` (Query, Type: `DateTime?`)

---

### `DELETE` /api/admin/farms/{farmId}/yield-reports/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/admin/farms/{farmId}/yield-reports/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/admin/farms/{farmId}/yield-reports/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateYieldReportDto`)

```json
{
  "ReportName": "string",
  "IsScheduled": true,
  "ScheduleCron": "string"
}
```

---

### `GET` /api/admin/farms/{farmId}/yield-reports/{id}/export

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Query Parameters:**

- `"CSV"` (Query, Type: `=`)

---

## Auth Endpoints

### `POST` /api/auth/login

**Request Body (`application/json`):** (Dto: `LoginDto`)

```json
{
  "Email": "string",
  "Password": "string"
}
```

---

### `POST` /api/auth/logout

**Request Body (`application/json`):** (Dto: `RevokeTokenDto`)

```json
{
  "RefreshToken": "string"
}
```

---

### `POST` /api/auth/refresh-token

**Request Body (`application/json`):** (Dto: `RefreshTokenDto`)

```json
{
  "AccessToken": "string",
  "RefreshToken": "string"
}
```

---

### `POST` /api/auth/register

**Request Body (`application/json`):** (Dto: `RegisterDto`)

```json
{
  "FarmName": "string",
  "FarmEmail": "string",
  "FarmPhone": "string",
  "FarmAddress": "string",
  "FarmCity": "string",
  "FarmState": "string",
  "FarmCountry": "string",
  "FarmPostalCode": "string",
  "TotalLandHectares": 0.0,
  "AdminName": "string",
  "AdminEmail": "string",
  "AdminPassword": "string",
  "AdminPhone": "string"
}
```

---

### `POST` /api/auth/revoke-token

**Request Body (`application/json`):** (Dto: `RevokeTokenDto`)

```json
{
  "RefreshToken": "string"
}
```

---

### `GET` /api/auth/validate

---

## CropCycles Endpoints

### `GET` /api/farms/{farmId}/crop-cycles

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropType` (Query, Type: `string?`)
- `GrowthStage` (Query, Type: `string?`)
- `Status` (Query, Type: `string?`)
- `ExpectedHarvestDateFrom` (Query, Type: `DateTime?`)
- `ExpectedHarvestDateTo` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `ActiveOnly` (Query, Type: `bool?`)
- `OverdueOnly` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `POST` /api/farms/{farmId}/crop-cycles

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `CreateCropCycleDto`)

```json
{
  "FieldId": 0,
  "CropType": "string",
  "PlantingDate": "2026-06-15T09:48:12Z",
  "ExpectedHarvestDate": "2026-06-15T09:48:12Z",
  "GrowthStage": "string",
  "Status": "string"
}
```

---

### `GET` /api/farms/{farmId}/crop-cycles/overdue

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `DELETE` /api/farms/{farmId}/crop-cycles/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/farms/{farmId}/crop-cycles/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/farms/{farmId}/crop-cycles/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateCropCycleDto`)

```json
{
  "CropType": "string",
  "PlantingDate": "2026-06-15T09:48:12Z",
  "ExpectedHarvestDate": "2026-06-15T09:48:12Z",
  "GrowthStage": "string",
  "Status": "string"
}
```

---

## Download Endpoints

### `GET` /api/downloads/reports/{fileName}

**Route Parameters:**

- `fileName` (Type: integer/string)

---

## Fields Endpoints

### `GET` /api/farms/{farmId}/fields

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldName` (Query, Type: `string?`)
- `Location` (Query, Type: `string?`)
- `SoilType` (Query, Type: `string?`)
- `Status` (Query, Type: `string?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `POST` /api/farms/{farmId}/fields

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `CreateFieldDto`)

```json
{
  "FieldName": "string",
  "Location": "string",
  "AreaHectares": 0.0,
  "SoilType": "string",
  "Status": "string",
  "Latitude": 0.0,
  "Longitude": 0.0
}
```

---

### `POST` /api/farms/{farmId}/fields/bulk-import

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `1024` (Query, Type: `*`)

**Request Body:** None (Empty)

---

### `POST` /api/farms/{farmId}/fields/bulk-soft-delete

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `List<int>`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

### `GET` /api/farms/{farmId}/fields/export

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/farms/{farmId}/fields/statistics

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/farms/{farmId}/fields/template

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `DELETE` /api/farms/{farmId}/fields/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/farms/{farmId}/fields/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/farms/{farmId}/fields/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateFieldDto`)

```json
{
  "FieldName": "string",
  "Location": "string",
  "AreaHectares": 0.0,
  "SoilType": "string",
  "Status": "string",
  "Latitude": 0.0,
  "Longitude": 0.0
}
```

---

### `PUT` /api/farms/{farmId}/fields/{id}/location

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateLocationDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

## WorkerAuth Endpoints

### `POST` /api/worker/auth/login

**Request Body (`application/json`):** (Dto: `WorkerLoginDto`)

```json
{
  "Email": "string",
  "Password": "string"
}
```

---

### `POST` /api/worker/auth/logout

**Request Body (`application/json`):** (Dto: `RevokeTokenDto`)

```json
{
  "RefreshToken": "string"
}
```

---

### `POST` /api/worker/auth/refresh-token

**Request Body (`application/json`):** (Dto: `RefreshTokenDto`)

```json
{
  "AccessToken": "string",
  "RefreshToken": "string"
}
```

---

### `POST` /api/worker/auth/revoke-token

**Request Body (`application/json`):** (Dto: `RevokeTokenDto`)

```json
{
  "RefreshToken": "string"
}
```

---

## WorkerFields Endpoints

### `GET` /api/worker/fields

---

### `GET` /api/worker/fields/{fieldId}

**Route Parameters:**

- `fieldId` (Type: integer/string)

---

## WorkerHarvest Endpoints

### `POST` /api/worker/harvests

**Request Body (`application/json`):** (Dto: `CreateHarvestDto`)

```json
{
  "FieldId": 0,
  "CropCycleId": 0,
  "HarvestDate": "2026-06-15T09:48:12Z",
  "QuantityKg": 0.0,
  "QualityGrade": "string",
  "HarvestMethod": "string",
  "Notes": "string",
  "PricePerKg": 0.0,
  "BatchNumber": "string"
}
```

---

### `GET` /api/worker/harvests/my

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `WorkerId` (Query, Type: `int?`)
- `ApprovalStatus` (Query, Type: `string?`)
- `QualityGrade` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/worker/harvests/pending-count

---

### `DELETE` /api/worker/harvests/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `GET` /api/worker/harvests/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `PUT` /api/worker/harvests/{id}

**Route Parameters:**

- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateHarvestDto`)

```json
{
  "HarvestDate": "2026-06-15T09:48:12Z",
  "QuantityKg": 0.0,
  "QualityGrade": "string",
  "HarvestMethod": "string",
  "Notes": "string",
  "PricePerKg": 0.0,
  "BatchNumber": "string"
}
```

---

### `POST` /api/worker/harvests/{id}/respond

**Route Parameters:**

- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `HarvestWorkerResponseDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

## WorkerObservation Endpoints

### `POST` /api/worker/observations

**Request Body (`application/json`):** (Dto: `CreateObservationDto`)

```json
{
  "FieldId": 0,
  "CropCycleId": 0,
  "ObservationDate": "2026-06-15T09:48:12Z",
  "CropHealth": "string",
  "PestDetected": true,
  "PestType": "string",
  "Notes": "string",
  "ImageUrls": [
    "value_of_string"
  ]
}
```

---

### `GET` /api/worker/observations/my

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `CropCycleId` (Query, Type: `int?`)
- `WorkerId` (Query, Type: `int?`)
- `CropHealth` (Query, Type: `string?`)
- `PestDetected` (Query, Type: `bool?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)
- `ValidationStatus` (Query, Type: `string?`)

---

### `DELETE` /api/worker/observations/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `GET` /api/worker/observations/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `PUT` /api/worker/observations/{id}

**Route Parameters:**

- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateObservationDto`)

```json
{
  "ObservationDate": "2026-06-15T09:48:12Z",
  "CropHealth": "string",
  "PestDetected": true,
  "PestType": "string",
  "Notes": "string",
  "ImageUrls": [
    "value_of_string"
  ]
}
```

---

## WorkerProfile Endpoints

### `GET` /api/worker/profile

---

### `PUT` /api/worker/profile

**Request Body (`application/json`):** (Dto: `UpdateWorkerProfileDto`)

```json
{
  "Name": "string",
  "Phone": "string",
  "CurrentPassword": "string",
  "NewPassword": "string"
}
```

---

### `PUT` /api/worker/profile/change-password

**Request Body (`application/json`):** (Dto: `ChangeWorkerPasswordDto`)

```json
{
  "CurrentPassword": "string",
  "NewPassword": "string"
}
```

---

## WorkerQualityCheck Endpoints

### `POST` /api/worker/quality-checks

**Request Body (`application/json`):** (Dto: `CreateQualityCheckDto`)

```json
{
  "HarvestId": 0,
  "CheckDate": "2026-06-15T09:48:12Z",
  "MoisturePct": 0.0,
  "DefectPct": 0.0,
  "FinalGrade": "string",
  "Notes": "string"
}
```

---

### `GET` /api/worker/quality-checks/my

**Query Parameters:**

- `HarvestId` (Query, Type: `int?`)
- `WorkerId` (Query, Type: `int?`)
- `ApprovalStatus` (Query, Type: `string?`)
- `FinalGrade` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IncludeDeleted` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/worker/quality-checks/pending-count

---

### `DELETE` /api/worker/quality-checks/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `GET` /api/worker/quality-checks/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `PUT` /api/worker/quality-checks/{id}

**Route Parameters:**

- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateQualityCheckDto`)

```json
{
  "CheckDate": "2026-06-15T09:48:12Z",
  "MoisturePct": 0.0,
  "DefectPct": 0.0,
  "FinalGrade": "string",
  "Notes": "string"
}
```

---

### `POST` /api/worker/quality-checks/{id}/respond

**Route Parameters:**

- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `QualityCheckWorkerResponseDto`)

```json
{
  // Properties not parsed or empty DTO
}
```

---

## WorkerSensor Endpoints

### `GET` /api/worker/farms/{farmId}/sensors/alerts/unresolved

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/worker/farms/{farmId}/sensors/field/{fieldId}/latest

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/worker/farms/{farmId}/sensors/latest

**Route Parameters:**

- `farmId` (Type: integer/string)

---

## WorkerTasks Endpoints

### `GET` /api/worker/tasks

**Query Parameters:**

- `Status` (Query, Type: `string?`)
- `Priority` (Query, Type: `string?`)
- `TaskName` (Query, Type: `string?`)
- `DueDateFrom` (Query, Type: `DateTime?`)
- `DueDateTo` (Query, Type: `DateTime?`)
- `IsOverdue` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/worker/tasks/history

**Query Parameters:**

- `Status` (Query, Type: `string?`)
- `Priority` (Query, Type: `string?`)
- `TaskName` (Query, Type: `string?`)
- `DueDateFrom` (Query, Type: `DateTime?`)
- `DueDateTo` (Query, Type: `DateTime?`)
- `IsOverdue` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/worker/tasks/statistics

---

### `GET` /api/worker/tasks/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

### `PUT` /api/worker/tasks/{id}/status

**Route Parameters:**

- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateWorkerTaskStatusDto`)

```json
{
  "Status": "string",
  "CompletionNotes": "string"
}
```

---

## WorkerWeather Endpoints

### `GET` /api/worker/farms/{farmId}/weather/alerts

**Route Parameters:**

- `farmId` (Type: integer/string)

---

### `GET` /api/worker/farms/{farmId}/weather/current/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/worker/farms/{farmId}/weather/forecast/{fieldId}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `fieldId` (Type: integer/string)

---

### `GET` /api/worker/farms/{farmId}/weather/history

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `FieldId` (Query, Type: `int?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)

---

## WorkerYieldReport Endpoints

### `GET` /api/worker/yield-reports

**Query Parameters:**

- `CropCycleId` (Query, Type: `int?`)
- `FieldId` (Query, Type: `int?`)
- `ReportType` (Query, Type: `string?`)
- `FromDate` (Query, Type: `DateTime?`)
- `ToDate` (Query, Type: `DateTime?`)
- `IsScheduled` (Query, Type: `bool?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)

---

### `GET` /api/worker/yield-reports/{id}

**Route Parameters:**

- `id` (Type: integer/string)

---

## Workers Endpoints

### `GET` /api/farms/{farmId}/workers

**Route Parameters:**

- `farmId` (Type: integer/string)

**Query Parameters:**

- `Name` (Query, Type: `string?`)
- `Email` (Query, Type: `string?`)
- `Role` (Query, Type: `string?`)
- `IsActive` (Query, Type: `bool?`)
- `HireDateFrom` (Query, Type: `DateTime?`)
- `HireDateTo` (Query, Type: `DateTime?`)
- `Page` (Query, Type: `int?`)
- `PageSize` (Query, Type: `int?`)
- `SortBy` (Query, Type: `string?`)
- `IsDescending` (Query, Type: `bool`)
- `IncludeDeleted` (Query, Type: `bool?`)

---

### `POST` /api/farms/{farmId}/workers

**Route Parameters:**

- `farmId` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `CreateWorkerDto`)

```json
{
  "Name": "string",
  "Email": "string",
  "Phone": "string",
  "Role": "string",
  "Password": "string",
  "HireDate": "2026-06-15T09:48:12Z"
}
```

---

### `DELETE` /api/farms/{farmId}/workers/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `GET` /api/farms/{farmId}/workers/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/farms/{farmId}/workers/{id}

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `UpdateWorkerDto`)

```json
{
  "Name": "string",
  "Email": "string",
  "Phone": "string",
  "Role": "string",
  "IsActive": true
}
```

---

### `PUT` /api/farms/{farmId}/workers/{id}/activate

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body:** None (Empty)

---

### `PUT` /api/farms/{farmId}/workers/{id}/deactivate

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body:** None (Empty)

---

### `GET` /api/farms/{farmId}/workers/{id}/login-history

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

---

### `PUT` /api/farms/{farmId}/workers/{id}/reset-password

**Route Parameters:**

- `farmId` (Type: integer/string)
- `id` (Type: integer/string)

**Request Body (`application/json`):** (Dto: `ChangeWorkerPasswordDto`)

```json
{
  "CurrentPassword": "string",
  "NewPassword": "string"
}
```

---
