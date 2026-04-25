# 📊 RDS Metrics Exporter

> Automated AWS RDS CloudWatch metrics fetcher and Excel reporter for multi-account environments.

![C#](https://img.shields.io/badge/C%23-.NET-512BD4?logo=dotnet&logoColor=white)
![AWS](https://img.shields.io/badge/AWS-Multi--Account-FF9900?logo=amazonaws&logoColor=white)
![RDS](https://img.shields.io/badge/AWS-RDS-527FFF?logo=amazonaws&logoColor=white)
![CloudWatch](https://img.shields.io/badge/AWS-CloudWatch-FF4F8B?logo=amazonaws&logoColor=white)
![Excel](https://img.shields.io/badge/Export-Excel-217346?logo=microsoftexcel&logoColor=white)

---

## 📌 What is RDS Metrics Exporter?

RDS Metrics Exporter is a **C# .NET automation tool** that connects to multiple AWS accounts simultaneously, fetches **24 RDS CloudWatch metrics** and exports them into organized **Excel reports** automatically.

Built for enterprise environments managing **multiple AWS accounts** with multiple RDS instances.

---

## ⚡ Key Highlights

| Feature | Detail |
|---|---|
| ☁️ **Multi-Account** | Connects to 17+ AWS accounts simultaneously |
| 📊 **24 Metrics** | CPU, RAM, Storage, IOPS, Latency & more |
| 📁 **Auto Excel Export** | Separate Excel file per RDS instance |
| ⏱️ **Flexible Granularity** | Hour, Minute or Second level data |
| 📅 **Custom Date Range** | Fetch data for any date range |
| 🔧 **Configurable** | Enable/disable any metric via config |

---

## 📈 Metrics Tracked

| Metric | Metric | Metric |
|---|---|---|
| ✅ CPUUtilization | ✅ DatabaseConnections | ✅ FreeStorageSpace |
| ✅ ReadLatency | ✅ WriteLatency | ✅ ReadIOPS |
| ✅ WriteIOPS | ✅ ReadThroughput | ✅ WriteThroughput |
| ✅ DBLoadCPU | ✅ DBLoadNonCPU | ✅ FreeableMemory |
| ✅ NetworkReceiveThroughput | ✅ NetworkTransmitThroughput | ✅ CPUCreditUsage |
| ✅ CPUCreditBalance | ✅ DiskQueueDepth | ✅ SwapUsage |
| ✅ DBLoad | ✅ BurstBalance | ✅ EBSIOBalance% |
| ✅ EBSByteBalance% | ✅ CPUSurplusCreditBalance | ✅ CPUSurplusCreditsCharged |

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Language** | C# .NET |
| **Cloud** | AWS CloudWatch, AWS RDS |
| **Multi-Account** | AWS IAM Access Keys |
| **Export** | EPPlus (Excel generation) |
| **Config** | App.config (XML) |

---

## 🏗️ How It Works

```
1️⃣  Reads AWS credentials from config
    └── Supports 17+ AWS accounts
         ↓
2️⃣  Connects to each AWS account
    └── Uses AmazonRDSClient & CloudWatchClient
         ↓
3️⃣  Discovers all RDS instances
    └── Auto-detects instances in each account
         ↓
4️⃣  Fetches 24 CloudWatch metrics
    └── For specified date range & time window
         ↓
5️⃣  Converts byte-based metrics to GB
    └── FreeableMemory, Storage, Throughput etc.
         ↓
6️⃣  Exports data to Excel
    └── Separate file per RDS instance
```

---

## ⚙️ Configuration

The tool is fully configurable via `appsettings.sample.config`:

```xml
<!-- Enable/Disable specific metrics using binary -->
<add key="BinaryConfiguration" value="111111111111111111111111" />

<!-- Time granularity: "second", "minute", or "hour" -->
<add key="TimeGranularity" value="Hour" />

<!-- Metric sequence order in Excel -->
<add key="MetricSequence" value="CPUUtilization,DatabaseConnections,..." />
```

---

## 📁 Project Structure

```
RDS-Metrics-Exporter/
│
├── 📄 Program.cs
│       └── Main logic, metric fetching & Excel export
│
├── 📄 AWSCredentials.cs
│       └── AWS credentials model class
│
├── 📄 appsettings.sample.config
│       └── Safe configuration template
│
└── 📄 README.md
        └── Project documentation
```

---

## 🔑 Key Features In Detail

**Multi-Account Support:**
- Connects to unlimited AWS accounts simultaneously
- Each account uses separate IAM credentials
- Results organized per account and per RDS instance

**Smart Metric Filtering:**
- Binary configuration enables/disables any metric
- Example: `111100...` = only first 4 metrics enabled
- No code changes needed — just update config!

**Automatic Unit Conversion:**
- Byte-based metrics auto-converted to GB
- Includes: FreeableMemory, FreeStorageSpace,
  NetworkThroughput, ReadThroughput, WriteThroughput

**Flexible Time Range:**
- Custom start and end dates
- Custom time window per day (e.g., 10 AM to 8 PM)
- Hour/Minute/Second granularity

---

## 🏆 What Makes It Special

- **Enterprise Scale** — handles 17+ AWS accounts simultaneously
- **Zero Manual Work** — fully automated metric collection
- **Organized Output** — separate Excel per RDS instance
- **Banking Grade** — built for production banking environment
- **Highly Configurable** — no code changes needed for customization

---

## 📚 Skills Demonstrated

`C#` `.NET` `AWS RDS` `AWS CloudWatch` `Multi-Account AWS`
`Excel Automation` `EPPlus` `Cloud Monitoring` `DevOps`
`Infrastructure Reporting` `Banking Infrastructure` `Automation`

---

## 👤 Author

**Bhushan Koli** — Cloud Engineer

[![GitHub](https://img.shields.io/badge/GitHub-bhushankoli--dev-181717?logo=github)](https://github.com/bhushankoli-dev)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Bhushan%20Koli-0077B5?logo=linkedin&logoColor=white)](https://www.linkedin.com/in/bhushan-koli)

📍 Pune, India

---

*Automated AWS RDS monitoring and reporting at enterprise scale.* 🚀
