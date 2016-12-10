
namespace Wongs.Services.Localization
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the Wongs core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface ILocalizationProvider
    {
        string GetString(string key, string resourceFileRoot);
        string GetString(string key, string resourceFileRoot, string language);
        //string GetString(string key, string resourceFileRoot, string language);
        string GetString(string key, string resourceFileRoot, string language,  bool disableShowMissingKeys);

        /// <summary>
        /// Saves a string to a resource file.
        /// </summary>
        /// <param name="key">The key to save (e.g. "MyWidget.Text").</param>
        /// <param name="value">The text value for the key.</param>
        /// <param name="resourceFileRoot">Relative path for the resource file root (e.g. "DesktopModules/Admin/Lists/App_LocalResources/ListEditor.ascx.resx").</param>
        /// <param name="language">The locale code in lang-region format (e.g. "fr-FR").</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <param name="resourceType">Specifies whether to save as portal, host or system resource file.</param>
        /// <param name="createFile">if set to <c>true</c> a new file will be created if it is not found.</param>
        /// <param name="createKey">if set to <c>true</c> a new key will be created if not found.</param>
        /// <returns>If the value could be saved then true will be returned, otherwise false.</returns>
        /// <exception cref="System.Exception">Any file io error or similar will lead to exceptions</exception>
        bool SaveString(string key, string value, string resourceFileRoot, string language,  Wongs.Services.Localization.LocalizationProvider.CustomizedLocale resourceType, bool addFile, bool addKey);
    }
}
