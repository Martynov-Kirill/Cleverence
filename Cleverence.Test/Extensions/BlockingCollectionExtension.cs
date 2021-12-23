using Cleverence.Entities;

using System.Collections.Concurrent;
using System.Text;

namespace Cleverence.Test.Extensions
{
	public static class BlockingCollectionExtension
	{
		public static string ShowItems(this BlockingCollection<Message> collection)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var message in collection)
			{
				stringBuilder.AppendFormat($"\nid: {message.Id} - {message.Content}");
			}
			return stringBuilder.ToString();
		}
	}
}
