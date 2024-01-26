# VEGA_project
## Deploying backend
run following code lines in VS Code terminal:
```
cd vega
```
```
dotnet user-secrets init 
```
```
dotnet user-secrets set ConnectionStrings:VegaDB "Host=#;Port=#;Database=#;Username=#;Password=#"
```
```
dotnet run
```
