-- Create tables based on your existing Azure SQL schema
CREATE TABLE IF NOT EXISTS Buildings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Latitude DOUBLE NOT NULL,
    Longitude DOUBLE NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME NULL
);

CREATE TABLE IF NOT EXISTS BuildingConnections (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FromBuildingId INT NOT NULL,
    ToBuildingId INT NOT NULL,
    Distance INT NOT NULL,
    TrafficFactor DOUBLE DEFAULT 1.0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME NULL,
    FOREIGN KEY (FromBuildingId) REFERENCES Buildings(Id),
    FOREIGN KEY (ToBuildingId) REFERENCES Buildings(Id)
);

CREATE TABLE IF NOT EXISTS UserLocations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(100) NOT NULL,
    CurrentNode VARCHAR(200) NULL,
    CurrentEdge VARCHAR(400) NULL,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_timestamp (Timestamp)
);

CREATE TABLE IF NOT EXISTS UserPresences (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(255) NOT NULL,
    CurrentBuildingId INT NULL, -- Changed from NOT NULL to NULL
    LastSeen DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CurrentBuildingId) REFERENCES Buildings(Id),
    UNIQUE INDEX idx_userpresences_userid (UserId) -- Assuming one presence record per user
);

CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(256) NOT NULL UNIQUE,
    Password VARCHAR(100) NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME NULL
);