using Assignment3.Services.Interfaces;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace Assignment3.Services.Implementations
{
    public class ReportRenderer : IReportRenderer
    {
        public byte[] RenderReport(string path, object data)
        {
            var reportPath = HostingEnvironment.MapPath(path);

            if (string.IsNullOrEmpty(reportPath) ||
                !File.Exists(reportPath))
            {
                throw new FileNotFoundException(
                    "Report file was not found.",
                    reportPath);
            }

            var reportPackager = new ReportPackager();

            Telerik.Reporting.Report report;

            using (var stream = File.OpenRead(reportPath))
            {
                report = (Telerik.Reporting.Report)reportPackager
                    .UnpackageDocument(stream);
            }

            var table = report.Items
                .Find("table1", true)
                .FirstOrDefault() as Telerik.Reporting.Table;

            if (table != null)
            {
                table.DataSource = data;

                report.DataSource = null;
            }
            else
            {
                report.DataSource = data;
            }

            var reportSource = new InstanceReportSource
            {
                ReportDocument = report
            };

            var processor = new ReportProcessor();

            var result = processor.RenderReport(
                "PDF",
                reportSource,
                null);

            if (result.HasErrors)
            {
                throw new System.Exception(
                    "An error occurred while rendering the report.");
            }

            return result.DocumentBytes;
        }
    }
}
