# Blazor Web Starter
This project serves as a quick start to use when building monolith blazor apps.

Currently this template handles the hosting, startup, and backend setup.

Future version of this template will incorporate more Blazor related design decisions, as well as Authentication examples.

This project assumes:
- You will deploy the project as a Docker container in production
- There will be a reverse proxy such as NGINX handling HTTPS termination placed in front of the application
- Postgres is used as the database and has a max connection limit of at least 50 concurrent connections, ideally more.
- You may have background servces such as UDP listeners, etc. that can run as additional Daemons from this hosted instance. (IE if you are planning to setup microserves, this template is overkill in how it sets up the hosts)
- You plan to use Bootstrap with MDI Icons for the UI framework.
- You plan to build your database schema code first and want migrations applied on app startup.

All of these assumptions are easy to adapt and change from this template, but out of the box these have been made for you.
