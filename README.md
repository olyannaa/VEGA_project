# VEGA_project
## Deploying backend
### run following code lines in VS Code terminal:
Open vega folder
```
cd vega
```
Create UserSecrets, if you do not have one for project (check .csproj file).
```
dotnet user-secrets init 
```
Add connection string to user secrets. Put your exact params of Postgres database.
```
dotnet user-secrets set ConnectionStrings:VegaDB "HOST=#;Port=#;Database=#;Username=#;Password=#;SSL Certificate=*path*;SSL Key=*path*;Root Certificate=*path*"
```
Run application.
```
dotnet run
```
