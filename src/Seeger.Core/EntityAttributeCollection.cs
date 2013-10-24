﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Seeger
{
    public class EntityAttributeCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private Dictionary<string, string> _innerDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private XElement _xml;

        public XElement XmlData
        {
            get { return _xml; }
        }

        public int Count
        {
            get { return _innerDic.Count; }
        }

        public EntityAttributeCollection()
            : this(null)
        {
        }

        public EntityAttributeCollection(XElement attributeData)
        {
            _xml = new XElement("attributes");

            if (attributeData != null)
            {
                Verify(attributeData);
                UpdateFrom(attributeData);
            }
        }

        public bool ContainsKey(string key)
        {
            return _innerDic.ContainsKey(key);
        }

        public void Clear()
        {
            foreach (var each in _innerDic.Keys)
            {
                Remove(each);
            }
        }

        public T GetValue<T>(string key)
        {
            return GetValue<T>(key, default(T));
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            var value = GetValue(key);
            if (value == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public string GetString(string key)
        {
            return GetString(key, null);
        }

        public string GetString(string key, string defaultValue)
        {
            object value = GetValue(key);
            return value == null ? defaultValue : value.ToString();
        }

        public string[] GetStringArray(string key, char valueSeparator)
        {
            string value = GetString(key);
            if (value == null)
            {
                return new string[0];
            }

            return value.SplitWithoutEmptyEntries(valueSeparator);
        }

        public TElement[] GetArray<TElement>(string key, char valueSeparator)
        {
            var elementType = typeof(TElement);
            return GetStringArray(key, valueSeparator).Select(it => (TElement)Convert.ChangeType(it, elementType)).ToArray();
        }

        public virtual string GetValue(string key)
        {
            if (!_innerDic.ContainsKey(key))
            {
                return null;
            }
            return _innerDic[key];
        }

        public virtual void Add(string key, object value)
        {
            Require.NotNullOrEmpty(key, "key");
            Require.NotNull(value, "value");
            Require.That(!ContainsKey(key), "Key '" + key + "' already exists.");

            var strValue = value.AsString();

            _innerDic.Add(key, strValue);
            _xml.Add(CreateAttributeElement(key, strValue));
        }

        public virtual void AddRange(IEnumerable<KeyValuePair<string, string>> attributes)
        {
            Require.NotNull(attributes, "attributes");

            foreach (var item in attributes)
            {
                Add(item.Key, item.Value);
            }
        }

        public virtual void Set(string key, object newValue)
        {
            Require.NotNullOrEmpty(key, "key");
            Require.NotNull(newValue, "newValue");
            Require.That(ContainsKey(key), "Key '" + key + "' was not found.");

            var strNewValue = newValue.AsString();

            _innerDic[key] = strNewValue;
            FindAttributeElement(key).Attribute("value").Value = strNewValue;
        }

        public void AddOrSet(string key, object value)
        {
            if (ContainsKey(key))
            {
                Set(key, value);
            }
            else
            {
                Add(key, value);
            }
        }

        public virtual bool Remove(string key)
        {
            Require.NotNullOrEmpty(key, "key");

            if (_innerDic.Remove(key))
            {
                FindAttributeElement(key).Remove();
                return true;
            }
            return false;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(_innerDic);
        }

        private void UpdateFrom(XElement xml)
        {
            foreach (var element in xml.Elements())
            {
                var kv = Parse(element);
                _innerDic.Add(kv.Key, kv.Value);
                _xml.Add(new XElement(element));
            }
        }

        private KeyValuePair<string, string> Parse(XElement xml)
        {
            var value = xml.Attribute("value").Value;
            return new KeyValuePair<string, string>(xml.Attribute("key").Value, value);
        }

        private void Verify(XElement xml)
        {
            if (xml.Name != "attributes")
            {
                throw new InvalidOperationException("Tag name of the root node of the xml must be 'attributes'.");
            }

            foreach (XElement element in xml.Elements())
            {
                if (element.Name != "attr")
                {
                    throw new InvalidOperationException("Attriubute item xml element's tag name must be 'attr'.");
                }
                if (element.Attribute("key") == null || element.Attribute("value") == null)
                {
                    throw new InvalidOperationException("'key', 'value' attributes are required for attriubte item xml element.");
                }
                if (element.HasElements)
                {
                    throw new InvalidOperationException("Attriubte item xml element cannot have child elements.");
                }
            }
        }

        private XElement CreateAttributeElement(string key, object value)
        {
            return new XElement("attr",
                new XAttribute("key", key),
                new XAttribute("value", value));
        }

        private XElement FindAttributeElement(string key)
        {
            return _xml.Elements().FirstOrDefault(it => it.Attribute("key").Value == key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _innerDic.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
