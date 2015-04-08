using System;
using System.Xml;
using UnityEngine;

namespace Sapphire
{
    public static class ValueParser
    {
        public static object GetValueForType(XmlNode node, Type t, string value)
        {
            try
            {
                if (t == typeof(int))
                {
                    return int.Parse(value);
                }

                if (t == typeof(uint))
                {
                    return uint.Parse(value);
                }

                if (t == typeof(float))
                {
                    return float.Parse(value);
                }

                if (t == typeof(double))
                {
                    return double.Parse(value);
                }

                if (t == typeof(bool))
                {
                    if (value == "true") return true;
                    if (value == "false") return false;
                    if (value == "0") return false;
                    if (value == "1") return true;
                    return bool.Parse(value);
                }

                if (t == typeof(string))
                {
                    return value;
                }

                if (t == typeof(Vector2))
                {
                    var values = value.Split(',');
                    if (values.Length != 2)
                    {
                        throw new ParseException("Vector2 definition must have two components", node);
                    }

                    return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                }

                if (t == typeof(Vector3))
                {
                    var values = value.Split(',');
                    if (values.Length != 3)
                    {
                        throw new ParseException("Vector3 definition must have three components", node);
                    }

                    return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                }

                if (t == typeof(Vector4))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Vector4 definition must have four components", node);
                    }

                    return new Vector4(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }

                if (t == typeof(Rect))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Rect definition must have four components", node);
                    }

                    return new Rect(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }

                if (t == typeof(Color))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Color definition must have four components", node);
                    }

                    return new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }

                if (t == typeof(Color32))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Color32 definition must have four components", node);
                    }

                    return new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
                }
            }
            catch (Exception)
            {
                throw new ParseException(String.Format(
                    "Failed to parse value \"{0}\" for type \"{1}\"", value, t), node);
            }

            throw new UnsupportedTypeException(t, node);
        }

    }

}
