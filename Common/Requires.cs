#region Usings

using System;

#endregion

namespace Wongs.Common
{
	/// <summary>
	/// Assert Class.
	/// </summary>
    public static class Requires
    {
        #region "Public Methods"

		/// <summary>
		/// Determines whether argValue is type of T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void IsTypeOf<T>(string argName, object argValue)
        {
            if (!((argValue) is T))
            {
                throw new ArgumentException(string.Format("The argument '{0}' must be of type '{1}'.", argName, typeof(T).FullName));
            }
        }

		/// <summary>
		/// Determines whether argValue is less than zero.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void NotNegative(string argName, int argValue)
        {
            if (argValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName, string.Format("The argument '{0}' cannot be negative.", argName));
            }
        }

		/// <summary>
		/// Determines whether the argValue is null.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void NotNull(string argName, object argValue)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

		/// <summary>
		/// Determines whether the argValue is null or empty.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void NotNullOrEmpty(string argName, string argValue)
        {
            if (string.IsNullOrEmpty(argValue))
            {
                throw new ArgumentException(string.Format("The argument '{0}' cannot be null or empty.", argName), argName);
            }
        }

		/// <summary>
		/// Determins whether propertyValye is not null or empty.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argProperty">The arg property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNullOrEmpty(string argName, string argProperty, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ArgumentException(argName,
                                            string.Format("The property '{1}' in object '{0}' cannot be null or empty.", argName, argProperty));
            }
        }

		/// <summary>
		/// Determines whether propertyValue is less than zero.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argProperty">The arg property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNegative(string argName, string argProperty, int propertyValue)
        {
            if (propertyValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName,
                                                      string.Format("The property '{1}' in object '{0}' cannot be negative.", argName, argProperty));
            }
        }

		/// <summary>
		/// Determines whether propertyValue equal to testValue.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argProperty">The arg property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <param name="testValue">The test value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void PropertyNotEqualTo<TValue>(string argName, string argProperty, TValue propertyValue, TValue testValue) where TValue : IEquatable<TValue>
        {
            if (propertyValue.Equals(testValue))
            {
                throw new ArgumentException(argName, string.Format("The property '{1}' in object '{0}' is invalid.", argName, argProperty));
            }
        }

        #endregion
    }
}
