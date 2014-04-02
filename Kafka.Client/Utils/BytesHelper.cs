using System;
using System.Linq;
using System.Text;

namespace Kafka.Client.Utils
{
	public static class BytesHelper
	{
		public static int GetScalarBytesCount<T>(T obj)
		{
			return GetScalarBytesCount(obj, typeof(T));
		}

		private static int GetScalarBytesCount(object obj, Type type)
		{
			if (type == typeof (byte))
				return sizeof (byte);
			if (type == typeof (Int16))
				return sizeof (Int16);
			if (type == typeof (Int32))
				return sizeof (Int32);
			if (type == typeof (Int64))
				return sizeof (Int64);
			if (type == typeof (byte[]))
			{
				var bytes = (byte[]) obj;
				return sizeof (Int32) + bytes.Length;
			}
			if (type == typeof (string))
			{
				var str = (string) obj;
				var bytes = Encoding.UTF8.GetBytes(str);
				return sizeof (Int32) + bytes.Length;
			}
			throw new InvalidOperationException(string.Format("Can't get bytes count for scalar {0}, unknown type: {1}", obj, type));
		}

		public static int GetArrayBytesCount<T>(T[] objs)
		{
			return GetArrayBytesCount(objs.Select(o => (object)o).ToArray(), typeof (T));
		}

		private static int GetArrayBytesCount(object[] objs, Type type)
		{
			return GetScalarBytesCount(objs.Length) + objs.Select(o => GetScalarBytesCount(o, type)).Sum();
		}
	}
}