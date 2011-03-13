using System;
using System.Collections;
using System.Collections.Generic;

#if !NET20
using System.Linq;
#endif

namespace FluentJson.Helpers
{
    public class TypeHelper
    {
        /// <summary>
        /// Sees if the given type is threatable as another type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="asType">Type to be threated as.</param>
        /// <returns></returns>
        static public bool IsThreatableAs(Type type, Type asType)
        {
            // Works for most cases
            if (asType.IsAssignableFrom(type)) return true;
            
            // Account for generics
            if (type.IsGenericType)
            {
                Type typeReduced = type.GetGenericTypeDefinition();
                Type asTypeReduced = asType;

                if (asType.IsGenericType)
                {
                    asTypeReduced = asType.GetGenericTypeDefinition();
                }

                // Verify again
                if (asTypeReduced.IsAssignableFrom(typeReduced))
                {
                    // Verify arguments
                    Type[] typeArguments = type.GetGenericArguments();
                    Type[] asTypeArguments = asType.GetGenericArguments();

                    // Lengths should be equal
                    if (typeArguments.Length != asTypeArguments.Length) return false;

                    bool isSuccess = true;
                    for(int i = 0; i < typeArguments.Length; i++)
                    {
                        // Arguments for type should be actual types
                        if (typeArguments[0].IsGenericParameter)
                        {
                            isSuccess = false;
                            break;
                        }

                        // Arguments for "as" type should be generic parameters
                        if (!asTypeArguments[0].IsGenericParameter)
                        {
                            isSuccess = false;
                            break;
                        }
                    }

                    if (isSuccess)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sees if the given type is numerical. Decimals do not count as a number.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsNumerical(Type type)
        {
            return (type.IsPrimitive) && type != typeof(bool) && type != typeof(char);
        }

        /// <summary>
        /// Sees if the given type is a list.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsList(Type type)
        {
            return IsThreatableAs(type, typeof(IList)) || IsThreatableAs(type, typeof(IList<>));
        }

        /// <summary>
        /// Sees if the given is a dictionary.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsDictionary(Type type)
        {
            return IsThreatableAs(type, typeof(IDictionary)) || IsThreatableAs(type, typeof(IDictionary<,>));
        }

        /// <summary>
        /// Sees if the given type is a generic type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsGeneric(Type type)
        {
            return type.GetGenericArguments().Length > 0;
        }
    }
}
