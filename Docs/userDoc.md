# EasySave User Documentation

## Table of Contents
1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Core Features](#core-features)
4. [User Interface Guide](#user-interface-guide)
5. [Backup Operations](#backup-operations)
6. [Configuration](#configuration)
7. [Troubleshooting](#troubleshooting)
8. [EasyEncrypt](#easyencrypt)
9. [EasyClient](#easyclient)

## Introduction
EasySave is a powerful versioning and saving software designed to streamline project management and development workflows. This documentation will guide you through all the features and functionalities of EasySave.

## Getting Started
### System Requirements
- Windows 10/11, macOS 10.15+, or Linux
- .NET 9.0 or later
- 4GB RAM minimum
- 500MB free disk space

### Installation
1. Download the latest release from the releases page
2. Extract the archive to your desired location
3. Run EasySave.exe (Windows) or EasySave (macOS/Linux)

## Core Features
- Unlimited backup tasks
- File encryption
- Multilingual support (English, French, Welsh)
- Real-time progress tracking
- Comprehensive logging
- Dark/Light theme support

## User Interface Guide
The application provides an intuitive interface with the following main components:
- Main window with backup task list
- Configuration panel
- Progress tracking
- Log viewer
- Language selector
- Theme switcher

## Backup Operations
### Creating a Backup Task
1. Click "New Backup" button
2. Enter a unique backup name
3. Select source directory
4. Select target directory
5. Choose backup type (full or differential)
6. Save the configuration

### Running Backups
1. Select a backup task from the list
2. Click "Start Backup"
3. Monitor progress in real-time
4. View detailed logs

## Configuration
### General Settings
- Language selection
- Theme preference
- Log location
- Auto-save settings

### Backup Settings
- Default backup type
- Encryption preferences
- Process blocking options
- Network settings

## Troubleshooting
### Common Issues
1. Backup fails to start
2. Progress not updating
3. Language not changing
4. Encryption errors

### Solutions
- Check file permissions
- Verify disk space
- Ensure network connectivity
- Review log files

## EasyEncrypt
EasyEncrypt is a standalone application from the EasySave suite of tools.
It provides simple, synchronous XOR encryption for securing data.

In addition to the main app, EasyEncrypt includes a packaged DLL pipeline, allowing any external process or future application to easily access and use its encryption methods.

## EasyClient
EasyClient is a companion application designed to remotely control the EasySave app from any device.
It allows users to start, pause, resume, or cancel saving processes in real time, providing full remote access and control over ongoing backups.
