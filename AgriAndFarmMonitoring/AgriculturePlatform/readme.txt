layers definition:

infrastructure - data layer (dbcontext, migrations, repository )
domain - bussines layer (entities, enums, core business rules)
application - service layer(services, DTOs, AutoMapper, business logic)
api - web api/controller (middleware, program.cs file)



infrastructure/
  dbcontext
  repositories(implementation from Domain(IRepository))
  migrations
^
|
|
|

Domain/
  entities
  enums
  interfaces(IRepository) and business rules
^
|
|

application/
  DTOs
  interfaces(IService)
  services(Service(implementation))
  mappings

^
|
|  

API/
  controllers
  middlewares
  Hubs
  program.cs
  appsettings
