# Subscribing to Events of other Green Energy Hub Domains

These guidelines are mainly based on the work and solutions from team Batman
as implemented in the metering point domain.

* Add copy of protobuf contract (`*.proto`)
* Configure .NET project to auto generate .NET classes by adding the following to the project file

  ```csproj
    <ItemGroup>
    <Protobuf Include="**/*.proto">
      <GrpcServices>None</GrpcServices>
      <Access>Public</Access>
      <ProtoCompile>True</ProtoCompile>
      <CompileOutputs>True</CompileOutputs>
      <OutputDir>obj\Debug\net6.0\</OutputDir>
      <Generator>MSBuild:Compile</Generator>
    </Protobuf>
  </ItemGroup>
  ```
  
* Add the NuGet package that enables the generation from the step above

  ```csproj
  <PackageReference Include="Grpc.Tools" Version="2.37.0" PrivateAssets="All"/>
  ```

* Add the NuGet package containing the protobuf types used in the generated .NET classes

  ```csproj
  <PackageReference Include="Google.Protobuf" Version="3.15.8" />
  ```
