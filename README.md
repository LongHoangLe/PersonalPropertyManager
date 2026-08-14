# Personal Property Manager

A WPF (.NET 9.0) desktop application for tracking personal belongings and property,
backed by **Microsoft EF Core 9** with a **SQLite** database. Each item can carry a
picture, a current value (USD), a description (work experiences, skills associated
with the item), a desire status (Wanted / Needed / None), and a type.

The app supports full CRUD: **view, add, update, delete, modify** items.

## Features

- **CRUD over SQLite** via EF Core 9 (Code-First, `EnsureCreated` on startup).
- **MVVM** architecture (`CommunityToolkit.Mvvm`) with constructor-injected services.
- **Image attachment** — pick a picture from disk; the file is copied next to the app
  and stored as a path on the row.
- **Filtering & searching** by text, item type, and desire status.
- **Totals** — live count and total value (USD) across the loaded set.
- **Seed data** — 4 example rows so the UI shows content on first launch.
- **Clean styling** — a single `Styles.xaml` resource dictionary replaces the WPF
  default chrome.

## Project layout

```
PersonalPropertyManager/
├── App.xaml / App.xaml.cs           # WPF entry point; wires up DI, EF Core, shows MainWindow
├── PersonalPropertyManager.csproj   # net9.0-windows, EF Core 9, CommunityToolkit.Mvvm
├── Models/
│   ├── PersonalProperty.cs          # Entity (INotifyPropertyChanged + EF attributes)
│   └── Enums.cs                     # DesireStatus, ItemType
├── Data/
│   ├── PropertyDbContext.cs         # EF Core DbContext + HasData seed rows
│   └── DbInitializer.cs             # Ensures DB file exists and schema is created
├── Services/
│   ├── IPropertyService.cs          # Abstraction for testability
│   └── PropertyService.cs           # CRUD + search over the DbContext
├── ViewModels/
│   └── MainViewModel.cs             # All commands and bindable state
├── Views/
│   ├── MainWindow.xaml(.cs)         # Two-pane layout: list on the left, editor on the right
│   └── Styles.xaml                  # Implicit styles (window, button, datagrid, etc.)
├── Converters/
│   └── Converters.cs                # ImagePath→Bitmap, currency, visibility, enum labels
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   └── Settings.settings / Settings.Designer.cs
└── README.md
```

## Database

The DB lives at `app.db` next to the published `.exe`. To use a different path, set
the `PPM_DB` environment variable before launching:

```powershell
$env:PPM_DB = "C:\Data\mystuff.db"
dotnet run
```

### Schema

Single table, `Properties`, defined in `PropertyDbContext.OnModelCreating`:

| Column          | Type            | Notes                                    |
|-----------------|-----------------|------------------------------------------|
| Id              | int             | Primary key, identity                    |
| Name            | nvarchar(200)   | Required                                 |
| Description     | nvarchar(4000)  | Work experience / skills / free text     |
| CurrentValue    | decimal(18,2)   | USD                                      |
| ImagePath       | nvarchar(500)   | Absolute path to the attached picture    |
| DesireStatus    | int             | 0=None, 1=Wanted, 2=Needed               |
| ItemType        | int             | See `Enums.cs`                           |
| AcquiredDate    | datetime        |                                          |
| Notes           | nvarchar(2000)  | Free-form notes                          |
| Location        | nvarchar(200)   | Where the item is kept                   |
| CreatedAt       | datetime        | Set on insert                            |
| UpdatedAt       | datetime        | Set on every save                        |

## Build & run

> Requires Windows + .NET 9 SDK (`dotnet --list-sdks` should show `9.x`).

```powershell
cd PersonalPropertyManager
dotnet restore
dotnet build
dotnet run
```

To publish a self-contained build:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -o publish
```

## How to use

1. **Browse** — the seeded items appear in the DataGrid on the left.
2. **Add** — click `+ Add New`; an empty editor opens on the right. Fill the fields and
   click `Save`.
3. **Edit** — click any row to load it into the editor, change fields, click `Save`.
4. **Delete** — select a row and click `Delete` (confirms first).
5. **Image** — click `Browse Image…` in the toolbar or the editor's quick-action panel.
   The image is copied into `Resources/Images/` next to the `.exe`.
6. **Filter** — type in the search box, choose a type and/or status, click `Search`.
   `Clear` resets the filters.

## Notes on the stack

- **EF Core 9** (`Microsoft.EntityFrameworkCore`, `…Sqlite`, `…Tools`, `…Design`)
- **CommunityToolkit.Mvvm 8.3.2** for `[ObservableProperty]` and `[RelayCommand]`
- **Microsoft.Extensions.DependencyInjection 9.0** — used in `App.xaml.cs` to wire
  `DbContext`, services, and view-models
- WPF (`UseWPF=true`, `net9.0-windows`)