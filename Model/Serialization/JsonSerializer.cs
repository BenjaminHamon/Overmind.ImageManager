using Newtonsoft.Json;
using System.IO;

using JsonSerializerImplementation = Newtonsoft.Json.JsonSerializer;

namespace Overmind.ImageManager.Model.Serialization
{
	public class JsonSerializer : ISerializer
	{
		public JsonSerializer(JsonSerializerImplementation implementation)
		{
			this.implementation = implementation;
		}

		private readonly JsonSerializerImplementation implementation;

		public void Serialize(Stream stream, object value)
		{
			using (StreamWriter streamWriter = new StreamWriter(stream))
			using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
				implementation.Serialize(jsonWriter, value);
		}

		public T Deserialize<T>(Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			using (JsonReader jsonReader = new JsonTextReader(streamReader))
				return implementation.Deserialize<T>(jsonReader);
		}

		public void SerializeToFile(string path, object value)
		{
			using (FileStream fileStream = File.OpenWrite(path + ".tmp"))
				Serialize(fileStream, value);

			File.Delete(path);
			File.Move(path + ".tmp", path);
		}

		public T DeserializeFromFile<T>(string path)
		{
			using (FileStream fileStream = File.OpenRead(path))
				return Deserialize<T>(fileStream);
		}

		public string SerializeToString(object value)
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
					implementation.Serialize(jsonWriter, value);

				return stringWriter.ToString();
			}
		}

		public T DeserializeFromString<T>(string serializedValue)
		{
			using (StringReader stringReader = new StringReader(serializedValue))
			using (JsonReader jsonReader = new JsonTextReader(stringReader))
				return implementation.Deserialize<T>(jsonReader);
		}
	}
}
