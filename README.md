# API

A modular ASP.NET Core 10 Web API with authentication, authorization, and common enterprise features.

## Features

- **Authentication & Authorization** – Users, roles, and permissions management
- **Email** – SMTP integration with Handlebars templates
- **File Storage** – Local file management
- **Localization** – Multi-language support (en-US, es-ES)
- **Notifications & Verification Codes** – User communication utilities
- **Traces** – Activity logging

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core (SQL Server)
- Swagger / OpenAPI
- Docker support

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server

### Run Locally

```bash
dotnet restore 
dotnet ef database update 
dotnet run
```
