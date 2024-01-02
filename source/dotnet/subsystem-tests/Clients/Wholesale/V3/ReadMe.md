
# What are the files purpose

- nswag.json | This file is used to generate the client code in WholesaleClient.cs from a swagger.json file located at the url specified in the file.
- swagger.json | This is a copy of the swagger.json file used to generate the client code. This is so we can see what the swagger.json the client code was generated from looked like at the time of generation.
- WholesaleClient.cs | This is the generated client.

# How to generate the client code

1. Delete the swagger.json file.
2. Create VPN connection to u-001 using Azure VPN Client
3. Rebuild the SubsystemTest project.

A new swagger.json file and WholesaleClient.cs file will be generated.

# How to build the client from a different swagger.json file, than the one specified in nswag.json

Simply go to the nswag.json file and change the url to the swagger.json file you want to use.

# How is the nswag.json file run to generate the client code

In the SubsystemTest.csproj file there is a target that runs the nswag.json file. Look for the element `<Target Name="NSwag" ...>`.

# Breaking changes to the client code

We need to find out what to do in case of breaking changes to the client code and what impact it will have on the BFF.
