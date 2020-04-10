# card_catalog_core
.NET Core Reimplementation of Rust card_catalog

Written in C#, this program collects file metadata and stores it in a SQLite database. Some things it gathers include:

1. File name
2. Path
3. Size
4. XxHash checksum

In addition to collecting data about files, the program assists in indexing files
with tools such as file tagging, search, and more. Development on these features
is underway... stay tuned!

# Getting Started

### Run locally, outside of any container:

```
# inside database project
dotnet restore
dotnet-ef database update # starting with .NET Core 3, install dotnet-ef separately

# inside CLI/classlib/etc
dotnet restore
dotnet run
```