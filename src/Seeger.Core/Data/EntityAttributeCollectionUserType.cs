﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Linq;

using NHibernate.UserTypes;
using NHibernate.SqlTypes;

namespace Seeger.Data
{
    class EntityAttributeCollectionUserType : IUserType
    {
        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            if (value == null) return null;

            return new EntityAttributeCollection(((EntityAttributeCollection)value).XmlData);
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }

            var collectionX = (EntityAttributeCollection)x;
            var collectionY = (EntityAttributeCollection)y;

            if (collectionX.Count != collectionY.Count)
            {
                return false;
            }

            foreach (var kv in collectionX)
            {
                if (!kv.Value.Equals(collectionY.GetValue(kv.Key)))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(object x)
        {
            return GetHashCode((EntityAttributeCollection)x);
        }

        private int GetHashCode(EntityAttributeCollection collection)
        {
            unchecked
            {
                int hash = 17;

                foreach (var kv in collection)
                {
                    hash = (hash * 397) ^ kv.Key.GetHashCode();
                    hash = (hash * 397) ^ kv.Value.GetHashCode();
                }

                return hash;
            }
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            string value = rs[names[0]].AsString();

            if (value.Length > 0)
            {
                return new EntityAttributeCollection(XElement.Parse(value));
            }
            return new EntityAttributeCollection();
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            string underlyingValue = String.Empty;

            if (value != null)
            {
                EntityAttributeCollection attrs = (EntityAttributeCollection)value;
                if (attrs.Count > 0)
                {
                    underlyingValue = attrs.XmlData.ToString();
                }
            }

            ((IDataParameter)cmd.Parameters[index]).Value = underlyingValue;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(EntityAttributeCollection); }
        }

        private static readonly SqlType[] _sqlTypes = new[] { NHibernate.NHibernateUtil.XmlDoc.SqlType };

        public NHibernate.SqlTypes.SqlType[] SqlTypes
        {
            get { return _sqlTypes; }
        }
    }
}
