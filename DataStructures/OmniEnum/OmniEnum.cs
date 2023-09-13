using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using System.Text;
using OmnicatLabs.StatefulObject;

namespace OmnicatLabs.OmniEnum
{
    public class OmniEnum<EnumType, DataType> : IComparable, IEnumerable<OmniEnum<EnumType, DataType>>
    {
        public int id { get; private set; }
        public string name { get; private set; }

        public DataType data { get; set; }

        protected static List<OmniEnum<EnumType, DataType>> fields = new List<OmniEnum<EnumType, DataType>>();

        protected OmniEnum()
        {
            fields.Add(this);
            id = fields.Count - 1; //change all id creates to this

            //If the enum is a State we need to do something special
            if (typeof(EnumType).GetGenericTypeDefinition() == typeof(State<>))
            {
                //Because the fields of a State will be determined by whatever it is inherited by, we have to check for which class that is.
                //Because each generic definition of a State is treated as a different type, there should only be one class that inherits from the State hence index 0
                //Debug.Log(Assembly.GetAssembly(typeof(EnumType)).GetTypes()
                //    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(EnumType)))
                //    .ToList()[0].GetFields()[id].Name);
                name = Assembly.GetAssembly(typeof(EnumType)).GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(EnumType))).ToList()[0].GetFields()[id].Name;
            }
            else
            {
                name = typeof(EnumType).GetType().GetProperties()[id].Name;
            }
        }

        public OmniEnum(string _name)
        {
            fields.Add(this);
            id = fields.Count - 1;
            name = _name;
        }

        public OmniEnum(string _name, DataType _data)
        {
            name = _name;
            data = _data;
        }

        public static void AddField(OmniEnum<EnumType, DataType> field)
        {
            fields.Add(field);
            field.id = fields.Count - 1;
            //Debug.Log(field.id);
            //Debug.Log(fields);
        }

        public static void RemoveField(string fieldName)
        {
            fields.Find(field => field.name == fieldName).Clear();
            fields.RemoveAll(field => field.name == fieldName);
        }
        public static implicit operator OmniEnum<EnumType, DataType>(DataType _data)
        {
            var newEnum = new OmniEnum<EnumType, DataType>();
            newEnum.data = _data;
            return newEnum;
        }
        public static implicit operator DataType(OmniEnum<EnumType, DataType> _field) => _field.data;

        public static implicit operator OmniEnum<EnumType, DataType>(string enumName)
        {
            if (fields.Any(field => field.name == enumName))
            {
                return fields.Find(field => field.name == enumName);
            }
            else
            {
                Debug.LogError($"{enumName} not found in list of available fields.\nEnsure the name of the state you are trying to change to exists");
                return null;
            }
        }

        public override string ToString() => name;

        public static List<OmniEnum<EnumType, DataType>> GetAll() => fields;

        public static void Sort(Comparison<OmniEnum<EnumType, DataType>> comparison)
        {
            fields.Sort(comparison);
            foreach (var field in fields)
            {
                field.id = fields.IndexOf(field) + 1; //look at this later
            }
        }

        public OmniEnum<EnumType, DataType> Find(Predicate<OmniEnum<EnumType, DataType>> predicate)
        {
            foreach (var field in fields)
            {
                if (predicate.Invoke(field))
                {
                    return field;
                }
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is not OmniEnum<EnumType, DataType> otherValue)
            {
                return false;
            }

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = id.Equals(otherValue.id);

            return typeMatches && valueMatches;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public int CompareTo(object other) => id.CompareTo(((OmniEnum<EnumType, DataType>)other).id);

        public IEnumerator<OmniEnum<EnumType, DataType>> GetEnumerator() => fields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ForEach(Action<OmniEnum<EnumType, DataType>> action)
        {
            foreach (var field in fields)
            {
                action.Invoke(field);
            }
        }

        public static void ClearDynamics()
        {
            fields.ForEach(field => field.Clear());
            fields.Clear();
        }

        private void Clear()
        {
            name = null;
            id = -1;
            data = default;
        }

        public object OmniDebug()
        {
            return OmniEnumDebugger();
        }

        //Defines the indexer so we can treat the enum as an array
        public OmniEnum<EnumType, DataType> this[int i]
        {
            get { return fields[i]; }
            set { fields[i] = value; }
        }

        protected StringBuilder OmniEnumDebugger()
        {
            StringBuilder debugString = new StringBuilder();
            debugString.Append($"Name: {name} ");
            debugString.Append($"ID: {id}, ");
            debugString.Append($"Data Type: {data.GetType()} ");
            return debugString;
        }

        #region Operator Overloads
        public static OmniEnum<EnumType, DataType> operator +(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b)
        {
            var field = fields.Find(field => field.id == a.id + b.id);
            if (field == null)
            {
                Debug.LogError($"ID addition of {a} and {b} resulted in a field that does not exist");
                return null;
            }
            return field;
        }
        public static OmniEnum<EnumType, DataType> operator -(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b)
        {
            {
                var field = fields.Find(field => field.id == a.id - b.id);
                if (field == null)
                {
                    Debug.LogError($"ID subtraction of {a} and {b} resulted in a field that does not exist");
                    return null;
                }
                return field;
            }
        }
        public static OmniEnum<EnumType, DataType> operator *(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b)
        {
            {
                var field = fields.Find(field => field.id == a.id * b.id);
                if (field == null)
                {
                    Debug.LogError($"ID Multiplication of {a} and {b} resulted in a field that does not exist");
                    return null;
                }
                return field;
            }
        }
        public static OmniEnum<EnumType, DataType> operator /(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b)
        {
            {
                var field = fields.Find(field => field.id == a.id / b.id);
                if (field == null)
                {
                    Debug.LogError($"ID division of {a} and {b} resulted in a field that does not exist");
                    return null;
                }
                return field;
            }
        }
        public static OmniEnum<EnumType, DataType> operator |(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b) => fields.Find(field => field.id == (a.id | b.id));
        public static OmniEnum<EnumType, DataType> operator ++(OmniEnum<EnumType, DataType> a)
        {
            var field = fields.Find(field => a.id + 1 == field.id);
            if (field == null)
            {
                Debug.LogError($"ID increment from {a} resulted in a field that does not exist");
                return null;
            }
            return field;
        }
        public static OmniEnum<EnumType, DataType> operator --(OmniEnum<EnumType, DataType> a)
        {
            var field = fields.Find(field => a.id - 1 == field.id);
            if (field == null)
            {
                Debug.LogError($"ID decrement from {a} resulted in a field that does not exist");
                return null;
            }
            return field;
        }
        public static bool operator <(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b) => a.id < b.id;
        public static bool operator >(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b) => a.id > b.id;
        public static bool operator <=(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b) => a.id <= b.id;
        public static bool operator >=(OmniEnum<EnumType, DataType> a, OmniEnum<EnumType, DataType> b) => a.id >= b.id;
        #endregion
    }
}