# VEGA_project
## Deploying backend
run following code in VS Code terminal:
``
dotnet user-secrets init // optional, if you do not have UserSecretID

dotnet user-secrets set ConnectionStrings:VegaDB "Host=#;Port=#;Database=#;Username=#;Password=#"

dotnet run
```