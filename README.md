# DotNetCore CosmosDb Sample

Command line utility to execute SQL commands against CosmosDb

- System.CommandLine.Experimental
- Microsoft.Extensions.Hosting

## Configuration

Make sure that Azure Vault contains the following entries

```
    CosmosDb:Endpoint
    CosmosDb:AuthKey
    CosmosDb:DatabaseName
    CosmosDb:ContainerName
```

## Run

[Making a tiny .NET Core 3.0 entirely self-contained single executable](https://www.hanselman.com/blog/MakingATinyNETCore30EntirelySelfcontainedSingleExecutable.aspx)

```bash
    dotnet tool install dotnet-warp -g
    # https://docs.microsoft.com/en-us/dotnet/core/rid-catalog?WT.mc_id=blog-blog-timheuer#windows-rids
     
    dotnet build -c Release -r win-x64
    dotnet publish -c Release --self-contained -r win-x64
    
    # or

    # https://docs.microsoft.com/en-us/dotnet/core/rid-catalog?WT.mc_id=blog-blog-timheuer#linux-rids
    dotnet build -c Release -r linux-x64
    dotnet publish -c Release --self-contained -r linux-x64

    dotnet run -a:https://{vaultName}.vault.azure.net/ -q:"SELECT * FROM c where c.Book ='Moby'" del
```
