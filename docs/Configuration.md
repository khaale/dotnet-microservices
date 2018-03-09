# Configuration

## Requirements
We want to support
- Static configuration, provided by external tool (i.e. k8s ConfigMap) as config files or environment variables in deployment time.
- Dynamic configuration, provided by external tool (Consul) via REST API.

## Solution

### Static configuration
There is a [build-in support](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration) for static configuration from various sources.
Supported options:
- File formats (INI, JSON, and XML)
- Command-line arguments
- Environment variables
- In-memory .NET objects
- An encrypted user store
- Azure Key Vault
- Custom providers (installed or created)

### Dynamic configuration

[Winton.Extensions.Configuration.Consul](https://github.com/wintoncode/Winton.Extensions.Configuration.Consul) provides ASP.NET Core configuration provider for Consul.
Remarks:
- Supports dynamic configuration change.
- All the configuration should be stored in a single Consul KV value in json format. Tree-like KV structure support is in backlog. 
