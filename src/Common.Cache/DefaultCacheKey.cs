// -----------------------------------------------------------------------
// <copyright file="DefaultCacheKey.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    public static class DefaultCacheKey
    {
        public static string GetListCacheKey<T>() where T : class
        {
            return $"{typeof(T).Name}/list";
        }

        public static string GetItemCacheKey<T>(string id) where T : class
        {
            return $"{typeof(T).Name}/{id.TrimStart('/')}";
        }
    }
}