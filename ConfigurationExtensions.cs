﻿using System;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.Heartbeat.Dashboard;

namespace Hangfire.Heartbeat
{
    public static class ConfigurationExtensions
    {
        [PublicAPI]
        public static IGlobalConfiguration UseHeartbeatPage(this IGlobalConfiguration config, TimeSpan checkInterval)
        {
            DashboardRoutes.Routes.AddRazorPage(OverviewPage.PageRoute, x => new OverviewPage(checkInterval));
            NavigationMenu.Items.Add(page => new MenuItem(OverviewPage.Title, page.Url.To(OverviewPage.PageRoute))
            {
                Active = page.RequestPath.StartsWith(OverviewPage.PageRoute)
            });
            DashboardRoutes.Routes.Add(OverviewPage.StatsRoute, new UtilizationJsonDispatcher());

            DashboardRoutes.Routes.Add(
                "/heartbeat/knockout-3.4.2.js",
                new ContentDispatcher("application/js", "Hangfire.Heartbeat.Dashboard.js.knockout-3.4.2.js",
                    TimeSpan.FromDays(30)));

            DashboardRoutes.Routes.Add(
                "/heartbeat/numeral.min.js",
                new ContentDispatcher("application/js", "Hangfire.Heartbeat.Dashboard.js.numeral.min.js", TimeSpan.FromDays(30)));

            DashboardRoutes.Routes.Add(
                "/heartbeat/OverviewPage.js",
                new ContentDispatcher("application/js", "Hangfire.Heartbeat.Dashboard.js.OverviewPage.js", TimeSpan.FromSeconds(1)));

            DashboardRoutes.Routes.Add(
                "/heartbeat/styles.css",
                new ContentDispatcher("text/css", "Hangfire.Heartbeat.Dashboard.css.styles.css", TimeSpan.FromSeconds(1)));

            return config;
        }
    }
}
