[![CodeFactor](https://www.codefactor.io/repository/github/simnico99/zirconsound/badge?s=9db312f012e5b701d5d0347de46d975baffe25f9)](https://www.codefactor.io/repository/github/simnico99/zirconsound)
[![Build Status](https://dev.azure.com/ZirconCloud/ZirconSound/_apis/build/status/Simnico99.ZirconSound?branchName=master)](https://dev.azure.com/ZirconCloud/ZirconSound/_build/latest?definitionId=4&branchName=master)
[![License](https://img.shields.io/github/license/Simnico99/ZirconSound)](https://github.com/Simnico99/ZirconSound/blob/main/LICENSE)

# ZirconSound
A bot for discord that play music from youtube..


## Configurations
You need to add configurations to the root of your project ``` appsettings.Development.json```  and ``` appsettings.json```  here are exemples:

#### appsettings.Development.json Exemple:
```json
{
  "prefix": ".",
  "token": "INSERT DEV BOT DISCORD TOKEN HERE!",
  "Logging": {
    "LogLevel": {
      "Discord": "Warning"
    }
  },
  "Serilog": {
    "Using": [],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId", "WithAssemblyName", "WithAssemblyVersion" ],
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

#### appsettings.json Exemple:

```json
{
  "prefix": "!",
  "token": "INSERT BOT DISCORD TOKEN HERE",
  "Logging": {
    "LogLevel": {
      "Discord": "Warning"
    }
  },
  "Serilog": {
    "Using": [],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId", "WithAssemblyName", "WithAssemblyVersion" ],
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "SEQ SERVER URL HERE",
          "apiKey": "SEQ SERVER KEY"
        }
      }
    ]
  }
}

```
