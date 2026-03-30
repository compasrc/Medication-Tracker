# Medication Tracker Database Schema

**Course:** COMP 425  
**Database Name:** Manage_Medications  
**Server:** 147.126.2.58  
**Last Updated:** 2025

---

## Database Overview

The Medication Tracker database is designed to manage users, medications, medication schedules, and medication logs. It enables tracking of medication adherence and scheduling for healthcare management.

---

## Entity Relationship Diagram (ERD)

```
                              ┌─────────────────────┐
                              │      USERS          │
                              ├─────────────────────┤
                              │ PK: Id (INT)        │
                              │    FirstName        │
                              │    LastName         │
                              │    Email (UNIQUE)   │
                              │    PhoneNumber      │
                              │    CreatedAt        │
                              │    UpdatedAt        │
                              └─────────────────────┘
                                    │         │
                    ┌───────────────┘         └──────────────┐
                    │ 1:M                                1:M │
                    ↓                                         ↓
        ┌──────────────────────────────┐    ┌────────────────────────┐
        │   MEDICATIONSCHEDULES        │    │   MEDICATIONLOGS       │
        ├──────────────────────────────┤    ├────────────────────────┤
        │ PK: Id (INT)                 │    │ PK: Id (INT)           │
        │ FK: UserId (INT)             │    │ FK: UserId (INT)       │
        │ FK: MedicationId (INT)       │    │ FK: MedicationId (INT) │
        │    ScheduleTime (NVARCHAR)   │    │ FK: MedicationScheduleId
        │    Notes (NVARCHAR)          │    │    TakenAt (DATETIME)  │
        │    StartDate (DATETIME)      │    │    WasTaken (BIT)      │
        │    EndDate (DATETIME)        │    │    Notes (NVARCHAR)    │
        │    IsActive (BIT)            │    │    CreatedAt (DATETIME)│
        │    CreatedAt (DATETIME)      │    └────────────────────────┘
        │    UpdatedAt (DATETIME)      │
        └──────────────────────────────┘
                    │
                    │ M:1
                    └──────────────┐
                                   ↓
                        ┌─────────────────────┐
                        │   MEDICATIONS       │
                        ├─────────────────────┤
                        │ PK: Id (INT)        │
                        │    Name             │
                        │    Description      │
                        │    Dosage           │
                        │    Frequency        │
                        │    CreatedAt        │
                        │    UpdatedAt        │
                        └─────────────────────┘
```

---

## Table Schema Details

### 1. USERS Table

Stores user information for the medication tracking system.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| **Id** | INT | PRIMARY KEY, IDENTITY(1,1) | Unique user identifier |
| FirstName | NVARCHAR(100) | NOT NULL | User's first name |
| LastName | NVARCHAR(100) | NOT NULL | User's last name |
| Email | NVARCHAR(255) | NOT NULL, UNIQUE | User's email address (unique) |
| PhoneNumber | NVARCHAR(20) | NULL | User's phone number |
| CreatedAt | DATETIME | DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME | NULL | Record last update timestamp |

**Relationships:** 1:M with MedicationSchedules, 1:M with MedicationLogs  
**Delete Behavior:** CASCADE (Deleting a user cascades to their schedules and logs)

---

### 2. MEDICATIONS Table

Stores medication information.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| **Id** | INT | PRIMARY KEY, IDENTITY(1,1) | Unique medication identifier |
| Name | NVARCHAR(200) | NOT NULL | Medication name |
| Description | NVARCHAR(500) | NULL | Medication description |
| Dosage | NVARCHAR(100) | NOT NULL | Dosage amount (e.g., "500mg") |
| Frequency | NVARCHAR(100) | NOT NULL | Frequency (e.g., "twice daily") |
| CreatedAt | DATETIME | DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME | NULL | Record last update timestamp |

**Relationships:** 1:M with MedicationSchedules, 1:M with MedicationLogs  
**Delete Behavior:** CASCADE

---

### 3. MEDICATIONSCHEDULES Table

Stores medication schedule information for users.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| **Id** | INT | PRIMARY KEY, IDENTITY(1,1) | Unique schedule identifier |
| **UserId** | INT | FOREIGN KEY, NOT NULL | Reference to Users table |
| **MedicationId** | INT | FOREIGN KEY, NOT NULL | Reference to Medications table |
| ScheduleTime | NVARCHAR(50) | NOT NULL | Scheduled time (e.g., "08:00 AM") |
| Notes | NVARCHAR(500) | NULL | Additional notes about schedule |
| StartDate | DATETIME | NOT NULL | When the schedule begins |
| EndDate | DATETIME | NULL | When the schedule ends (if applicable) |
| IsActive | BIT | DEFAULT 1 | Active/inactive flag |
| CreatedAt | DATETIME | DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME | NULL | Record last update timestamp |

**Relationships:** M:1 with Users, M:1 with Medications, 1:M with MedicationLogs  
**Delete Behavior:** CASCADE  
**Indexes:** UserId, MedicationId

---

### 4. MEDICATIONLOGS Table

Stores records of when medications were actually taken.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| **Id** | INT | PRIMARY KEY, IDENTITY(1,1) | Unique log entry identifier |
| **UserId** | INT | FOREIGN KEY, NOT NULL | Reference to Users table |
| **MedicationId** | INT | FOREIGN KEY, NOT NULL | Reference to Medications table |
| **MedicationScheduleId** | INT | FOREIGN KEY, NOT NULL | Reference to MedicationSchedules table |
| TakenAt | DATETIME | NOT NULL | When the medication was taken |
| WasTaken | BIT | DEFAULT 1 | Whether medication was taken |
| Notes | NVARCHAR(500) | NULL | Additional notes about dosage |
| CreatedAt | DATETIME | DEFAULT GETUTCDATE() | Record creation timestamp |

**Relationships:** M:1 with Users, M:1 with Medications, M:1 with MedicationSchedules  
**Delete Behavior:** CASCADE  
**Indexes:** UserId, MedicationId, MedicationScheduleId

---

## Key Features

✓ **Data Integrity:** Foreign key relationships with CASCADE delete behavior  
✓ **Unique Constraints:** Email uniqueness ensures no duplicate user accounts  
✓ **Audit Trail:** Timestamp fields (CreatedAt, UpdatedAt) track record changes  
✓ **Flexibility:** Optional fields (EndDate, Notes) provide scheduling flexibility  
✓ **Performance:** Indexes on frequently queried foreign keys  
✓ **Scalability:** Identity columns for automatic ID generation  

---

## Example Queries

### Track medication adherence for a user
```sql
SELECT 
    u.FirstName, u.LastName,
    m.Name AS MedicationName,
    ms.ScheduleTime,
    ml.TakenAt,
    ml.WasTaken,
    ml.Notes
FROM MedicationLogs ml
JOIN Users u ON ml.UserId = u.Id
JOIN Medications m ON ml.MedicationId = m.Id
JOIN MedicationSchedules ms ON ml.MedicationScheduleId = ms.Id
WHERE u.Id = 1
ORDER BY ml.TakenAt DESC;
```

### Get active medication schedules for a user
```sql
SELECT 
    m.Name AS MedicationName,
    ms.ScheduleTime,
    ms.StartDate,
    ms.EndDate
FROM MedicationSchedules ms
JOIN Medications m ON ms.MedicationId = m.Id
WHERE ms.UserId = 1 AND ms.IsActive = 1
ORDER BY ms.ScheduleTime;
```

---

## Notes

- All timestamps use UTC for consistency
- Deleting a user removes all their schedules and logs
- The Email field is case-insensitive in SQL Server by default
- MedicationSchedules can track both one-time and recurring medications
- MedicationLogs provide complete audit trail of medication adherence
