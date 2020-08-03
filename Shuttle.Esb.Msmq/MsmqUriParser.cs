using System;
using System.Text.RegularExpressions;
using Shuttle.Core.Contract;
using Shuttle.Core.Uris;

namespace Shuttle.Esb.Msmq
{
    public class MsmqUriParser
    {
        internal const string Scheme = "msmq";

        private readonly Regex _regexIpAddress =
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
                throw new UriFormatException(string.Format(Esb.Resources.UriFormatException,
                    "msmq://{{host-name}}/{{queue-name}}", uri));
            }

            Uri = builder.Uri;

            Local = Uri.Host.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase);

            var usesIPAddress = _regexIpAddress.IsMatch(host);

            Path = Local
                ? $@"{host}\private$\{uri.Segments[1]}"
                : usesIPAddress
                    ? $@"FormatName:DIRECT=TCP:{host}\private$\{uri.Segments[1]}"
                    : $@"FormatName:DIRECT=OS:{host}\private$\{uri.Segments[1]}";

            JournalPath = string.Concat(Path, "$journal");

            var queryString = new QueryString(uri);

            SetUseDeadLetterQueue(queryString);
        }

        public Uri Uri { get; }
        public bool Local { get; }
        public string Path { get; }
        public string JournalPath { get; }
        public bool UseDeadLetterQueue { get; private set; }

        private void SetUseDeadLetterQueue(QueryString queryString)
        {
            UseDeadLetterQueue = true;

            var parameter = queryString["useDeadLetterQueue"];

            if (parameter == null)
            {
                return;
            }

            if (bool.TryParse(parameter, out var result))
            {
                UseDeadLetterQueue = result;
            }
        }
    }
}