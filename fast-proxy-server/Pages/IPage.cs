
namespace Pages
{
	internal interface IPage
	{
		public bool HighLevel { get; set; }
		public bool Test(string url);
		public Task<byte[]> CreateHtml(string url, byte[] request);
	}
}
