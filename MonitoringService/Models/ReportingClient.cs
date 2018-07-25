﻿namespace MonitoringService.Models
{
    public class ReportingClient
    {
        public int UniqueId { get; set; }
        public string BindAddress { get; set; }


        public string FQDN => $"http://{BindAddress}";
        public string StatusUrl => $"{FQDN}/status";

        internal static string GetTabUrl(int tabId) => $"tab/{tabId}";
        internal static string GetTabNextUrl() => $"tab/next";
        internal static string GetTabPreviousUrl() => $"tab/prev";

        internal static string GetEnableFullscreenUrl() => $"window/enableFullscreen";
        internal static string GetDisableFullscreenUrl() => $"window/disableFullscreen";
        internal static string GetToggleFullscreenUrl() => $"window/toggleFullscreen";
    }
}
