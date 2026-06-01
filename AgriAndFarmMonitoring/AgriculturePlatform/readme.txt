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


entities:

--> crop monitoring tables:

1. fields
2. crop_cycles
3 sensor_readings
4. alerts
5. observations
6. weather_data

--> workers table
1. workers
2. tasks
3. attendance

--> yield reports tables
1. harvests
2. quality_checks
3. yield_reports


repositories and implementations:

feilds --> 
  add feild, 
  get feild, 
  fetch feild by id & name, soil_type
  update feild, 
  delete feild

crop_cycles
  add crop_cycle
  get crop_cycle by id, feild_id, growth_stage
  update crop_cycle by feild_id and id
  delete crop_cycle by id, feild_id

sensor_readings
  get all readings,
  get by feild_id, crop_cycle_id,

alerts
  get all alerts
  get by feild_id, crop_cycle_id, alert_type, severity

observations
  get all,
  get by id, feild_id, feild_name, crop_cycle_id or name
  get observations by worker_id, and by date filter

weather data
  get all,
  get by feild_id
  get history from date range

--> workers management 

workers table
  get all workers,
  get by id, name, phonenum, role, hire_date, status

tasks
  add/asign task,
  get all tasks ,
  get by id, worker_id, feild_id, crop_cycle_id,task_name, assigned_date and date 
  and tasks assigned in date ranges
  get tasks by status


--> yield reports

harvests
  add harvest
  get all, 
  get by id, feild_id, crop_cycle_id, 
  get harvests by worker_id
  and by date
  by date range
  get harvests by quality_grade

quality_checks
  add quality
  get all,
  get by id, harvest_id, 
  get harvests checked by worker_id

yield_reports
  get all reports
  get by crop_cycle_id



---> logs are important for all operations
--> phone num, and email like credentials should be encrypted!

