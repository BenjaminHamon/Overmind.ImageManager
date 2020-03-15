using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Overmind.ImageManager.Model.Compatibility
{
	internal class ImageModel_V2_Converter : JsonConverter<ImageModel>
	{
		public override ImageModel ReadJson(JsonReader reader, Type objectType,
			ImageModel existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader == null)
				return null;

			if (existingValue == null)
				existingValue = new ImageModel();

			JObject jsonObject = JObject.Load(reader);

			Uri source = jsonObject["Source"]?.ToObject<Uri>();
			jsonObject.Property("Source")?.Remove();

			using (var jsonObjectReader = jsonObject.CreateReader())
				serializer.Populate(jsonObjectReader, existingValue);

			existingValue.Source.Uri = source;

			return existingValue;
		}

		public override void WriteJson(JsonWriter writer, ImageModel value, JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}
	}
}
