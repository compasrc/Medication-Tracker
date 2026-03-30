-- Medication Tracker Database Schema
-- Created for Loyola COMP 425 - Medication Management System
-- Database: Manage_Medications

-- =============================================
-- Users Table
-- =============================================
CREATE TABLE [Users] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [PhoneNumber] NVARCHAR(20),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL
);

-- =============================================
-- Medications Table
-- =============================================
CREATE TABLE [Medications] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [Dosage] NVARCHAR(100) NOT NULL,
    [Frequency] NVARCHAR(100) NOT NULL,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL
);

-- =============================================
-- MedicationSchedules Table
-- =============================================
CREATE TABLE [MedicationSchedules] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [MedicationId] INT NOT NULL,
    [ScheduleTime] NVARCHAR(50) NOT NULL,
    [Notes] NVARCHAR(500),
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NULL,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME NULL,
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([MedicationId]) REFERENCES [Medications]([Id]) ON DELETE CASCADE
);

-- =============================================
-- MedicationLogs Table
-- =============================================
CREATE TABLE [MedicationLogs] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [MedicationId] INT NOT NULL,
    [MedicationScheduleId] INT NOT NULL,
    [TakenAt] DATETIME NOT NULL,
    [WasTaken] BIT DEFAULT 1,
    [Notes] NVARCHAR(500),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([MedicationId]) REFERENCES [Medications]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([MedicationScheduleId]) REFERENCES [MedicationSchedules]([Id]) ON DELETE CASCADE
);

-- =============================================
-- Indexes for Performance
-- =============================================
CREATE INDEX [IX_MedicationSchedules_UserId] ON [MedicationSchedules]([UserId]);
CREATE INDEX [IX_MedicationSchedules_MedicationId] ON [MedicationSchedules]([MedicationId]);
CREATE INDEX [IX_MedicationLogs_UserId] ON [MedicationLogs]([UserId]);
CREATE INDEX [IX_MedicationLogs_MedicationId] ON [MedicationLogs]([MedicationId]);
CREATE INDEX [IX_MedicationLogs_MedicationScheduleId] ON [MedicationLogs]([MedicationScheduleId]);
