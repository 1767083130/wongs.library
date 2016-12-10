#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.Caching;
using Wongs.Collections.Internal;
using Wongs.Services.Cache;
using Wongs.Services.Log;

#endregion

namespace Wongs.Common.Utilities
{
    public enum CoreCacheType
    {
        Host = 1,
        Portal = 2,
        Tab = 3
    }

    /// -----------------------------------------------------------------------------
    /// Project:    Wongs
    /// Namespace:  Wongs.Common.Utilities
    /// Class:      DataCache
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DataCache class is a facade class for the CachingProvider Instance's
    /// </summary>
    public class DataCache
    {

        public const CacheItemPriority ResourceFilesCachePriority = CacheItemPriority.Normal;
        public const int ResourceFilesCacheTimeOut = 20;

        public const string ResourceFileLookupDictionaryCacheKey = "ResourceFileLookupDictionary";
        public const CacheItemPriority ResourceFileLookupDictionaryCachePriority = CacheItemPriority.NotRemovable;
        public const int ResourceFileLookupDictionaryTimeOut = 200;

        private static string _CachePersistenceEnabled = "";

        private static readonly ReaderWriterLock dictionaryLock = new ReaderWriterLock();
        private static readonly Dictionary<string, object> lockDictionary = new Dictionary<string, object>();

        private static readonly SharedDictionary<string, Object> dictionaryCache = new SharedDictionary<string, Object>();

        public static bool CachePersistenceEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(_CachePersistenceEnabled))
                {
                    //todo
                    _CachePersistenceEnabled = "false";
                    //_CachePersistenceEnabled = Config.GetSetting("EnableCachePersistence") ?? "false";
                }
                return bool.Parse(_CachePersistenceEnabled);
            }
        }

        private static string GetDnnCacheKey(string CacheKey)
        {
            return CachingProvider.GetCacheKey(CacheKey);
        }

        private static string CleanCacheKey(string CacheKey)
        {
            return CachingProvider.CleanCacheKey(CacheKey);
        }

        internal static void ItemRemovedCallback(string key, object value, CacheItemRemovedReason removedReason)
        {
            ////if the item was removed from the cache, log the key and reason to the event log
            //try
            //{
            //    if (Globals.Status == Globals.UpgradeStatus.None)
            //    {
            //        var log = new LogInfo();
            //        switch (removedReason)
            //        {
            //            case CacheItemRemovedReason.Removed:
            //                log.LogTypeKey = EventLogController.EventLogType.CACHE_REMOVED.ToString();
            //                break;
            //            case CacheItemRemovedReason.Expired:
            //                log.LogTypeKey = EventLogController.EventLogType.CACHE_EXPIRED.ToString();
            //                break;
            //            case CacheItemRemovedReason.Underused:
            //                log.LogTypeKey = EventLogController.EventLogType.CACHE_UNDERUSED.ToString();
            //                break;
            //            case CacheItemRemovedReason.DependencyChanged:
            //                log.LogTypeKey = EventLogController.EventLogType.CACHE_DEPENDENCYCHANGED.ToString();
            //                break;
            //        }
            //        log.LogProperties.Add(new LogDetailInfo(key, removedReason.ToString()));
            //        LogController.Instance.AddLog(log);
            //    }
            //}
            //catch (Exception exc)
            //{
            //    //Swallow exception            
            //    Logger.Error(exc);
            //}
        }

        public static void ClearCache()
        {
            CachingProvider.Instance().Clear("Prefix", "DNN_");
            using (ISharedCollectionLock writeLock = dictionaryCache.GetWriteLock())
            {
                dictionaryCache.Clear();
            }

            //todo
            ////log the cache clear event
            //var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.CACHE_REFRESH.ToString()};
            //log.LogProperties.Add(new LogDetailInfo("*", "Refresh"));
            //LogController.Instance.AddLog(log);
        }

        public static void ClearCache(string cachePrefix)
        {
            CachingProvider.Instance().Clear("Prefix", GetDnnCacheKey(cachePrefix));
        }

        public static void ClearFolderCache(int PortalId)
        {
            CachingProvider.Instance().Clear("Folder", PortalId.ToString());
        }

        public static void ClearHostCache(bool Cascade)
        {
            if (Cascade)
            {
                ClearCache();
            }
            else
            {
                CachingProvider.Instance().Clear("Host", "");
            }
        }

        public static void ClearModulePermissionsCachesByPortal(int PortalId)
        {
            CachingProvider.Instance().Clear("ModulePermissionsByPortal", PortalId.ToString());
        }

        public static void ClearPortalCache(int PortalId, bool Cascade)
        {
            CachingProvider.Instance().Clear(Cascade ? "PortalCascade" : "Portal", PortalId.ToString());
        }

        private static object GetCachedDataFromRuntimeCache(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
            object objObject = GetCache(cacheItemArgs.CacheKey);

            // if item is not cached
            if (objObject == null)
            {
                //Get Unique Lock for cacheKey
                object @lock = GetUniqueLockObject(cacheItemArgs.CacheKey);

                // prevent other threads from entering this block while we regenerate the cache
                lock (@lock)
                {
                    // try to retrieve object from the cache again (in case another thread loaded the object since we first checked)
                    objObject = GetCache(cacheItemArgs.CacheKey);

                    // if object was still not retrieved

                    if (objObject == null)
                    {
                        // get object from data source using delegate
                        try
                        {
                            objObject = cacheItemExpired(cacheItemArgs);
                        }
                        catch (Exception ex)
                        {
                            objObject = null;
                            Exceptions.LogException(ex);
                        }

                        // set cache timeout
                        int timeOut = cacheItemArgs.CacheTimeOut * 10000; //todo //Convert.ToInt32(Host.PerformanceSetting);

                        // if we retrieved a valid object and we are using caching
                        if (objObject != null && timeOut > 0)
                        {
                            // save the object in the cache
                            SetCache(cacheItemArgs.CacheKey,
                                     objObject,
                                     cacheItemArgs.CacheDependency,
                                     Cache.NoAbsoluteExpiration,
                                     TimeSpan.FromMinutes(timeOut),
                                     cacheItemArgs.CachePriority,
                                     cacheItemArgs.CacheCallback);

                            // check if the item was actually saved in the cache

                            if (GetCache(cacheItemArgs.CacheKey) == null)
                            {
                                //todo
                                // log the event if the item was not saved in the cache ( likely because we are out of memory )
                                //var log = new LogInfo{ LogTypeKey = EventLogController.EventLogType.CACHE_OVERFLOW.ToString() };
                                //log.LogProperties.Add(new LogDetailInfo(cacheItemArgs.CacheKey, "Overflow - Item Not Cached"));
                                //LogController.Instance.AddLog(log);
                            }
                        }

                        //This thread won so remove unique Lock from collection
                        RemoveUniqueLockObject(cacheItemArgs.CacheKey);
                    }
                }
            }

            return objObject;
        }

        private static object GetCachedDataFromDictionary(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
            object cachedObject;

            bool isFound;
            using (ISharedCollectionLock readLock = dictionaryCache.GetReadLock())
            {
                isFound = dictionaryCache.TryGetValue(cacheItemArgs.CacheKey, out cachedObject);
            }

            if (!isFound)
            {
                // get object from data source using delegate
                try
                {
                    cachedObject = cacheItemExpired != null ? cacheItemExpired(cacheItemArgs) : null;
                }
                catch (Exception ex)
                {
                    cachedObject = null;
                    Exceptions.LogException(ex);
                }

                using (ISharedCollectionLock writeLock = dictionaryCache.GetWriteLock())
                {
                    if (!dictionaryCache.ContainsKey(cacheItemArgs.CacheKey))
                    {
                        if (cachedObject != null)
                        {
                            dictionaryCache[cacheItemArgs.CacheKey] = cachedObject;
                        }
                    }
                }
            }

            return cachedObject;
        }

        public static TObject GetCachedData<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
            // declare local object and try and retrieve item from the cache
            return GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, false);
        }

        internal static TObject GetCachedData<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool storeInDictionary)
        {
            object objObject = storeInDictionary 
                                   ? GetCachedDataFromDictionary(cacheItemArgs, cacheItemExpired) 
                                   : GetCachedDataFromRuntimeCache(cacheItemArgs, cacheItemExpired);

            // return the object
            if (objObject == null)
            {
                return default(TObject);
            }
            return (TObject)objObject;
        }

        private static object GetUniqueLockObject(string key)
        {
            object @lock = null;
            dictionaryLock.AcquireReaderLock(new TimeSpan(0, 0, 5));
            try
            {
                //Try to get lock Object (for key) from Dictionary
                if (lockDictionary.ContainsKey(key))
                {
                    @lock = lockDictionary[key];
                }
            }
            finally
            {
                dictionaryLock.ReleaseReaderLock();
            }
            if (@lock == null)
            {
                dictionaryLock.AcquireWriterLock(new TimeSpan(0, 0, 5));
                try
                {
                    //Double check dictionary
                    if (!lockDictionary.ContainsKey(key))
                    {
                        //Create new lock
                        lockDictionary[key] = new object();
                    }
                    //Retrieve lock
                    @lock = lockDictionary[key];
                }
                finally
                {
                    dictionaryLock.ReleaseWriterLock();
                }
            }
            return @lock;
        }

        private static void RemoveUniqueLockObject(string key)
        {
            dictionaryLock.AcquireWriterLock(new TimeSpan(0, 0, 5));
            try
            {
                //check dictionary
                if (lockDictionary.ContainsKey(key))
                {
                    //Remove lock
                    lockDictionary.Remove(key);
                }
            }
            finally
            {
                dictionaryLock.ReleaseWriterLock();
            }
        }

        public static TObject GetCache<TObject>(string CacheKey)
        {
            object objObject = GetCache(CacheKey);
            if (objObject == null)
            {
                return default(TObject);
            }
            return (TObject)objObject;
        }

        public static object GetCache(string CacheKey)
        {
            return CachingProvider.Instance().GetItem(GetDnnCacheKey(CacheKey));
        }

        public static void RemoveCache(string CacheKey)
        {
            CachingProvider.Instance().Remove(GetDnnCacheKey(CacheKey));
        }

        public static void RemoveFromPrivateDictionary(string DnnCacheKey)
        {
            using (ISharedCollectionLock writeLock = dictionaryCache.GetWriteLock())
            {
                dictionaryCache.Remove(CleanCacheKey(DnnCacheKey));
            }
        }

        public static void SetCache(string CacheKey, object objObject)
        {
            DNNCacheDependency objDependency = null;
            SetCache(CacheKey, objObject, objDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static void SetCache(string CacheKey, object objObject, DNNCacheDependency objDependency)
        {
            SetCache(CacheKey, objObject, objDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static void SetCache(string CacheKey, object objObject, DateTime AbsoluteExpiration)
        {
            DNNCacheDependency objDependency = null;
            SetCache(CacheKey, objObject, objDependency, AbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static void SetCache(string CacheKey, object objObject, TimeSpan SlidingExpiration)
        {
            DNNCacheDependency objDependency = null;
            SetCache(CacheKey, objObject, objDependency, Cache.NoAbsoluteExpiration, SlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static void SetCache(string CacheKey, object objObject, DNNCacheDependency objDependency, DateTime AbsoluteExpiration, TimeSpan SlidingExpiration)
        {
            SetCache(CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static void SetCache(string CacheKey, object objObject, DNNCacheDependency objDependency, DateTime AbsoluteExpiration, TimeSpan SlidingExpiration, CacheItemPriority Priority,
                                    CacheItemRemovedCallback OnRemoveCallback)
        {
            if (objObject != null)
            {
                //if no OnRemoveCallback value is specified, use the default method
                if (OnRemoveCallback == null)
                {
                    OnRemoveCallback = ItemRemovedCallback;
                }
                CachingProvider.Instance().Insert(GetDnnCacheKey(CacheKey), objObject, objDependency, AbsoluteExpiration, SlidingExpiration, Priority, OnRemoveCallback);
            }
        }


        #region "GetCachedObject"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCachedObject gets an object from the Cache
        /// </summary>
        /// <typeparam name="TObject">The type of th object to fetch</typeparam>
        /// <param name="cacheItemArgs">A CacheItemArgs object that provides parameters to manage the
        /// cache AND to fetch the item if the cache has expired</param>
        /// <param name="cacheItemExpired">A CacheItemExpiredCallback delegate that is used to repopulate
        /// the cache if the item has expired</param>
        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired);
        }

        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, saveInDictionary);
        }

        #endregion
    }
}