#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Wongs.Services.Localization;
using Wongs.Common.Utilities;

#endregion

namespace Wongs.Services.Cache
{
	/// <summary>
	/// CachingProvider provides basic component of cache system, by default it will use HttpRuntime.Cache.
	/// </summary>
	/// <remarks>
	/// <para>Using cache will speed up the application to a great degree, we recommend to use cache for whole modules,
	/// but sometimes cache also make confuse for user, if we didn't take care of how to make cache expired when needed,
	/// such as if a data has already been deleted but the cache arn't clear, it will cause un expected errors.
	/// so you should choose a correct performance setting type when you trying to cache some stuff, and always remember
	/// update cache immediately after the data changed.</para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// public static void ClearCache(string cachePrefix)
    /// {
    ///     CachingProvider.Instance().Clear("Prefix", GetDnnCacheKey(cachePrefix));
	/// }
	/// </code>
	/// </example>
    public abstract class CachingProvider
    {
		#region Private Members

        private static System.Web.Caching.Cache _cache;
        private const string CachePrefix = "DNN_";
		
		#endregion

		#region Protected Properties

		/// <summary>
		/// Gets the default cache provider.
		/// </summary>
		/// <value>HttpRuntime.Cache</value>
        protected static System.Web.Caching.Cache Cache
        {
            get
            {
                return _cache ?? (_cache = HttpRuntime.Cache);
            }
        }
		
		#endregion

		#region Shared/Static Methods

		/// <summary>
		/// Cleans the cache key by remove cache key prefix.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>cache key without prefix.</returns>
		/// <exception cref="ArgumentException">cache key is empty.</exception>
        public static string CleanCacheKey(string CacheKey)
        {
            if (String.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            return CacheKey.Substring(CachePrefix.Length);
        }

		/// <summary>
		/// Gets the cache key with key prefix.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>CachePrefix + CacheKey</returns>
		/// <exception cref="ArgumentException">Cache key is empty.</exception>
        public static string GetCacheKey(string CacheKey)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            return CachePrefix + CacheKey;
        }

		/// <summary>
		/// Instances of  caching provider.
		/// </summary>
		/// <returns>The Implemments provider of cache system defind in web.config.</returns>
        public static CachingProvider Instance()
        {
            if (_cachingProvider == null)
            {
                _cachingProvider = new FBCachingProvider();
            }
            return _cachingProvider;
            //return ComponentFactory.GetComponent<CachingProvider>();
        }

        private static CachingProvider _cachingProvider = null;
		
	#endregion

	    #region Private Methods
	
        private void ClearCacheInternal(string prefix, bool clearRuntime)
        {
            foreach (DictionaryEntry objDictionaryEntry in HttpRuntime.Cache)
            {
                if (Convert.ToString(objDictionaryEntry.Key).StartsWith(prefix))
                {
                    if (clearRuntime)
                    {
						//remove item from runtime cache
                        RemoveInternal(Convert.ToString(objDictionaryEntry.Key));
                    }
                    else
                    {
						//Call provider's remove method
                        Remove(Convert.ToString(objDictionaryEntry.Key));
                    }
                }
            }
        }

        //private void ClearCacheKeysByPortalInternal(int portalId, bool clearRuntime)
        //{
        //    RemoveFormattedCacheKey(DataCache.PortalCacheKey, clearRuntime, Null.NullInteger, string.Empty);
        //    RemoveFormattedCacheKey(DataCache.LocalesCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.ProfileDefinitionsCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.ListsCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.SkinsCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.PortalUserCountCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.PackagesCacheKey, clearRuntime, portalId);

        //    RemoveCacheKey(DataCache.AllPortalsCacheKey, clearRuntime);
        //}



        //private void ClearModulePermissionsCachesByPortalInternal(int portalId, bool clearRuntime)
        //{
        //    foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(portalId))
        //    {
        //        RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabPair.Value.TabID);
        //    }
        //}

        //private void ClearPortalCacheInternal(int portalId, bool cascade, bool clearRuntime)
        //{
        //    RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId);

        //    Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
        //    if (locales == null || locales.Count == 0)
        //    {
        //        //At least attempt to remove default locale
        //        string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
        //        RemoveCacheKey(String.Format(DataCache.PortalCacheKey, portalId, defaultLocale), clearRuntime);
        //    }
        //    else
        //    {
        //        foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
        //        {
        //            RemoveCacheKey(String.Format(DataCache.PortalCacheKey, portalId, portalLocale.Code), clearRuntime);
        //        }
        //    }
        //    if (cascade)
        //    {
        //        foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(portalId))
        //        {
        //            ClearModuleCacheInternal(tabPair.Value.TabID, clearRuntime);
        //        }
        //        foreach (ModuleInfo moduleInfo in ModuleController.Instance.GetModules(portalId))
        //        {
        //            RemoveCacheKey("GetModuleSettings" + moduleInfo.ModuleID, clearRuntime);
        //        }
        //    }
			
        //    //Clear "portal keys" for Portal
        //    ClearFolderCacheInternal(portalId, clearRuntime);
        //    ClearCacheKeysByPortalInternal(portalId, clearRuntime);
        //    ClearDesktopModuleCacheInternal(portalId, clearRuntime);
        //    ClearTabCacheInternal(portalId, clearRuntime);

        //    RemoveCacheKey(String.Format(DataCache.RolesCacheKey, portalId), clearRuntime);
        //    RemoveCacheKey(String.Format(DataCache.JournalTypesCacheKey, portalId), clearRuntime);
        //}

        //private void ClearTabCacheInternal(int portalId, bool clearRuntime)
        //{
        //    RemoveFormattedCacheKey(DataCache.TabCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.TabAliasSkinCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.TabCustomAliasCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.TabUrlCacheKey, clearRuntime, portalId);
        //    RemoveFormattedCacheKey(DataCache.TabPermissionCacheKey, clearRuntime, portalId);
        //    Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
        //    if (locales == null || locales.Count == 0)
        //    {
        //        //At least attempt to remove default locale
        //        string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
        //        RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, defaultLocale, portalId), clearRuntime);
        //    }
        //    else
        //    {
        //        foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
        //        {
        //            RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, portalLocale.Code, portalId), clearRuntime);
        //        }
        //    }

        //    RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, Null.NullString, portalId), clearRuntime);
        //    RemoveCacheKey(string.Format(DataCache.TabSettingsCacheKey, portalId), clearRuntime);
        //}

        private void RemoveCacheKey(string CacheKey, bool clearRuntime)
        {
            if (clearRuntime)
            {
				//remove item from runtime cache
                RemoveInternal(GetCacheKey(CacheKey));
            }
            else
            {
				//Call provider's remove method
                Remove(GetCacheKey(CacheKey));
            }
        }

        private void RemoveFormattedCacheKey(string CacheKeyBase, bool clearRuntime, params object[] parameters)
        {
            if (clearRuntime)
            {
				//remove item from runtime cache
                RemoveInternal(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
            else
            {
				//Call provider's remove method
                Remove(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
        }
		
		#endregion

		#region Protected Methods

	    /// <summary>
		/// Removes the internal.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
        protected void RemoveInternal(string cacheKey)
        {
			//attempt remove from private dictionary
            DataCache.RemoveFromPrivateDictionary(cacheKey);
            //remove item from memory
            if (Cache[cacheKey] != null)
            {
                Cache.Remove(cacheKey);
            }
        }

		/// <summary>
		/// Clears the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="data">The data.</param>
        public virtual void Clear(string type, string data)
        {
            //todo
            //ClearCacheInternal(type, data, false);
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <returns>cache content</returns>
        public virtual object GetItem(string cacheKey)
        {
            return Cache[cacheKey];
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <param name="itemToCache">The object.</param>
        public virtual void Insert(string cacheKey, object itemToCache)
        {
            Insert(cacheKey, itemToCache, null as DNNCacheDependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <param name="itemToCache">The object.</param>
		/// <param name="dependency">The dependency.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency)
        {
            Insert(cacheKey, itemToCache, dependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <param name="itemToCache">The object.</param>
		/// <param name="dependency">The dependency.</param>
		/// <param name="absoluteExpiration">The absolute expiration.</param>
		/// <param name="slidingExpiration">The sliding expiration.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            Insert(cacheKey, itemToCache, dependency, absoluteExpiration, slidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <param name="itemToCache">The value.</param>
		/// <param name="dependency">The dependency.</param>
		/// <param name="absoluteExpiration">The absolute expiration.</param>
		/// <param name="slidingExpiration">The sliding expiration.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="onRemoveCallback">The on remove callback.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority,
                                   CacheItemRemovedCallback onRemoveCallback)
		{
		    Cache.Insert(cacheKey, itemToCache, dependency == null ? null : dependency.SystemCacheDependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
		}

	    /// <summary>
		/// Determines whether is web farm.
		/// </summary>
		/// <returns>
		///   <c>true</c> if is web farm; otherwise, <c>false</c>.
		/// </returns>
        public virtual bool IsWebFarm()
        {
            return true; //todo
            //return (ServerController.GetEnabledServers().Count > 1);
        }

		/// <summary>
		/// Purges the cache.
		/// </summary>
		/// <returns></returns>
        public virtual string PurgeCache()
        {
            return Localization.Localization.GetString("PurgeCacheUnsupported.Text", Localization.Localization.GlobalResourceFile);
        }

		/// <summary>
		/// Removes the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
        public virtual void Remove(string CacheKey)
        {
            RemoveInternal(CacheKey);
        }
		
		#endregion
    }
}