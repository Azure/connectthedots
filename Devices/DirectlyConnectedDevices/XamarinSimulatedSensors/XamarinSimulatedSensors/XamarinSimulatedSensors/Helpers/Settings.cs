// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace XamarinSimulatedSensors.Helpers
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters. 
  /// </summary>
  public static class Settings
  {
    private static ISettings AppSettings
    {
      get
      {
        return CrossSettings.Current;
      }
    }

    #region Setting Constants

    private const string DisplayNameKey = "DisplayName";
    private static readonly string DisplayNameDefault = string.Empty;

    private const string ConnectionStringKey = "ConnectionString";
    private static readonly string ConnectionStringDefault = string.Empty;

    #endregion

    public static string DisplayName
    {
            get { return AppSettings.GetValueOrDefault<string>(DisplayNameKey, DisplayNameDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DisplayNameKey, value); }
    }

    public static string ConnectionString
    {
        get { return AppSettings.GetValueOrDefault<string>(ConnectionStringKey, ConnectionStringDefault); }
        set { AppSettings.AddOrUpdateValue<string>(ConnectionStringKey, value); }
    }

  }
}