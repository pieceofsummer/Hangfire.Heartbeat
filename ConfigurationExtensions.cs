﻿using System;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.Heartbeat.Dashboard;

namespace Hangfire.Heartbeat
{
    public static class ConfigurationExtensions
    {
        [PublicAPI, Obsolete("Deprecated. Use UseHeartbeatPage(HeartbeatPageOptions) instead.")]
        public static IGlobalConfiguration UseHeartbeatPage(this IGlobalConfiguration config, TimeSpan checkInterval)
        {
            return UseHeartbeatPage(config, new HeartbeatPageOptions()
            {
                // preserve original formula for legacy method
                StatsPollingInterval = (int)checkInterval.TotalMilliseconds + Constants.WaitMilliseconds
            });
        }

        [PublicAPI]
        public static IGlobalConfiguration UseHeartbeatPage(this IGlobalConfiguration config, HeartbeatPageOptions options = null)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            options = options ?? new HeartbeatPageOptions();
            
            DashboardRoutes.Routes.Add(OverviewPage.StatsRoute, new UtilizationJsonDispatcher());
            
            DashboardRoutes.Routes.AddRazorPage(OverviewPage.PageRoute, x => new OverviewPage(options));
            
            NavigationMenu.Items.Add(page => new MenuItem(options.Title, page.Url.To(OverviewPage.PageRoute))
            {
                Active = page.RequestPath.StartsWith(OverviewPage.PageRoute)
            });
            
            DashboardRoutes.Routes.Add(
                "/heartbeat/jsknockout",
                new ContentDispatcher("application/js", "Hangfire.Heartbeat.Dashboard.js.knockout-3.4.2.js",
                    TimeSpan.FromDays(30)));

            DashboardRoutes.Routes.Add(
                "/heartbeat/jsnumeral",
                new ContentDispatcher("application/js", "Hangfire.Heartbeat.Dashboard.js.numeral.min.js", TimeSpan.FromDays(30)));

            DashboardRoutes.Routes.Add(
                "/heartbeat/jspage",
                new ContentDispatcher("application/js", "Hangfire.Heartbeat.Dashboard.js.OverviewPage.js", TimeSpan.FromSeconds(1)));

            DashboardRoutes.Routes.Add(
                "/heartbeat/cssstyles",
                new ContentDispatcher("text/css", "Hangfire.Heartbeat.Dashboard.css.styles.css", TimeSpan.FromSeconds(1)));

            return config;
        }
    }
}
