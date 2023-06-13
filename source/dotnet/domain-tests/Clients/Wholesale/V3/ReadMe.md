
## What are the files purpose.

- nswag.json | This file is used to generate the client code in WholesaleClient.cs from a swagger.json file located at the url specified in the file.
- swagger.json | This is a copy of the swagger.json file used to generate the client code. This is so we can see what the swagger.json the client code was generated from looked like at the time of generation.
- WholesaleClient.cs | This is the generated client.

## How to generate the client code

1. Delete the swagger.json file.
2. Rebuild the DomainTest project.
3. A new swagger.json file and WholesaleClient.cs file will be generated.

## How to build the client from a different swagger.json file, than the one specified in nswag.json.

Simply go to the nswag.json file and change the url to the swagger.json file you want to use.

## How is the nswag.json file run to generate the client code.

In the DomainTest.csproj file there is a target that runs the nswag.json file.
```c#
  <Target Name="NSwag" AfterTargets="PostBuildEvent" Condition=" '$(Configuration)' == 'Debug' ">
    <Exec WorkingDirectory="$(ProjectDir)" EnvironmentVariables="ASPNETCORE_ENVIRONMENT=Development" Command="if not exist Clients/Wholesale/V3/swagger.json $(NSwagExe_Net60) run Clients/Wholesale/V3/nswag.json /variables:Configuration=$(Configuration)" Condition="!Exists('Clients/Wholesale/V3/swagger.json')" />
  </Target>
```