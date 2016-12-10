#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Wongs.Services.Localization.Internal;
using System.Text.RegularExpressions;

#endregion

namespace Wongs.Services.Localization
{
    /// <summary>
    /// Localization class support localization in system.
    /// </summary>
    /// <remarks>
    /// <para>As Wongs is used in more and more countries it is very important to provide modules with 
    /// good support for international users. Otherwise we are limiting our potential user base to 
    /// that using English as their base language.</para>
    /// <para>
    /// You can store the muti language content in resource files and use the api below to get localization content.
    /// Resouces files named as: Control(Page)Name + Extension (.aspx/.ascx ) + Language + ".resx"
    /// e.g: Installwizard.aspx.de-DE.resx
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// pageCreationProgressArea.Localization.Total = Localization.GetString("TotalLanguages", LocalResourceFile);
    /// pageCreationProgressArea.Localization.TotalFiles = Localization.GetString("TotalPages", LocalResourceFile);
    /// pageCreationProgressArea.Localization.Uploaded = Localization.GetString("TotalProgress", LocalResourceFile);
    /// pageCreationProgressArea.Localization.UploadedFiles = Localization.GetString("Progress", LocalResourceFile);
    /// pageCreationProgressArea.Localization.CurrentFileName = Localization.GetString("Processing", LocalResourceFile);
    /// </code>
    /// </example>
    public class Localization
    {
        //private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Localization));

        #region Private Members

        private static string _defaultKeyName = "resourcekey";
        //private static readonly ILocaleController LocaleController.Instance = LocaleController.Instance;
        //private static readonly ILocalizationProvider _localizationProvider = LocalizationProvider.Instance;
        private static Nullable<Boolean> _showMissingKeys;

        #endregion

        #region Public Shared Properties
        
        /// <summary>
        /// Returns ~/App_GlobalResources
        /// </summary>
        public static string ApplicationResourceDirectory
        {
            get
            {
                return "~/App_LocalResources";
            }
        }

        /// <summary>
        /// Returns ~/App_GlobalResources/Exceptions.resx
        /// </summary>
        public static string ExceptionsResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/Exceptions.resx";
            }
        }

        /// <summary>
        /// Returns ~/App_GlobalResources/GlobalResources.resx
        /// </summary>
        public static string GlobalResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/GlobalResources.resx";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The KeyName property returns and caches the name of the key attribute used to lookup resources.
        /// This can be configured by setting ResourceManagerKey property in the web.config file. The default value for this property
        /// is 'key'.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string KeyName
        {
            get
            {
                return _defaultKeyName;
            }
            set
            {
                _defaultKeyName = value;
                if (String.IsNullOrEmpty(_defaultKeyName))
                {
                    _defaultKeyName = "resourcekey";
                }
            }
        }

        public static string LocalResourceDirectory
        {
            get
            {
                return "App_LocalResources";
            }
        }

        public static string LocalSharedResourceFile
        {
            get
            {
                return "SharedResources.resx";
            }
        }

        public static string SharedResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/SharedResources.resx";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// determines whether to render a visual indicator that a key is missing
        /// is 'key'.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool ShowMissingKeys
        {
            get
            {
                return false;

                //if (_showMissingKeys == null)
                //{
                //    if (Config.GetSetting("ShowMissingKeys") == null)
                //    {
                //        _showMissingKeys = false;
                //    }
                //    else
                //    {
                //        _showMissingKeys = bool.Parse(Config.GetSetting("ShowMissingKeys".ToLower()));
                //    }
                //}

                //return _showMissingKeys.Value;
            }
        }

        public static string SupportedLocalesFile
        {
            get
            {
                return ApplicationResourceDirectory + "/Locales.xml";
            }
        }

        public static string SystemLocale
        {
            get
            {
                return "en-US";
            }
        }

        public static string SystemTimeZone
        {
            get
            {
                return "Pacific Standard Time";
            }
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CurrentCulture returns the current Culture being used
        /// is 'key'.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string CurrentCulture
        {
            get
            {
                //_CurrentCulture
                return Thread.CurrentThread.CurrentCulture.ToString();
            }
        }

        /// <summary>
        /// The CurrentUICulture for the Thread
        /// </summary>
        public string CurrentUICulture
        {
            // _CurrentCulture
            get
            {
                return Thread.CurrentThread.CurrentUICulture.ToString();
            }
        }

        #endregion

        #region Private Shared Methods

        private static void LocalizeDataControlField(DataControlField controlField, string resourceFile)
        {
            string localizedText;

            //Localize Header Text
            if (!string.IsNullOrEmpty(controlField.HeaderText))
            {
                localizedText = GetString((controlField.HeaderText + ".Header"), resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    controlField.HeaderText = localizedText;
                    controlField.AccessibleHeaderText = controlField.HeaderText;
                }
            }
            if (controlField is TemplateField)
            {
                //do nothing
            }
            else if (controlField is ButtonField)
            {
                var button = (ButtonField)controlField;
                localizedText = GetString(button.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    button.Text = localizedText;
                }
            }
            else if (controlField is CheckBoxField)
            {
                var checkbox = (CheckBoxField)controlField;
                localizedText = GetString(checkbox.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    checkbox.Text = localizedText;
                }
            }
            else if (controlField is CommandField)
            {
                var commands = (CommandField)controlField;
                localizedText = GetString(commands.CancelText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.CancelText = localizedText;
                }
                localizedText = GetString(commands.DeleteText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.DeleteText = localizedText;
                }
                localizedText = GetString(commands.EditText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.EditText = localizedText;
                }
                localizedText = GetString(commands.InsertText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.InsertText = localizedText;
                }
                localizedText = GetString(commands.NewText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.NewText = localizedText;
                }
                localizedText = GetString(commands.SelectText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.SelectText = localizedText;
                }
                localizedText = GetString(commands.UpdateText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.UpdateText = localizedText;
                }
            }
            else if (controlField is HyperLinkField)
            {
                var link = (HyperLinkField)controlField;
                localizedText = GetString(link.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    link.Text = localizedText;
                }
            }
            else if (controlField is ImageField)
            {
                var image = (ImageField)controlField;
                localizedText = GetString(image.AlternateText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    image.AlternateText = localizedText;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts old TimeZoneOffset to new TimeZoneInfo. 
        /// </summary>
        /// <param name="timeZoneOffsetInMinutes">An offset in minutes, e.g. -480 (-8 times 60) for Pasicif Time Zone</param>        
        /// <returns>TimeZoneInfo is returned if timeZoneOffsetInMinutes is valid, otherwise TimeZoneInfo.Local is returned.</returns>
        /// <remarks>Initial mapping is based on hard-coded rules. These rules are hard-coded from old standard TimeZones.xml data.
        /// When offset is not found hard-coded mapping, a lookup is performed in timezones defined in the system. The first found entry is returned.
        /// When mapping is not found, a default TimeZoneInfo.Local us returned.</remarks>
        public static TimeZoneInfo ConvertLegacyTimeZoneOffsetToTimeZoneInfo(int timeZoneOffsetInMinutes)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            //lookup existing mapping
            switch (timeZoneOffsetInMinutes)
            {
                case -720:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Dateline Standard Time");
                    break;
                case -660:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time");
                    break;
                case -600:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
                    break;
                case -540:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time");
                    break;
                case -480:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    break;
                case -420:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                    break;
                case -360:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    break;
                case -300:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    break;
                case -240:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
                    break;
                case -210:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
                    break;
                case -180:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                    break;
                case -120:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mid-Atlantic Standard Time");
                    break;
                case -60:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Cape Verde Standard Time");
                    break;
                case 0:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    break;
                case 60:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    break;
                case 120:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                    break;
                case 180:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                    break;
                case 210:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                    break;
                case 240:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                    break;
                case 270:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Afghanistan Standard Time");
                    break;
                case 300:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                    break;
                case 330:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    break;
                case 345:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
                    break;
                case 360:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time");
                    break;
                case 390:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
                    break;
                case 420:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    break;
                case 480:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    break;
                case 540:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    break;
                case 570:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Cen. Australia Standard Time");
                    break;
                case 600:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                    break;
                case 660:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Magadan Standard Time");
                    break;
                case 720:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                    break;
                case 780:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tonga Standard Time");
                    break;
                default:
                    foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
                    {
                        if (timeZone.BaseUtcOffset.TotalMinutes == timeZoneOffsetInMinutes)
                        {
                            timeZoneInfo = timeZone;
                            break;
                        }
                    }
                    break;
            }

            return timeZoneInfo;
        }


        public static string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback)
        {
            return TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(cultureCodes, fallback);
        }

        public static string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes)
        {
            return TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(cultureCodes);
        }

        #region GetExceptionMessage

        public static string GetExceptionMessage(string key, string defaultValue)
        {
            if (HttpContext.Current == null)
            {
                return defaultValue;
            }
            return GetString(key, ExceptionsResourceFile);
        }

        public static string GetExceptionMessage(string key, string defaultValue, params object[] @params)
        {
            if (HttpContext.Current == null)
            {
                return string.Format(defaultValue, @params);
            }
            var content = GetString(key, ExceptionsResourceFile);
            return string.Format(String.IsNullOrEmpty(content) ? defaultValue : GetString(key, ExceptionsResourceFile), @params);
        }

        #endregion

        public string GetFixedCurrency(decimal expression, string culture, int numDigitsAfterDecimal)
        {
            string oldCurrentCulture = CurrentUICulture;
            var newCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = newCulture;
            string currencyStr = expression.ToString(newCulture.NumberFormat.CurrencySymbol);
            var oldCulture = new CultureInfo(oldCurrentCulture);
            Thread.CurrentThread.CurrentUICulture = oldCulture;
            return currencyStr;
        }

        public string GetFixedDate(DateTime expression, string culture)
        {
            string oldCurrentCulture = CurrentUICulture;
            var newCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = newCulture;
            string dateStr = expression.ToString(newCulture.DateTimeFormat.FullDateTimePattern);
            var oldCulture = new CultureInfo(oldCurrentCulture);
            Thread.CurrentThread.CurrentUICulture = oldCulture;
            return dateStr;
        }

        #region Language detection
        /// <summary>
        /// Detects the current language for the request.
        /// The order in which the language is being detect is:
        ///		1. QueryString
        ///		2. Cookie
        ///		3. User profile (if request is authenticated)
        ///		4. Browser preference (if portal has this option enabled)
        ///		5. Portal default
        ///		6. System default (en-US)
        ///	At any point, if a valid language is detected nothing else should be done
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings</param>
        /// <returns>A valid CultureInfo</returns>
        public static CultureInfo GetPageLocale()
        {
            CultureInfo pageCulture = null;

            // 1. querystring
            pageCulture = GetCultureFromQs();

            // 2. cookie
            if ( pageCulture == null)
                pageCulture = GetCultureFromCookie();

            //// 3. user preference
            //if (pageCulture == null)
            //    pageCulture = GetCultureFromProfile();

            // 4. browser
            if (pageCulture == null)
                pageCulture = GetCultureFromBrowser();

            //// 5. portal default
            //if (pageCulture == null)
            //    pageCulture = GetCultureFromPortal();

            // 6. system default
            if (pageCulture == null)
                pageCulture = new CultureInfo(SystemLocale);

            // finally set the cookie
            SetLanguage(pageCulture.Name);
            return pageCulture;
        }

        /// <summary>
        /// Tries to get a valid language from the querystring
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings</param>
        /// <returns>A valid CultureInfo if any is found</returns>
        private static CultureInfo GetCultureFromQs()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request["language"] == null)
                return null;

            string language = HttpContext.Current.Request["language"];
            CultureInfo culture = GetCultureFromString(language);
            return culture;
        }

        /// <summary>
        /// Tries to get a valid language from the cookie
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings</param>
        /// <returns>A valid CultureInfo if any is found</returns>
        private static CultureInfo GetCultureFromCookie()
        {
            CultureInfo culture;
            if (HttpContext.Current == null || HttpContext.Current.Request.Cookies["language"] == null)
                return null;

            string language = HttpContext.Current.Request.Cookies["language"].Value;
            culture = GetCultureFromString(language);
            return culture;
        }


        /// <summary>
        /// Tries to get a valid language from the browser preferences if the portal has the setting 
        /// to use browser languages enabled.
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings</param>
        /// <returns>A valid CultureInfo if any is found</returns>
        private static CultureInfo GetCultureFromBrowser()
        {
            return GetBrowserCulture();
        }

        /// <summary>
        /// Tries to get a valid language from the portal default preferences
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings</param>
        /// <returns>A valid CultureInfo if any is found</returns>
        private static CultureInfo GetCultureFromPortal()
        {
            CultureInfo culture = new CultureInfo("en-US");
  
            //    // Get the first enabled locale on the portal
            //Dictionary<string, Locale> enabledLocales = new Dictionary<string, Locale>();
            //if (portalSettings.PortalId > Null.NullInteger)
            //    enabledLocales = LocaleController.Instance.GetLocales(portalSettings.PortalId);

            //if (enabledLocales.Count > 0)
            //{
            //    foreach (string localeCode in enabledLocales.Keys)
            //    {
            //        culture = new CultureInfo(localeCode);
            //        break;
            //    }
            //}

            return culture;
        }

        /// <summary>
        /// Tries to get a valid language from the browser preferences
        /// </summary>
        /// <param name="portalId">Id of the current portal</param>
        /// <returns>A valid CultureInfo if any is found</returns>
        public static CultureInfo GetBrowserCulture()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.UserLanguages == null)
                return null;

            CultureInfo culture = null;
            foreach (string userLang in HttpContext.Current.Request.UserLanguages)
            {
                //split userlanguage by ";"... all but the first language will contain a preferrence index eg. ;q=.5
                string language = userLang.Split(';')[0];
                culture = GetCultureFromString(language);
                if (culture != null)
                    break;
            }
            return culture;
        }

        /// <summary>
        /// Parses the language parameter into a valid and enabled language in the current portal.
        /// If an exact match is not found (language-region), it will try to find a match for the language only.
        /// Ex: requested locale is "en-GB", requested language is "en", enabled locale is "en-US", so "en" is a match for "en-US".
        /// </summary>
        /// <param name="portalId">Id of current portal</param>
        /// <param name="language">Language to be parsed</param>
        /// <returns>A valid and enabled CultureInfo that matches the language passed if any.</returns>
        private static CultureInfo GetCultureFromString(string language)
        {
            CultureInfo culture = new CultureInfo(language);
            //if (!String.IsNullOrEmpty(language))
            //{
            //    if (LocaleController.Instance.IsEnabled(ref language))
            //        culture = new CultureInfo(language);
            //    else
            //    {
            //        string preferredLanguage = language.Split('-')[0];

            //        Dictionary<string, Locale> enabledLocales = LocaleController.Instance.GetLocales();

            //        foreach (string localeCode in enabledLocales.Keys)
            //        {
            //            if (localeCode.Split('-')[0] == preferredLanguage.Split('-')[0])
            //            {
            //                culture = new CultureInfo(localeCode);
            //                break;
            //            }
            //        }
            //    }
            //}
            return culture;
        }
        #endregion

        public static string GetResourceFileName(string resourceFileName, string language)
        {
            if (!resourceFileName.EndsWith(".resx"))
            {
                resourceFileName += ".resx";
            }
            if (language != SystemLocale)
            {
                if (resourceFileName.ToLowerInvariant().EndsWith(".en-us.resx"))
                {
                    resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 11) + "." + language + ".resx";
                }
                else
                {
                    resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 5) + "." + language + ".resx";
                }
            }

            resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 5) + ".resx";
            return resourceFileName;
        }

        public static string GetResourceFile(Control control, string fileName)
        {
            return control.TemplateSourceDirectory + "/" + LocalResourceDirectory + "/" + fileName;
        }

        #region GetString

        //public static string GetString(string key, Control ctrl, string rsourceFile)
        //{
        //    //We need to find the parent module
        //    Control parentControl = ctrl.Parent;
        //    string localizedText;
        //    var moduleControl = parentControl as Control;
        //    if (moduleControl == null)
        //    {
        //        PropertyInfo pi = parentControl.GetType().GetProperty("LocalResourceFile");
        //        if (pi != null)
        //        {
        //            //If control has a LocalResourceFile property use this
        //            localizedText = GetString(key, pi.GetValue(parentControl, null).ToString());
        //        }
        //        else
        //        {
        //            //Drill up to the next level 
        //            localizedText = GetString(key, parentControl, rsourceFile);
        //        }
        //    }
        //    else
        //    {
        //        //We are at the Module Level so return key
        //        //Get Resource File Root from Parents LocalResourceFile Property
        //        localizedText = GetString(key, rsourceFile);
        //    }
        //    return localizedText;
        //}

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resource key
        /// </summary>
        /// <param name="key">The resource key to find</param>
        /// <returns>The localized Text</returns>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key)
        {
            return GetString(key, null,  null, false);
        }


        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="disableShowMissingKeys">Disable to show missing key.</param>
        /// <returns>The localized Text</returns>
        public static string GetString(string key, string resourceFileRoot, bool disableShowMissingKeys)
        {
            return GetString(key, resourceFileRoot, null, disableShowMissingKeys);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Resource File Name.</param>
        /// <returns>The localized Text</returns>
        public static string GetString(string key, string resourceFileRoot)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="language">A specific language to lookup the string</param>
        /// <returns>The localized Text</returns>
        //public static string GetString(string key, string resourceFileRoot, string language)
        //{
        //    return LocalizationProvider.Instance.GetString(key, resourceFileRoot, language);
        //}

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="portalSettings">The current portals Portal Settings</param>
        /// <param name="language">A specific language to lookup the string</param>
        /// <returns>The localized Text</returns>
        public static string GetString(string key, string resourceFileRoot,string language)
        {
            return GetString(key, resourceFileRoot, language, false);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="portalSettings">The current portals Portal Settings</param>
        /// <param name="language">A specific language to lookup the string</param>
        /// <param name="disableShowMissingKeys">Disables the show missing keys flag</param>
        /// <returns>The localized Text</returns>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, string resourceFileRoot, string language, bool disableShowMissingKeys)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot, language, disableShowMissingKeys);
        }

        #endregion

        #region GetStringUrl

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetStringUrl gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <returns>The localized Text</returns>
        /// <remarks>
        /// This function should be used to retrieve strings to be used on URLs.
        /// It is the same as <see cref="GetString(string, string)">GetString(name,ResourceFileRoot)</see> method
        /// but it disables the ShowMissingKey flag, so even it testing scenarios, the correct string
        /// is returned
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string GetStringUrl(string key, string resourceFileRoot)
        {
            return GetString(key, resourceFileRoot, null, true);
        }

        #endregion

        #region GetSafeJSString

        /// <summary>
        /// this function will escape reserved character fields to their "safe" javascript equivalents
        /// </summary>
        /// <param name="unsafeString">The string to be parsed for unsafe characters</param>
        /// <returns>the string that is safe to use in a javascript function</returns>
        public static string GetSafeJSString(string unsafeString)
        {
            return !string.IsNullOrEmpty(unsafeString) && unsafeString.Length > 0 ? Regex.Replace(unsafeString, "(['\"\\\\])", "\\$1") : unsafeString;
        }

        /// <summary>
        /// this function will escape reserved character fields to their "safe" javascript equivalents
        /// </summary>
        /// <param name="key">localization key</param>
        /// <param name="resourceFileRoot">file for localization key</param>
        /// <returns>the string that is safe to use in a javascript function</returns>
        public static string GetSafeJSString(string key, string resourceFileRoot)
        {
            var unsafeString = GetString(key, resourceFileRoot);
            return GetSafeJSString(unsafeString);
        }

        #endregion

     
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LocalizeDataGrid creates localized Headers for a DataGrid
        /// </summary>
        /// <param name="grid">Grid to localize</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        public static void LocalizeDataGrid(DataGrid grid, string resourceFile)
        {
            string localizedText;
            foreach (DataGridColumn col in grid.Columns)
            {
                //Localize Header Text
                if (!string.IsNullOrEmpty(col.HeaderText))
                {
                    localizedText = GetString(col.HeaderText + ".Header", resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        col.HeaderText = localizedText;
                    }
                }
                if (col is EditCommandColumn)
                {
                    var editCol = (EditCommandColumn)col;

                    //Edit Text - maintained for backward compatibility
                    localizedText = GetString(editCol.EditText + ".EditText", resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.EditText = localizedText;
                    }

                    //Edit Text
                    localizedText = GetString(editCol.EditText, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.EditText = localizedText;
                    }

                    //Cancel Text
                    localizedText = GetString(editCol.CancelText, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.CancelText = localizedText;
                    }

                    //Update Text
                    localizedText = GetString(editCol.UpdateText, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.UpdateText = localizedText;
                    }
                }
                else if (col is ButtonColumn)
                {
                    var buttonCol = (ButtonColumn)col;

                    //Edit Text
                    localizedText = GetString(buttonCol.Text, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        buttonCol.Text = localizedText;
                    }
                }
            }
        }

        /// <summary>
        /// Localizes headers and fields on a DetailsView control
        /// </summary>
        /// <param name="detailsView"></param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        public static void LocalizeDetailsView(DetailsView detailsView, string resourceFile)
        {
            foreach (DataControlField field in detailsView.Fields)
            {
                LocalizeDataControlField(field, resourceFile);
            }
        }

        /// <summary>
        /// Localizes headers and fields on a GridView control
        /// </summary>
        /// <param name="gridView">Grid to localize</param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        public static void LocalizeGridView(GridView gridView, string resourceFile)
        {
            foreach (DataControlField column in gridView.Columns)
            {
                LocalizeDataControlField(column, resourceFile);
            }
        }


        /// <summary>
        /// Localizes page
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        public static void LocalizePage(Page page, string resourceFile)
        {
            LocalizeControls(page, resourceFile);
        }
        
        /// <summary>
        /// Localizes DropDownList
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        private static void LocalizeDropDownList(DropDownList dropList, string resourceFile)
        {
            foreach (ListItem item in dropList.Items)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    string value = GetString(item.Text, resourceFile);
                    if (!string.IsNullOrEmpty(value))
                    {
                        item.Text = value;
                    }
                }  
            }
        }


        /// <summary>
        /// Localizes page
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        private static void LocalizeControls(Control parent, string resourceFile)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is GridView)
                {
                    LocalizeGridView((GridView)control, resourceFile);
                }
                else if (control is DataGrid)
                {
                    LocalizeDataGrid((DataGrid)control, resourceFile);
                }
                else if (control is DetailsView)
                {
                    LocalizeDetailsView((DetailsView)control, resourceFile);
                }
                else if (control is Label)
                {
                   
                    Label lable = (Label)control;
                    string value = GetString(lable.Text, resourceFile);
                    if (!string.IsNullOrEmpty(value))
                    {
                        lable.Text = GetString(lable.Text, resourceFile);
                    }  
                }
                else if (control is IButtonControl)
                {
                    IButtonControl button = (IButtonControl)control;
                    string value = GetString(button.Text, resourceFile);
                    if (!string.IsNullOrEmpty(value))
                    {
                        button.Text = value;
                    }
                }
                else if (control is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)control;
                    string value = GetString(checkBox.Text, resourceFile);
                    if (!string.IsNullOrEmpty(value))
                    {
                        checkBox.Text = value;
                    }
                
                }
                else if (control is DropDownList)
                {
                    LocalizeDropDownList((DropDownList)control, resourceFile);
                }

                LocalizeControls(control, resourceFile);
            }
        }


        public static void SetLanguage(string value)
        {
            try
            {
                HttpResponse response = HttpContext.Current.Response;
                if (response == null)
                {
                    return;
                }

                //save the page culture as a cookie
                HttpCookie cookie = response.Cookies.Get("language");
                if ((cookie == null))
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        cookie = new HttpCookie("language", value);
                        response.Cookies.Add(cookie);
                    }
                }
                else
                {
                    cookie.Value = value;
                    if (!String.IsNullOrEmpty(value))
                    {
                        response.Cookies.Set(cookie);
                    }
                    else
                    {
                        response.Cookies.Remove("language");
                    }
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        ///   Sets the culture codes on the current Thread
        /// </summary>
        /// <param name = "cultureInfo">Culture Info for the current page</param>
        /// <param name = "portalSettings">The current portalSettings</param>
        /// <remarks>
        ///   This method will configure the Thread culture codes.  Any page which does not derive from PageBase should
        ///   be sure to call this method in OnInit to ensure localiztion works correctly.  See the TelerikDialogHandler for an example.
        /// </remarks>
        public static void SetThreadCultures(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException("cultureInfo");
            }

            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        /// <summary>
        /// Maps the culture code string into the corresponding language ID in the
        /// database. In case there is no language defined in the systen with the
        /// passed code, -1 (<see cref="Null.NullInteger"/>) is returned.
        /// </summary>
        /// <param name="cultureCode">The culture to get the language ID for.</param>
        /// <returns>Language ID integer</returns>
        //public static int GetCultureLanguageID(string cultureCode)
        //{
        //    var locale = LocaleController.Instance.GetLocale(cultureCode);
        //    return locale != null ? locale.LanguageId : -1;
        //}


        public static string GetPageDefaultResource(Page page)
        {
            string language = SystemLocale;
            CultureInfo pageLocale = GetPageLocale();
            if (pageLocale != null)
            {
                language = pageLocale.Name;
            }

            string fileName = System.IO.Path.GetFileName(page.AppRelativeVirtualPath);
            return "/" + LocalResourceDirectory + "/" + fileName;
            //return page.TemplateSourceDirectory + "/" + LocalResourceDirectory + "/" + fileName;
            //return GetResourceFileName(pageName, language);
        }

        #endregion
    }
}