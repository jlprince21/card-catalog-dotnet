# Card Catalog Core

Written in C#, this program helps collect file metadata and manage physical
inventory of physical objects. It is a reimplementation of the Rust card_catalog
project with more capabilities.

For files some things gathered include:

1. File name
2. Path
3. Size
4. XxHash checksum

In addition to collecting file metadata, the program assists in indexing files
with tools such as file tagging, search, and more.

On the physical inventory side, Card Catalog helps manage a collection of
containers and the items they hold. You can move items around, change container
and item descriptions, and remove containers/items from the database.

Librarian is the companion UI for Card Catalog's API. You can grab it
[here](https://github.com/jlprince21/librarian-flutter).

# Getting Started

This application requies .NET Core 5, dotnet-ef CLI tools, and a PostgreSQL
database.

## Environment Variables

CARD_CATALOG_DB_CONNECTION - required, sets the connection string to the
PostgreSQL database that will be used. Example entry in ~/.zshrc on macOS is

`export CARD_CATALOG_DB_CONNECTION="Host=<IP_ADDRESS>;Port=5432;Database=<DATABASE_NAME>;Username=<USERNAME>;Password=<PASSWORD>"`

## Running the Projects

Setup a PostgreSQL database with an empty database that the API will use. Before
running migrations, run this query in the database you want Card Catalog to use
to enable an extension needed for UUID support.

``` SQL
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

Next, inside the CardCatalog.Api directory run the migrations:

``` bash
dotnet restore
dotnet-ef database update --verbose # remember to install dotnet-ef!
```

Now you can choose to run the web API or terminal programs.

To use the web API, go to the CardCatalog.Api directory and:

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

Using the Docker Compose file provided with this project currently doesn't run
the migrations. To get started:

1. Review and set the environment variables to your desired values.
2. Run this command to start a new database, pgAdmin, and the API.

``` bash
docker-compose up -d # Remove `-d` if you want to see output
```

3. Login to pgAdmin. Create a new server registration and database. Run the UUID
   extension enabling query. You will need to get the IP address of the database
   container when creating the server in pgAdmin, use:

``` bash
docker ps
docker inspect <POSTGRESQL_CONTAINER_ID> | grep IPAddress
```

4. Run the migrations manually using the local copy of the project. On macOS
   the connection string will use 127.0.0.1 to connect to the database.

The API should now be running on port 5555. Test it with your favorite HTTP
client or try pointing Librarian at the API and giving it a try!

## Credits

`docker-compose.yml` adapted from [rafaeladolfo/DockerComposeProductApi](https://github.com/rafaeladolfo/DockerComposeProductApi)
and [Mahbub Zaman](https://towardsdatascience.com/how-to-run-postgresql-and-pgadmin-using-docker-3a6a8ae918b5)