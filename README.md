# Farmacontrol

A full-featured pharmacy management system built with .NET 10. Handles the complete operational cycle — inventory, purchasing, stock reception, point of sale, and reporting — through a modern desktop GUI and a lightweight TUI for headless environments.

## Features

- **Inventory management** — product catalog with real-time search, batch tracking (FEFO), low-stock and expiry alerts
- **Purchase orders** — auto-generated restock suggestions, in-transit order tracking, and batch-level stock reception
- **Point of sale** — barcode/code scanner support, multi-payment methods, change calculation, optional prescription attachment
- **Sale voiding** — three stock resolution modes (return, write-off, or registration error) with mandatory audit trail
- **Reporting** — daily, monthly, annual and custom-range sales reports with PDF and Excel export
- **User management** — role-based access (Administrator / Employee) with bcrypt-hashed passwords

## Tech Stack

| | |
|---|---|
| Runtime | .NET 10 |
| Desktop UI | Avalonia UI 12 + Avalonia.Markup.Declarative |
| MVVM | CommunityToolkit.Mvvm 8.4 |
| ORM | Entity Framework Core 10 |
| Database | SQLite |
| PDF export | QuestPDF |
| Excel export | ClosedXML |
| Auth | BCrypt.Net-Next |

## Project Structure

```
Farmacontrol.Core          # Domain models, services, EF Core repository
Farmacontrol.Desktop       # Avalonia UI desktop application
Farmacontrol.ConsoleApp    # CLI application (shared Core)
```

## Getting Started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/your-username/farmacontrol.git
cd farmacontrol
dotnet restore

# Desktop app
dotnet run --project Farmacontrol.Desktop

# CLI app
dotnet run --project Farmacontrol.ConsoleApp
```

The database is created and migrated automatically on first run. No server setup required — SQLite is stored locally at `%LocalAppData%\Farmacontrol\farmacontrol.db` on Windows and `~/.local/share/Farmacontrol/farmacontrol.db` on macOS/Linux.

## License

MIT
