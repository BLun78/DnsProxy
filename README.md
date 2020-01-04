# DnsProxy
[[Deutsch](https://github.com/BLun78/DnsProxy/blob/master/README.de.md)]

## Description
A DNS proxy for software developers for the hybrid cloud in the enterprise environment.

Please consider that if the administrator of your network has not connected a public DNS, there must be a reason!

## Feature
- Default Dns e.g. DHCP DNS server
- DNS client
- DNS over HTTPS client (DoH)
- own hosts configuration with hosts.json
- read AWS VpcRndpoints for Hybrid Cloud (including ApiGateway endpoints)

## Lizenz
Frei verwendbar ohne Garantien das alles 100% nach DNS-RFC funktioniert. 
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
    "ListenerPort": 53, // Port of the DNS service - 53 is default port
    "NetworkWhitelist": [ // List of allowed networks that are allowed to access the service
      "127.0.0.1/32",
      "10.10.34.0/24"
    ],
    "DefaultQueryTimeout": 50000 // Server timeout per DNS request
  },
  "DnsDefaultServer": { // Fallback DNS Server
    "Servers": {
      "NameServers": [
        "1.1.1.1",
        "9.9.9.9",
        "8.8.4.4",
        "8.8.8.8"
      ],
      "Strategy": "Dns", // Dns or DoH recommended
      "CompressionMutation": true,
      "QueryTimeout": 5000 // Timeout for queries to external servers
    }
  },
  // Proxy settings
  "HttpProxyConfig": {
    "AuthenticationType": "None"
    "Uri": "http://127.0.0.1:8866",
    "User": ""
    "Password": ""
    "Domain": ""
    "BypassAddresses": ""
  },
  "AwsSettings": { // For hybrid cloud, the public IP address e.g. of SQS.[zone].amazonaws.com can be transferred to the Vpc endpoint.
    "UserAccounts": [
      {
        "UserAccountId": "5xxxxxxxxxxxxxx7",
        "UserName": "xxxxxxxx"
        "UserAccessKey": "Axxxxxxxxxxxxxxxxxxxxxxxxx6",
        "UserSecretKey": "mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxQ",
        "DoScan": false, // Should the acaunt
        "ScanVpcIds": [],
        "Roles": [ // Aws roles for the role change
          {
            "AwsAccountLabel": "blah, blah, blah."
            "AwsAccountId": "6xxxxxxxxxxxxxx",
            "Role": "engineer."
            "DoScan": true,
            "ScanVpcIds": [ "vpc-5xxxxxxxxxxxxxxx6" ]
          },
          {
            "AwsAccountLabel": "blublu",
            "AwsAccountId": "3xxxxxxxxxxxx4"
            "Role": "developer",
            "DoScan": false,
            "ScanVpcIds": []
          }
        ]
      }
    ]
  },
  "Aws": { // AWS Framework Configuration
    "Region": "eu-central-1"
  }
}
```

### rules.json
```Javascript
{
  "RulesConfig": {
    "Rules": [
      {
        "DomainNamePattern": "(.*)\\.box",
        "NameServer": [
          "10.10.34.21"
        ],
        "Strategy": "Dns",
        "IsEnabled": true,
        "CompressionMutation": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "(.*)\\.amazonaws.com.",
        "Strategy": "DoH",
        "NameServer": [
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://cloudflare-dns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "(.*)\\.docdb.amazonaws.com.",
        "Strategy": "DoH",
        "NameServer": [
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://cloudflare-dns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "(.*)\\.cache.amazonaws.com.",
        "Strategy": "DoH",
        "NameServer": [
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://cloudflare-dns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "(.*)\\.execute-api.eu-central-1.amazonaws.com.",
        "Strategy": "DoH",
        "NameServer": [
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://cloudflare-dns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      },
      {
        "DomainNamePattern": "(.*)\\.blun.de.",
        "Strategy": "DoH",
        "NameServer": [
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://cloudflare-dns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      },
      {
        "DomainName": "blun.de.",
        "Strategy": "DoH",
        "NameServer": [
          "https://dns.digitale-gesellschaft.ch/dns-query",
          "https://doh.opendns.com/dns-query",
          "https://cloudflare-dns.com/dns-query",
          "https://dns.google/dns-query",
          "https://dns.quad9.net/dns-query"
        ],
        "IsEnabled": true,
        "QueryTimeout": 30000
      },
      {
        "DomainName": "www.google.de",
        "NameServer": [
          "8.8.8.8",
          "8.8.4.4"
        ],
        "Strategy": "Dns",
        "IsEnabled": true,
        "CompressionMutation": true,
        "QueryTimeout": 30000
      }
      //,{
      //  "DomainNamePattern": "",
      //  "DomainName": "",
      //  "NameServer": [
      //    ""
      //  ],
      //  "IpAddress": "",
      //  "Strategy": "", // Dns, DoH
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
          "10.10.34.21"
        ],
        "DomainNames": [
          "blablabla1978.de"
        ]
      }
    ]
  }
}
```
