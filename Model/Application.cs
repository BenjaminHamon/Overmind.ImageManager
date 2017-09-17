using Newtonsoft.Json;

namespace Overmind.ImageManager.Model
{
	public class Application
	{
		public Application()
		{
			JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
			DataProvider = new DataProvider(serializer);
		}

		public DataProvider DataProvider { get; }
	}
}
