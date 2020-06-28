# DnsProxy
[[Deutsch](https://github.com/BLun78/DnsProxy/blob/master/README.de.md)]

## Description
A DNS proxy for software developers for the hybrid cloud in the enterprise environment.

Please consider that if the administrator of your network has not connected a public DNS, there must be a reason!

## Feature
- Default Dns e. g. DHCP DNS server
- DNS client
- DNS over HTTPS client (DoH)
- own hosts configuration with hosts.json
- read AWS VpcEndpoints for Hybrid Cloud (including ApiGateway endpoints)

## License
Free use with no warranty that the DNS-RFC is full implemented.
```Text
Copyright 2020 Bjoern Lundstroem

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
[Apache License 2.0 for DNS-Proxy](https://github.com/BLun78/DnsProxy/blob/master/LICENSE)

This project uses a modified version of the library [ARSoft.Tools.Net](https://github.com/alexreinert/ARSoft.Tools.Net). This library is available under the [Apache License 2.0](https://github.com/alexreinert/ARSoft.Tools.Net/blob/master/LICENSE). Only code quality changes and the migration to .Net Core 3.1 were made.

For [DNS over HTTPS (DoH)](https://en.wikipedia.org/wiki/DNS_over_HTTPS) the library [Makaretu.Dns.Unicast](https://github.com/richardschneider/net-udns) is used. This library is available under the [MIT license](https://github.com/richardschneider/net-udns/blob/master/LICENSE)

## Requirement
- Windows 10 with .Net Core 3.1 - 64 Bit 
- Linux with .Net Core 3.1 - 64 Bit 
- MacOs with .Net Core 3.1 - 64 Bit 

## Configuration
### config.json

```Javascript
{
  "DnsHostConfig": {
    "ListenerPort": 53,
    "NetworkWhitelist": [
      "127.0.0.1/32",
      "10.10.34.0/24"
    ],
    "DefaultQueryTimeout": 30000
  },
  "DnsDefaultServer": {
    "Servers": {
      "NameServer": [
        "1.1.1.1",
        "9.9.9.9",
        "8.8.8.8"
      ],
      "StrategyName": "Dns",
      "CompressionMutation": true,
      "QueryTimeout": 4000
    }
  },
  "CacheConfig": {
    "MinimalTimeToLiveInSeconds": 60
  },
  "HttpProxyConfig": {
    "AuthenticationType": "None", // None, Basic, WindowsDomain, WindowsUser
    "Address": "",
    "Port": null,
    "User": "",
    "Password": "",
    "Domain": "",
    "BypassAddresses": ""
  },
  "AwsSettings": {
    "Region": "eu-central-1",
    "OutputFileName": "ProxyBypassList.txt",
    "UserAccounts": [
      {
        "UserAccountId": "5xxxxxxxxxxxx7",
        "UserName": "xxxxxxxx",
        "UserAccessKey": "Axxxxxxxxxxxxxxxxxxx6",
        "UserSecretKey": "mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxQ",
        "DoScan": false,
        "ScanVpcIds": [],
        "Roles": [
          {
            "AwsAccountLabel": "blabla",
            "AwsAccountId": "6xxxxxxxxxxxx",
            "Role": "engineer",
            "DoScan": true,
            "ScanVpcIds": [ "vpc-0xxxxxxxxxxxxxx6" ]
          },
          {
            "AwsAccountLabel": "blublu",
            "AwsAccountId": "3xxxxxxxxxx4",
            "Role": "developer",
            "DoScan": false,
            "ScanVpcIds": []
          }
        ]
      }
    ]
  }
}
```

### rules.json
```Javascript
{
  "RulesConfig": {
    "Rules": [
      {
        "DomainNamePattern": "(.*)\\.aws",
        "NameServer": [
          "10.10.69.153",
          "10.10.74.137"
        ],
        "StrategyName": "Dns",
        "IsEnabled": true,
        "CompressionMutation": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "ffsdfsfsdfsdffsd.corp.blun.",
        "NameServer": [
          "10.100.69.140",
          "10.100.74.173"
        ],
        "StrategyName": "Dns",
        "IsEnabled": true,
        "CompressionMutation": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "sdfsdbfsdfsdfsdf.corp.blun.",
        "NameServer": [
          "10.100.69.15",
          "10.100.74.13"
        ],
        "StrategyName": "Dns",
        "IsEnabled": true,
        "CompressionMutation": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "sdfsdff3sddfsdfds.corp.blun.",
        "NameServer": [
          "10.100.69.13",
          "10.100.74.17"
        ],
        "StrategyName": "Dns",
        "IsEnabled": true,
        "CompressionMutation": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "(.*)\\.amazonaws.com.",
        "StrategyName": "DoH",
        "NameServer": [
          "https://dns.adguard.com/dns-query",
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      }
      //,{
      //  "DomainNamePattern": "",
      //  "DomainName": "",
      //  "NameServer": [
      //    ""
      //  ],
      //  "IpAddress": "",
      //  "StrategyName": "", // Dns, DoH
      //  "IsEnabled": false,
      //  "CompressionMutation": false,
      //  "QueryTimeout": 30000
      //}
    ]
  }
}
```

### hosts.json
```Javascript
{
  "HostsConfig": {
    "Rule": {
      "IsEnabled": true
    },
    "Hosts": [
      {
        "IpAddresses": [
          "104.16.249.249",
          "104.16.248.249"
        ],
        "DomainNames": [
          "cloudflare-dns.com"
        ]
      },
      {
        "IpAddresses": [
          "8.8.4.4",
          "8.8.8.8"
        ],
        "DomainNames": [
          "dns.google"
        ]
      },
      {
        "IpAddresses": [
          "185.95.218.43",
          "185.95.218.42"
        ],
        "DomainNames": [
          "dns.digitale-gesellschaft.ch"
        ]
      },
      {
        "IpAddresses": [
          "146.112.41.2"
        ],
        "DomainNames": [
          "doh.opendns.com"
        ]
      },
      {
        "IpAddresses": [
          "149.112.112.112",
          "9.9.9.9"
        ],
        "DomainNames": [
          "dns.quad9.net"
        ]
      },
      {
        "IpAddresses": [
          "176.103.130.130",
          "176.103.130.131"
        ],
        "DomainNames": [
          "dns.adguard.com"
        ]
      },
      {
        "IpAddresses": [
          "104.18.45.204",
          "104.18.44.204"
        ],
        "DomainNames": [
          "jp.tiarap.org"
        ]
      },
      {
        "IpAddresses": [
          "146.185.167.43"
        ],
        "DomainNames": [
          "doh.securedns.eu"
        ]
      },
      {
        "IpAddresses": [
          "96.113.151.149"
        ],
        "DomainNames": [
          "doh.xfinity.com"
        ]
      }
    ]
  }
}
```
