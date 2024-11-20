## Requirements for this project to work:
### Database requirements:
* PostgreSQL running localy on port 5432
* database name is 'xpro'
* user for this database is set as 'xpro'
* user password for database is 'XproApp'
### Project is using EF core as ORM. Database needs to be migrated before it can be seeded using a controller.
* Run the database migrations
* Run the project
* Go to the url where the project's Swagger UI opens up and find Seeder section
* Run the only available command there to seed the database with two users and allowed break duration.
## Front-end login credentials:
* admin: username -> admin, password -> Password123!
* user: username -> admin, password -> Password123!
* Instructions for serving the front-end are available in the other repository, xproFront
