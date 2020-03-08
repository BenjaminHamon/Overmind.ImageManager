using System.IO;

namespace Overmind.ImageManager.Model.Serialization
{
	public interface ISerializer
	{
		void Serialize(Stream stream, object value);
		T Deserialize<T>(Stream stream);

		void SerializeToFile(string path, object value);
		T DeserializeFromFile<T>(string path);

		string SerializeToString(object value);
		T DeserializeFromString<T>(string serializedValue);
	}
}
