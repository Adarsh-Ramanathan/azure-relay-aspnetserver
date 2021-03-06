﻿
namespace Microsoft.Azure.Relay.AspNetCore
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A collection or URL prefixes
    /// </summary>
    public class UrlPrefixCollection : ICollection<UrlPrefix>
    {
        private readonly IDictionary<int, UrlPrefix> _prefixes = new Dictionary<int, UrlPrefix>(1);
        private UrlGroup _urlGroup;
        private int _nextId = 1;

        internal UrlPrefixCollection()
        {
        }

        public int Count
        {
            get
            {
                lock (_prefixes)
                {
                    return _prefixes.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(string connectionString, string path = null, TokenProvider tokenProvider = null)
        {
            Add(UrlPrefix.Create(connectionString, null));
        }

        public void Add(string prefix, TokenProvider tokenProvider)
        {
            Add(UrlPrefix.Create(prefix, tokenProvider));
        }

        public void Add(UrlPrefix item)
        {
            lock (_prefixes)
            {
                var id = _nextId++;
                if (_urlGroup != null)
                {
                    _urlGroup.RegisterPrefix(item.FullPrefix, id);
                }
                _prefixes.Add(id, item);
            }
        }

        internal UrlPrefix GetPrefix(int id)
        {
            lock (_prefixes)
            {
                return _prefixes[id];
            }
        }

        public void Clear()
        {
            lock (_prefixes)
            {
                if (_urlGroup != null)
                {
                    UnregisterAllPrefixes();
                }
                _prefixes.Clear();
            }
        }

        public bool Contains(UrlPrefix item)
        {
            lock (_prefixes)
            {
                return _prefixes.Values.Contains(item);
            }
        }

        public void CopyTo(UrlPrefix[] array, int arrayIndex)
        {
            lock (_prefixes)
            {
                _prefixes.Values.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(string prefix)
        {
            return Remove(UrlPrefix.Create(prefix, null));
        }

        public bool Remove(UrlPrefix item)
        {
            lock (_prefixes)
            {
                int? id = null;
                foreach (var pair in _prefixes)
                {
                    if (pair.Value.Equals(item))
                    {
                        id = pair.Key;
                        if (_urlGroup != null)
                        {
                            _urlGroup.UnregisterPrefix(pair.Value.FullPrefix);
                        }
                    }
                }
                if (id.HasValue)
                {
                    _prefixes.Remove(id.Value);
                    return true;
                }
                return false;
            }
        }

        public IEnumerator<UrlPrefix> GetEnumerator()
        {
            lock (_prefixes)
            {
                return _prefixes.Values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // TODO: CR: This appears unused (does ASP.NET call via reflection or can it be removed?)
        internal void RegisterAllPrefixes(UrlGroup urlGroup)
        {
            lock (_prefixes)
            {
                _urlGroup = urlGroup;
                // go through the uri list and register for each one of them
                foreach (var pair in _prefixes)
                {
                    // We'll get this index back on each request and use it to look up the prefix to calculate PathBase.
                    _urlGroup.RegisterPrefix(pair.Value.FullPrefix, pair.Key);
                }
            }
        }

        internal void UnregisterAllPrefixes()
        {
            lock (_prefixes)
            {
                // go through the uri list and unregister for each one of them
                foreach (var prefix in _prefixes.Values)
                {
                    // ignore possible failures
                    _urlGroup.UnregisterPrefix(prefix.FullPrefix);
                }
            }
        }
    }
}
