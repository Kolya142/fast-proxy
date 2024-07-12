
namespace Logging
{
	struct LogEntryStruct
	{
		public byte[] Request;
		public byte[] Response;
		public string type;
		public string url;
		public string ClientAddr;
		public override string ToString()
		{
			return 
			   $"{type} {url} HTTP, {ClientAddr}\n" +
				"Request:\n" +
				Utils.BytesToString(Request) + "\n" +
				"Response:\n" +
				Utils.BytesToString(Response);
		}
	}
}
