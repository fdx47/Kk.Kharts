namespace KK.UG6x.StoreAndForward.Contracts;

public record SettingsRequest(string? GatewayUsername, string? GatewayPassword, int IntervalSeconds, string? InternetCheckHost, bool ForceOffline, bool OfflineOnlyMode);
