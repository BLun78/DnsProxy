# DNS-Proxy
[[English](https://github.com/BLun78/DnsProxy/blob/master/README.md)]

## Beschreibung
Ein DNS-Proxy für Softwareentwickler für die Hybrid-Cloud im Enterpriseumfeld.

Bitte bedenkt wenn der Administrator eures Netzwerkes keinen Public-DNS angeschlossen hat, hat dies sicherlich einen Grund!!!

## Feature
- Default Dns z. B. DHCP DNS-Server
- DNS Client
- DNS over HTTPS Client (DoH)
- Eigene Hosts configuration mit hosts.json
- AWS VpcRndpoints für Hybrid Cloud einlesen (inklusive ApiGateway Endpoints)

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

Diese Projekt nutzt einen modifizierte Version von der Bibliothek [ARSoft.Tools.Net](https://github.com/alexreinert/ARSoft.Tools.Net). Diese Bibliothek steht unter der [Apache License 2.0](https://github.com/alexreinert/ARSoft.Tools.Net/blob/master/LICENSE). Es wurden nur Code-Quality-Änderungen und die Migration zu .Net Core 3.1 gemacht.

Für [DNS over HTTPS (DoH)](https://en.wikipedia.org/wiki/DNS_over_HTTPS) wird die Bibliothek [Makaretu.Dns.Unicast](https://github.com/richardschneider/net-udns) verwendet. Diese Bibliothek steht unter der [MIT Lizens](https://github.com/richardschneider/net-udns/blob/master/LICENSE)

## Requirement
- Windows 10 mit .Net Core 3.1 - 64 Bit 
- Linux mit .Net Core 3.1 - 64 Bit 
- MacOs mit .Net Core 3.1 - 64 Bit 

## Konfiguration
### config.json

```Javascript
{
  "DnsHostConfig": {
    "ListenerPort": 53, // Port des DNS Dienstes - 53 ist default Port
    "NetworkWhitelist": [ // Liste der erlaubten Netzwerke die auf den Dienst zugreifen dürfen
      "127.0.0.1/32",
      "10.10.34.0/24"
    ],
    "DefaultQueryTimeout": 50000 // Server Timeout pro DNS-Request
  },
  "DnsDefaultServer": { // Fallback DNS Server
    "Servers": {
      "NameServer": [
        "1.1.1.1",
        "9.9.9.9",
        "8.8.4.4",
        "8.8.8.8"
      ],
      "Strategy": "Dns", // Dns or DoH empfohlen
      "CompressionMutation": true,
      "QueryTimeout": 5000 // Timeout bei anfragen an fremden Servern
    }
  },
  // Proxy einstellungen
  "HttpProxyConfig": {
    "AuthenticationType": "None",
    "Uri": "http://127.0.0.1:8866",
    "User": "",
    "Password": "",
    "Domain": "",
    "BypassAddresses": ""
  },
  "AwsSettings": { // Für Hybrid-Cloud kann man die Public IPAdresse z.B. von SQS.[zone].amazonaws.com hiermit auf den VpcEndpoint umschreiben.
    "UserAccounts": [
      {
        "UserAccountId": "5xxxxxxxxxxxx7",
        "UserName": "xxxxxxxx",
        "UserAccessKey": "Axxxxxxxxxxxxxxxxxxx6",
        "UserSecretKey": "mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxQ",
        "DoScan": false, // Soll der Akaunt
        "ScanVpcIds": [],
        "Roles": [ // Aws Rollen für den Rollen-Wechsel
          {
            "AwsAccountLabel": "blabla",
            "AwsAccountId": "6xxxxxxxxxxxx",
            "Role": "engineer",
            "DoScan": true,
            "ScanVpcIds": [ "vpc-0e355e36f7f9152f6" ]
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
  },
  "Aws": { // AWS Framework Konfiguration
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
