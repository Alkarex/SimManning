using System;

namespace SimManning
{
	/// <summary>
	/// Extensions adding .NET 4.5 functionnality for legacy compatibility with .NET 4.0.
	/// </summary>
	public static class ExtensionsLegacy
	{
		/// <summary>
		/// Returns the TypeInfo representation of the specified type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to convert.</param>
		/// <returns>
		/// The converted object.
		/// In the original function from .NET 4.5, returns a <see cref="System.Reflection.TypeInfo"/> object,
		/// but in this legacy extension for .NET 4.0, returns a <see cref="System.Type"/> object instead.
		/// </returns>
		/// <remarks>
		/// This is only a subset of the real .NET 4.5
		/// <see cref="System.Reflection.IntrospectionExtensions.GetTypeInfo"/>() function
		/// http://msdn.microsoft.com/en-us/library/system.reflection.introspectionextensions.gettypeinfo(v=VS.110).aspx
		/// </remarks>
		public static Type GetTypeInfo(this Type type)
		{
			return type;
		}
	}
}
