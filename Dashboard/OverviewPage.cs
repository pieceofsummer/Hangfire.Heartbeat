using System;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;

namespace Hangfire.Heartbeat.Dashboard
{
    internal class OverviewPage : RazorPage
    {
        public const string PageRoute = "/heartbeat";
        public const string StatsRoute = "/heartbeat/stats";

        private static readonly string PageHtml;

        private readonly HeartbeatPageOptions _options;

        static OverviewPage()
        {
            PageHtml = Utils.ReadStringResource("Hangfire.Heartbeat.Dashboard.html.OverviewPage.html");
        }

        public OverviewPage(HeartbeatPageOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public override void Execute()
        {
            Layout = new LayoutPage(_options.Title);
            
            // write static html content
            WriteLiteral(PageHtml);
            WriteEmptyLine();
            
            // write configuration element
            WriteLiteral("<div id=\"heartbeatConfig\" data-pollinterval=\"");
            Write(_options.StatsPollingInterval);
            WriteLiteral("\" data-pollurl=\"");
            Write(Url.To(StatsRoute));
            WriteLiteral("\"></div>");
        }

        private void WriteEmptyLine()
        {
            WriteLiteral("\r\n");
        }
    }
}
