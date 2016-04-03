using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Msmq
{
	public class MsmqUriParser
	{
		internal const string Scheme = "msmq";

		private readonly Regex regexIPAddress =
			new Regex(
				@"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$");

		public MsmqUriParser(Uri uri)
		{
			Guard.AgainstNull(uri, "uri");

			if (!uri.Scheme.Equals(Scheme, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new InvalidSchemeException(Scheme, uri.ToString());
			}

			var builder = new UriBuilder(uri);

			var host = uri.Host;

			if (host.Equals("."))
			{
				builder.Host = Environment.MachineName.ToLower();
			}

			if (uri.LocalPath == "/")
			{
				throw new UriFormatException(string.Format(EsbResources.UriFormatException, "msmq://{{host-name}}/{{queue-name}}",
					uri));
			}

			Uri = builder.Uri;

			Local = Uri.Host.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase);

			var usesIPAddress = regexIPAddress.IsMatch(host);

			Path = Local
				? string.Format(@"{0}\private$\{1}", host, uri.Segments[1])
				: usesIPAddress
					? string.Format(@"FormatName:DIRECT=TCP:{0}\private$\{1}", host, uri.Segments[1])
					: string.Format(@"FormatName:DIRECT=OS:{0}\private$\{1}", host, uri.Segments[1]);

			JournalPath = string.Concat(Path, "$journal");

			var parameters = HttpUtility.ParseQueryString(uri.Query);

			SetUseDeadLetterQueue(parameters);
		}

		public Uri Uri { get; private set; }
		public bool Local { get; private set; }
		public string Path { get; private set; }
		public string JournalPath { get; private set; }
		public bool UseDeadLetterQueue { get; private set; }

		private void SetUseDeadLetterQueue(NameValueCollection parameters)
		{
			UseDeadLetterQueue = true;

			var parameter = parameters.Get("useDeadLetterQueue");

			if (parameter == null)
			{
				return;
			}

			bool result;

			if (bool.TryParse(parameter, out result))
			{
				UseDeadLetterQueue = result;
			}
		}
	}
}