using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using System.Xml;
using System.Xml.XPath;

using Wongs.Collections.Internal;
using Wongs.Common;
using Wongs.Common.Utilities;
using Wongs.Services.Cache;
using Wongs.Services.Log;
using System.Globalization;


namespace Wongs.Services.Localization
{
    public class LocalizationProvider : ILocalizationProvider
    {
        private static readonly ILog Logger = new Logger();


        private static ILocalizationProvider _instance;
        public static ILocalizationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationProvider();
                }
                return _instance;
            }
        }

        #region Nested type: CustomizedLocale

        public enum CustomizedLocale
        {
            None = 0,
            Portal = 1,
            Host = 2
        }

        #endregion

        #region Implementation of ILocalizationProvider

        public string GetString(string key, string resourceFileRoot)
        {
            return GetString(key, resourceFileRoot, null, false);
        }

        public string GetString(string key, string resourceFileRoot, string language)
        {
            return GetString(key, resourceFileRoot, language,  false);
        }

        //public string GetString(string key, string resourceFileRoot, string language)
        //{
        //    return GetString(key, resourceFileRoot, language, false);
        //}

        public string GetString(string key, string resourceFileRoot, string language, bool disableShowMissingKeys)
        {
            //make the default translation property ".Text"
            if (key.IndexOf(".", StringComparison.Ordinal) < 1)
            {
                key += ".Text";
            }
            string resourceValue = string.Empty;
            bool keyFound = TryGetStringInternal(key, language, resourceFileRoot, ref resourceValue);

            //If the key can't be found then it doesn't exist in the Localization Resources
            if (Localization.ShowMissingKeys && !disableShowMissingKeys)
            {
                if (keyFound)
                {
                    resourceValue = "[L]" + resourceValue;
                }
                else
                {
                    resourceValue = "RESX:" + key;
                }
            }

            if (!keyFound)
            {
                Logger.WarnFormat("Missing localization key. key:{0} resFileRoot:{1} threadCulture:{2} userlan:{3}", key, resourceFileRoot, Thread.CurrentThread.CurrentUICulture, language);
            }

            return resourceValue;
        }

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
        public bool SaveString(string key, string value, string resourceFileRoot, string language, CustomizedLocale resourceType, bool createFile, bool createKey)
        {
            try
            {
                if (key.IndexOf(".", StringComparison.Ordinal) < 1)
                {
                    key += ".Text";
                }
                string resourceFileName = GetResourceFileName(resourceFileRoot, language);
                resourceFileName = resourceFileName.Replace("." + language.ToLower() + ".", "." + language + ".");
                //switch (resourceType)
                //{
                //    case CustomizedLocale.Host:
                //        resourceFileName = resourceFileName.Replace(".resx", ".Host.resx");
                //        break;
                //    case CustomizedLocale.Portal:
                //        resourceFileName = resourceFileName.Replace(".resx", ".Portal-" + portalSettings.PortalId + ".resx");
                //        break;
                //}
                resourceFileName = resourceFileName.TrimStart('~', '/', '\\');
                string filePath = HostingEnvironment.MapPath("~/" + Globals.ApplicationPath + resourceFileName);
                XmlDocument doc = null;
                if (File.Exists(filePath))
                {
                    doc = new XmlDocument();
                    doc.Load(filePath);
                }
                else
                {
                    if (createFile)
                    {
                        doc = new System.Xml.XmlDocument();
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                        XmlNode root = doc.CreateElement("root");
                        doc.AppendChild(root);
                        AddResourceFileNode(ref root, "resheader", "resmimetype", "text/microsoft-resx");
                        AddResourceFileNode(ref root, "resheader", "version", "2.0");
                        AddResourceFileNode(ref root, "resheader", "reader", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                        AddResourceFileNode(ref root, "resheader", "writer", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    }
                }
                if (doc == null) { return false; }
                XmlNode reskeyNode = doc.SelectSingleNode("root/data[@name=\"" + key + "\"]");
                if (reskeyNode != null)
                {
                    reskeyNode.SelectSingleNode("value").InnerText = value;
                }
                else
                {
                    if (createKey)
                    {
                        XmlNode root = doc.SelectSingleNode("root");
                        AddResourceFileNode(ref root, "data", key, value);
                    }
                    else
                    {
                        return false;
                    }
                }
                doc.Save(filePath);
                DataCache.RemoveCache("/" + resourceFileName.ToLower());
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while trying to create resource in {0}", resourceFileRoot), ex);
            }
        }

        #endregion

        /// <summary>
        /// Adds one of either a "resheader" or "data" element to resxRoot (which should be the root element of the resx file). 
        /// This function is used to construct new resource files and to add resource keys to an existing file.
        /// </summary>
        /// <param name="resxRoot">The RESX root.</param>
        /// <param name="elementName">Name of the element ("resheader" or "data").</param>
        /// <param name="nodeName">Name of the node (in case of "data" specify the localization key here, e.g. "MyWidget.Text").</param>
        /// <param name="nodeValue">The node value (text value to use).</param>
        private static void AddResourceFileNode(ref XmlNode resxRoot, string elementName, string nodeName, string nodeValue)
        {
            XmlNode newNode = resxRoot.AddElement(elementName, "").AddAttribute("name", nodeName);
            if (elementName == "data")
            {
                newNode = newNode.AddAttribute("xml:space", "preserve");
            }
            newNode.AddElement("value", nodeValue);
        }

        private static object GetResourceFileCallBack(CacheItemArgs cacheItemArgs)
        {
            string cacheKey = cacheItemArgs.CacheKey;
            Dictionary<string, string> resources = null;

            string filePath = null;
            try
            {
                //Get resource file lookup to determine if the resource file even exists
                SharedDictionary<string, bool> resourceFileExistsLookup = GetResourceFileLookupDictionary();

                if (ResourceFileMayExist(resourceFileExistsLookup, cacheKey))
                {
                    //check if an absolute reference for the resource file was used
                    if (cacheKey.Contains(":\\") && Path.IsPathRooted(cacheKey))
                    {
                        //if an absolute reference, check that the file exists
                        if (File.Exists(cacheKey))
                        {
                            filePath = cacheKey;
                        }
                    }

                    //no filepath found from an absolute reference, try and map the path to get the file path
                    if (filePath == null)
                    {
                        filePath = HostingEnvironment.MapPath(Globals.ApplicationPath + cacheKey);
                    }

                    //The file is not in the lookup, or we know it exists as we have found it before
                    if (File.Exists(filePath))
                    {
                        if (filePath != null)
                        {
                            var doc = new XPathDocument(filePath);
                            resources = new Dictionary<string, string>();
                            foreach (XPathNavigator nav in doc.CreateNavigator().Select("root/data"))
                            {
                                if (nav.NodeType != XPathNodeType.Comment)
                                {
                                    var selectSingleNode = nav.SelectSingleNode("value");
                                    if (selectSingleNode != null)
                                    {
                                        resources[nav.GetAttribute("name", String.Empty)] = selectSingleNode.Value;
                                    }
                                }
                            }
                        }
                        cacheItemArgs.CacheDependency = new DNNCacheDependency(filePath);

                        //File exists so add it to lookup with value true, so we are safe to try again
                        using (resourceFileExistsLookup.GetWriteLock())
                        {
                            resourceFileExistsLookup[cacheKey] = true;
                        }
                    }
                    else
                    {
                        //File does not exist so add it to lookup with value false, so we don't try again
                        using (resourceFileExistsLookup.GetWriteLock())
                        {
                            resourceFileExistsLookup[cacheKey] = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The following resource file caused an error while reading: {0}", filePath), ex);
            }
            return resources;
        }

        private static SharedDictionary<string, bool> GetResourceFileLookupDictionary()
        {
            return
                DataCache.GetCachedObject<SharedDictionary<string, bool>>(
                    new CacheItemArgs(DataCache.ResourceFileLookupDictionaryCacheKey, DataCache.ResourceFileLookupDictionaryTimeOut, DataCache.ResourceFileLookupDictionaryCachePriority),
                    c => new SharedDictionary<string, bool>(),
                    true);
        }

        private static Dictionary<string, string> GetResourceFile(string resourceFile)
        {
            return DataCache.GetCachedObject<Dictionary<string, string>>(new CacheItemArgs(resourceFile, DataCache.ResourceFilesCacheTimeOut, DataCache.ResourceFilesCachePriority),
                                                                   GetResourceFileCallBack,
                                                                   true);
        }

        private static string GetResourceFileName(string resourceFileRoot, string language)
        {
            string resourceFile;
            language = language.ToLower();
            if (resourceFileRoot != null)
            {
                if (language == Localization.SystemLocale.ToLower() || String.IsNullOrEmpty(language))
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5, 5).ToLower())
                    {
                        case ".resx":
                            resourceFile = resourceFileRoot;
                            break;
                        case ".ascx":
                            resourceFile = resourceFileRoot + ".resx";
                            break;
                        case ".aspx":
                            resourceFile = resourceFileRoot + ".resx";
                            break;
                        default:
                            resourceFile = resourceFileRoot + ".ascx.resx"; //a portal module
                            break;
                    }
                }
                else
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5, 5).ToLower())
                    {
                        case ".resx":
                            resourceFile = resourceFileRoot.Replace(".resx", "." + language + ".resx");
                            break;
                        case ".ascx":
                            resourceFile = resourceFileRoot.Replace(".ascx", ".ascx." + language + ".resx");
                            break;
                        case ".aspx":
                            resourceFile = resourceFileRoot.Replace(".aspx", ".aspx." + language + ".resx");
                            break;
                        default:
                            resourceFile = resourceFileRoot + ".ascx." + language + ".resx";
                            break;
                    }
                }
            }
            else
            {
                if (language == Localization.SystemLocale.ToLower() || String.IsNullOrEmpty(language))
                {
                    resourceFile = Localization.SharedResourceFile;
                }
                else
                {
                    resourceFile = Localization.SharedResourceFile.Replace(".resx", "." + language + ".resx");
                }
            }
            return resourceFile;
        }

        private static bool ResourceFileMayExist(SharedDictionary<string, bool> resourceFileExistsLookup, string cacheKey)
        {
            bool mayExist;
            using (resourceFileExistsLookup.GetReadLock())
            {
                mayExist = !resourceFileExistsLookup.ContainsKey(cacheKey) || resourceFileExistsLookup[cacheKey];
            }
            return mayExist;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, string userLanguage, string fallbackLanguage, string defaultLanguage, ref string resourceValue)
        {
            //Try the user's language first
            bool bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, userLanguage), ref resourceValue);

            if (!bFound && fallbackLanguage != userLanguage)
            {
                //Try fallback language next
                bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, fallbackLanguage), ref resourceValue);
            }
            if (!bFound && !(defaultLanguage == userLanguage || defaultLanguage == fallbackLanguage))
            {
                //Try default Language last
                bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, defaultLanguage), ref resourceValue);
            }
            return bFound;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, ref string resourceValue)
        {
            //Try Portal Resource File
            bool bFound = TryGetFromResourceFile(key, resourceFile, CustomizedLocale.Portal, ref resourceValue);
            if (!bFound)
            {
                //Try Host Resource File
                bFound = TryGetFromResourceFile(key, resourceFile, CustomizedLocale.Host, ref resourceValue);
            }
            if (!bFound)
            {
                //Try Portal Resource File
                bFound = TryGetFromResourceFile(key, resourceFile, CustomizedLocale.None, ref resourceValue);
            }
            return bFound;
        }

        private static bool TryGetStringInternal(string key, string userLanguage, string resourceFile, ref string resourceValue)
        {
            string defaultLanguage = "en-US"; //todo
            string fallbackLanguage = Localization.SystemLocale;

            if (String.IsNullOrEmpty(userLanguage))
            {
                CultureInfo pageLocale = Localization.GetPageLocale();
                userLanguage = pageLocale.Name;
            }

            //Set the userLanguage if not passed in //todo Thread.CurrentThread.CurrentUICulture始终为中文
            if (String.IsNullOrEmpty(userLanguage)) 
            {
                userLanguage = Thread.CurrentThread.CurrentUICulture.ToString();
            }

            //Default the userLanguage to the defaultLanguage if not set
            if (String.IsNullOrEmpty(userLanguage))
            {
                userLanguage = defaultLanguage;
            }
            Locale userLocale = null;
            //try
            //{
            //    if (Globals.Status != Globals.UpgradeStatus.Install)
            //    {
            //        //Get Fallback language, but not when we are installing (because we may not have a database yet)
            //        userLocale = LocaleController.Instance.GetLocale(userLanguage);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error(ex);
            //}

            if (userLocale != null && !String.IsNullOrEmpty(userLocale.Fallback))
            {
                fallbackLanguage = userLocale.Fallback;
            }
            if (String.IsNullOrEmpty(resourceFile))
            {
                resourceFile = Localization.SharedResourceFile;
            }

            //Try the resource file for the key
            bool bFound = TryGetFromResourceFile(key, resourceFile, userLanguage, fallbackLanguage, defaultLanguage, ref resourceValue);
            if (!bFound)
            {
                if (Localization.SharedResourceFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                {
                    //try to use a module specific shared resource file
                    string localSharedFile = resourceFile.Substring(0, resourceFile.LastIndexOf("/", StringComparison.Ordinal) + 1) + Localization.LocalSharedResourceFile;

                    if (localSharedFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                    {
                        bFound = TryGetFromResourceFile(key, localSharedFile, userLanguage, fallbackLanguage, defaultLanguage, ref resourceValue);
                    }
                }
            }
            if (!bFound)
            {
                if (Localization.SharedResourceFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                {
                    bFound = TryGetFromResourceFile(key, Localization.SharedResourceFile, userLanguage, fallbackLanguage, defaultLanguage, ref resourceValue);
                }
            }
            return bFound;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, CustomizedLocale resourceType, ref string resourceValue)
        {
            bool bFound = false;
            string resourceFileName = resourceFile;
            //switch (resourceType)
            //{
            //    case CustomizedLocale.Host:
            //        resourceFileName = resourceFile.Replace(".resx", ".Host.resx");
            //        break;
            //    case CustomizedLocale.Portal:
            //        resourceFileName = resourceFile.Replace(".resx", ".Portal-" + portalID + ".resx");
            //        break;
            //}

            if (resourceFileName.StartsWith("desktopmodules", StringComparison.InvariantCultureIgnoreCase)
                || resourceFileName.StartsWith("admin", StringComparison.InvariantCultureIgnoreCase)
                || resourceFileName.StartsWith("controls", StringComparison.InvariantCultureIgnoreCase))
            {
                resourceFileName = "~/" + resourceFileName;
            }

            //Local resource files are either named ~/... or <ApplicationPath>/...
            //The following logic creates a cachekey of /....
            string cacheKey = resourceFileName.Replace("~/", "/").ToLowerInvariant();
            if (!String.IsNullOrEmpty(Globals.ApplicationPath))
            {
                if (Globals.ApplicationPath != "/portals")
                {
                    if (cacheKey.StartsWith(Globals.ApplicationPath))
                    {
                        cacheKey = cacheKey.Substring(Globals.ApplicationPath.Length);
                    }
                }
                else
                {
                    cacheKey = "~" + cacheKey;
                    if (cacheKey.StartsWith("~" + Globals.ApplicationPath))
                    {
                        cacheKey = cacheKey.Substring(Globals.ApplicationPath.Length + 1);
                    }
                }
            }

            //Get resource file lookup to determine if the resource file even exists
            SharedDictionary<string, bool> resourceFileExistsLookup = GetResourceFileLookupDictionary();

            if (ResourceFileMayExist(resourceFileExistsLookup, cacheKey))
            {
                //File is not in lookup or its value is true so we know it exists
                Dictionary<string, string> dicResources = GetResourceFile(cacheKey);
                if (dicResources != null)
                {
                    bFound = dicResources.TryGetValue(key, out resourceValue);
                }
            }

            return bFound;
        }
    }
}