
## Commands

| Command      | Description            |
|:--|:--|
| `Db.ExportModelCreate.cmd`    | Export the database create model to the `ModelCreate.sql` script <sup>1)</sup> |
| `Db.ExportModelDrop.cmd`      | Export the database drop model to the `ModelDrop.sql` script <sup>1)</sup> |
| `Db.ModelCreate.cmd`          | Execute the `ModelCreate.sql` script on  the database |
| `Db.ModelDrop.cmd`            | Execute the `ModelDrop.sql` script on  the database |
| `Db.ModelUpdate.cmd`          | Execute the drop and then the create script on  the database |
| `Db.Publish.cmd`              | Creates the database setup script `SetupModel.sql` |
| `Db.VersionCreate.cmd`        | Execute the `VersionCreate.sql` script on  the database |
| `Domian.Model.Unit.Tests.cmd` | Run the domain model unit tests |
| `DotNet.Swagger.Install.cmd`  | install the dot net tool Swashbuckle for swagger |
| `SqlFormatter.cmd`            | format the raw sql export |
| `SqlScripter.cmd`             | script database to script file |
| `Swagger.Build.cmd`           | build the `swagger`.json file |

<sup>1)</sup> see [below](#database-script-export)<br />
<sup>2)</sup> `HiddenControllers` setting can not combined with `VisibleControllers` setting


## Database Script Export
1. start `SqlScripter`
2. start `SqlFormatter`
3. manual edit of [ModelCreate.sql](../Database/Current/ModelCreate.sql)
    - remove top and bottom database instrctions
4. manual edit of [ModelDrop.sql](../Database/Current/ModelDrop.sql)
    - remove top and bottom database instrctions