using Amazon.RDS;
using Amazon.RDS.Model;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using OfficeOpenXml;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Collections.Specialized;
using System.Data;

namespace RDSMetrics
{
    class Program
    {
        static DataSet storedData = new DataSet();

        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var section = ConfigurationManager.GetSection("AWSCredentials") as NameValueCollection;
            if (section == null)
            {
                throw new ConfigurationErrorsException("AWSCredentials section is missing from app.config file.");
            }

            var accessKeys = new List<AWSCredentials>();

            for (int i = 0; i < section.AllKeys.Length; i += 2)
            {
                string _key = section.GetValues(section.Keys[i])[0];
                string _secret = section.GetValues(section.Keys[i + 1])[0];
                accessKeys.Add(new AWSCredentials(_key, _secret));
            }

            // Retrieve the time granularity from the app.config file
            var timeGranularity = ConfigurationManager.AppSettings["TimeGranularity"];

            foreach (AWSCredentials credentials in accessKeys)
            {
                Console.WriteLine($"Using AWS Access Key: {credentials.KeyID}");

                var rdsClient = new AmazonRDSClient(credentials.KeyID, credentials.SecretKey, Amazon.RegionEndpoint.APSouth1);
                var cwClient = new AmazonCloudWatchClient(credentials.KeyID, credentials.SecretKey, Amazon.RegionEndpoint.APSouth1);

                var instances = rdsClient.DescribeDBInstances().DBInstances;

                var metricSequence = ConfigurationManager.AppSettings["MetricSequence"].Split(',').ToList();
                var binaryConfiguration = ConfigurationManager.AppSettings["BinaryConfiguration"];
                var metricFilter = new StringBuilder();
                for (int i = 0; i < binaryConfiguration.Length && i < metricSequence.Count; i++)
                {
                    metricFilter.Append(binaryConfiguration[i] == '1' ? $"{metricSequence[i]}," : "");
                }
                if (metricFilter.Length > 0)
                {
                    metricFilter.Remove(metricFilter.Length - 1, 1);
                    metricSequence = metricFilter.ToString().Split(',').ToList();
                }

                // Define your desired start date and end date
                // var startDate = new DateTime(2024, 07, 12).ToLocalTime(); // Replace with your desired start date
                // var endDate = new DateTime(2024, 07, 12).ToLocalTime();   // Replace with your desired end date
                var startDate = new DateTime(2026, 02, 01, 00, 00, 00, DateTimeKind.Local);
                var endDate = new DateTime(2026, 02, 28, 00, 00, 00, DateTimeKind.Local);
                //DateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
                // Define the time range (10 AM to 8 PM) for each day
                var startTime = startDate.ToLocalTime().AddHours(10); // .AddHours(10).ToLocalTime();     // Set the start time to 10 AM
                var endTime = startDate.ToLocalTime().AddHours(20);       //AddHours(2).ToLocalTime();       // Set the end time to 8 PM

                while (startDate <= endDate)
                {
                    Console.WriteLine($"Fetching data for {startDate:yyyy-MM-dd}");

                    foreach (var instance in instances)
                    {
                        Console.WriteLine($"Instance ID: {instance.DBInstanceIdentifier}");

                        // Get list of available metrics for this RDS instance
                        var listMetricsRequest = new ListMetricsRequest
                        {
                            Dimensions = new List<DimensionFilter>
                            {
                                new DimensionFilter
                                {
                                    Name = "DBInstanceIdentifier",
                                    Value = instance.DBInstanceIdentifier
                                }
                            },
                            Namespace = "AWS/RDS"
                        };
                        var metricsResponse = cwClient.ListMetrics(listMetricsRequest);

                        // Set the period based on the time granularity
                        int period;
                        switch (timeGranularity.ToLower())
                        {
                            case "second":
                                period = 1;
                                break;
                            case "minute":
                                period = 60;
                                break;
                            case "hour":
                                period = 3600;
                                break;
                            default:
                                throw new ConfigurationErrorsException("Invalid TimeGranularity specified in app.config file.");
                        }

                        // Retrieve datapoints for each available metric within the desired time range
                        var metricData = new Dictionary<string, List<Datapoint>>();
                        foreach (var metricName in metricSequence)
                        {
                            var metric = metricsResponse.Metrics.FirstOrDefault(m => m.MetricName == metricName);
                            if (metric == null) continue;

                            var getMetricRequest = new GetMetricStatisticsRequest
                            {
                                Dimensions = metric.Dimensions,
                                MetricName = metric.MetricName,
                                Namespace = metric.Namespace,
                                StartTimeUtc = startTime, // Use the start time (10 AM)
                                EndTimeUtc = endTime,     // Use the end time (8 PM)
                                Period = period,
                                Statistics = new List<string> { "Average" }
                            };
                            var metricDataResponse = cwClient.GetMetricStatistics(getMetricRequest);
                            if (metricDataResponse.Datapoints.Any())
                            {
                                metricData.Add(metric.MetricName, metricDataResponse.Datapoints);
                            }
                        }

                        // Add metric data to the storedData DataSet
                        AddDataToDataSet(instance, metricData, metricSequence);
                    }

                    // Move to the next day
                    startDate = startDate.AddDays(1);

                    // Adjust the start and end times for the next day
                    startTime = startDate.AddHours(10);
                    endTime = startDate.AddHours(20);
                }
            }
        }

        static void AddDataToDataSet(DBInstance instance, Dictionary<string, List<Datapoint>> metricData, List<string> metricSequence)
        {
            var table = storedData.Tables[instance.DBInstanceIdentifier];
            if (table == null)
            {
                // Create a new DataTable if it doesn't exist
                table = new DataTable(instance.DBInstanceIdentifier);

                // Add columns to the DataTable
                table.Columns.Add("Serial No.", typeof(int));
                table.Columns.Add("Timestamp", typeof(DateTime));

                foreach (var metricName in metricSequence)
                {
                    table.Columns.Add(metricName, typeof(double));
                }

                // Add the DataTable to the storedData DataSet
                storedData.Tables.Add(table);
            }

            // Get the last serial number
            int lastSerialNo = table.Rows.Count;

            // Make a copy of the metricSequence list before modifying it
            var modifiedMetricSequence = new List<string>(metricSequence);

            // Add new rows for each timestamp and metric data
            var timestamps = new HashSet<DateTime>(metricData.Values.SelectMany(dp => dp.Select(p => p.Timestamp)));
            var sortedTimestamps = timestamps.OrderBy(ts => ts);
            foreach (var timestamp in sortedTimestamps)
            {
                lastSerialNo++;
                var newRow = table.Rows.Add(lastSerialNo, timestamp);
                for (int i = 0; i < modifiedMetricSequence.Count; i++)
                {
                    var metricName = modifiedMetricSequence[i];
                    if (metricData.ContainsKey(metricName))
                    {
                        var datapoints = metricData[metricName];
                        var metric = datapoints.FirstOrDefault(dp => dp.Timestamp == timestamp);
                        if (metric != null)
                        {
                            // Check if the metric is byte-based and convert to MB
                            if (IsByteBasedMetric(metricName))
                            {
                                // newRow[i + 2] = metric.Average / (1024 * 1024 * 1024); // Convert bytes to MB
                                newRow[i + 2] = metric.Average;
                            }
                            else
                            {
                                newRow[i + 2] = metric.Average;
                            }
                        }
                        else
                        {
                            newRow[i + 2] = DBNull.Value;
                        }
                    }
                    else
                    {
                        newRow[i + 2] = DBNull.Value;
                    }
                }
            }

            // For Generate Excel file
            GenerateExcelFile();
        }

        static void GenerateExcelFileOnDemand(string instanceId)
        {
            var table = storedData.Tables[instanceId];
            if (table == null)
            {
                Console.WriteLine($"No data found for instance ID: {instanceId}");
                return;
            }

            // Create a file path using instance ID
            var fileName = $"{instanceId}.xlsx";
            var filePath = Path.Combine(@"D:\Cloudwatch matrices excel", fileName);

            try
            {
                using (var package = File.Exists(filePath) ? new ExcelPackage(new FileInfo(filePath)) : new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == instanceId);
                    if (worksheet == null)
                    {
                        // Create a new worksheet if it doesn't exist
                        worksheet = package.Workbook.Worksheets.Add(instanceId);

                        // Write headers for each metric column in the specified order
                        worksheet.Cells[1, 1].Value = "Serial No.";
                        worksheet.Cells[1, 2].Value = "Timestamp";
                        for (int i = 0; i < table.Columns.Count - 2; i++)
                        {
                            worksheet.Cells[1, i + 3].Value = table.Columns[i + 2].ColumnName;
                        }

                        // Set the column width of the serial no. and timestamp columns to auto-fit the content
                        worksheet.Column(1).AutoFit();
                        worksheet.Column(2).AutoFit();
                    }

                    // Get the last row index in the worksheet
                    var lastRowIndex = worksheet.Dimension?.End.Row ?? 1;

                    // Add an empty row if there is already data in the worksheet
                    if (lastRowIndex > 1)
                    {
                        lastRowIndex++;
                        worksheet.InsertRow(lastRowIndex, 1);
                    }

                    // Add the data from the storedData DataSet to the worksheet
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        var rowIndex = lastRowIndex + i + 1;

                        // Format the timestamp value
                        var timestampValue = row["Timestamp"] != DBNull.Value ? ((DateTime)row["Timestamp"]).ToString("yyyy-MM-dd HH:mm:ss") : null;
                        worksheet.Cells[rowIndex, 2].Value = timestampValue;

                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            if (j == 1) // Skip the second column (timestamp) since it is already set above
                                continue;

                            var metricName = table.Columns[j].ColumnName;
                            if (IsByteBasedMetric(metricName))
                            {
                                var valueInBytes = row[j] != DBNull.Value ? (double)row[j] : (double?)null; // Use nullable double
                                if (valueInBytes.HasValue)
                                {
                                    var valueInMB = valueInBytes / (1024 * 1024 * 1024); // Convert bytes to GB
                                    worksheet.Cells[rowIndex, j + 1].Value = valueInMB;
                                }
                                else
                                {
                                    worksheet.Cells[rowIndex, j + 1].Value = null;
                                }
                            }
                            else
                            {
                                worksheet.Cells[rowIndex, j + 1].Value = row[j] != DBNull.Value ? row[j] : null;
                            }
                        }
                    }

                    // Save the file
                    var file = new FileInfo(filePath);
                    package.SaveAs(file);
                    Console.WriteLine($"Excel file saved at: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the Excel file: {ex.Message}");
            }
        }

        static void GenerateExcelFile()
        {
            foreach (DataTable table in storedData.Tables)
            {
                GenerateExcelFileOnDemand(table.TableName);
            }

            // Clear the stored data after generating the Excel files
            storedData.Clear();
        }

        // Function to check if a metric name corresponds to a byte-based metric
        static bool IsByteBasedMetric(string metricName)
        {
            var byteBasedMetrics = new List<string>
            {
                "FreeableMemory",
                "FreeStorageSpace",
                "NetworkReceiveThroughput",
                "NetworkTransmitThroughput",
                "ReadThroughput",
                "WriteThroughput",
                "SwapUsage",
            };

            return byteBasedMetrics.Contains(metricName);
        }
    }
}
