# Card Catalog Core

Written in C#, this program helps collect file metadata and manage physical
inventory of physical objects. It is a reimplementation of the Rust card_catalog
project with more capabilities.

For files some things gathered include:

1. File name
2. Path
3. Size
4. XxHash checksum

In addition to collecting data about files, the program assists in indexing
files with tools such as file tagging, search, and more.

On the physical inventory side, Card Catalog allows you to manage a collection
of containers and the items they hold. You can move items around, change
container and item descriptions, and remove containers/items from the database.

Development of a companion app is underway to support use of this system. A link
to it will go here someday TODO.

# Getting Started

This application requies .NET Core 5 and dotnet-ef CLI tools which will need to
be installed separately as of writing.

## Environment Variables

CARD_CATALOG_SQLITE_PATH - required, sets the path to the SQLite database that
will be used. Example entry in ~/.zshrc on macOS is

`export CARD_CATALOG_SQLITE_PATH="Data Source=/Users/jdoe/card_catalog_core.db"`

## Running the Projects

First, inside the CardCatalog.Core directory create the database and run
migrations:

``` bash
dotnet restore
dotnet-ef database update # remember to install dotnet-ef!
```

Now you can choose to run the web API or terminal programs.

To use the web api, go to the CardCatalog.Api directory and:

``` bash
dotnet restore
dotnet run
```

Scanning files requires using the console application. Go to
CardCatalog.Terminal directory and:

...to hash files:

``` bash
dotnet run --hash <ROOT_DIRECTORY>
```

...to search for and optionally remove files no longer present (orphans):

``` bash
dotnet run --orphans
```

## Docker Reference

As of writing, if using Docker Compose to build the web API, the code will need
a change to not look at environment variables for SQLite path.

Change the UseSqlite calls to something like this for the path:

`"Data Source=/database/card_catalog_core.db"`

Note the Docker setup is assuming a SQLite DB has already been created and
dropped into volume specified in the `docker-compose.yml`

Next, start the project up. Remove `-d` if you want to see output in the first
run:

`docker-compose up -d`

The API should now be running on port 5555.