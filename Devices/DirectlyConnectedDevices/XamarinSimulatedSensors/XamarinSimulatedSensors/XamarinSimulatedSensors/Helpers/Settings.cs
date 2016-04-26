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

    private const string DeviceNameKey = "DeviceName";
    private static readonly string DeviceNameDefault = string.Empty;

    private const string DeviceConnectionStringKey = "DeviceName";
    private static readonly string DeviceConnectionStringDefault = string.Empty;

    #endregion

    public static string DeviceName
    {
            get { return AppSettings.GetValueOrDefault<string>(DeviceNameKey, DeviceNameDefault); }
            set { AppSettings.AddOrUpdateValue<string>(DeviceNameKey, value); }
    }

    public static string DeviceConnectionString
    {
        get { return AppSettings.GetValueOrDefault<string>(DeviceConnectionStringKey, DeviceConnectionStringDefault); }
        set { AppSettings.AddOrUpdateValue<string>(DeviceConnectionStringKey, value); }
    }

  }
}