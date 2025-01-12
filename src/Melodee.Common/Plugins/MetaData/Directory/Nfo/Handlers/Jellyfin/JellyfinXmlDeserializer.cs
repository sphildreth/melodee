using System.Xml.Linq;
using Melodee.Common.Extensions;
using Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers.Jellyfin.Models.Jellyfin;

namespace Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers.Jellyfin;

public class JellyfinXmlDeserializer<T> where T : class, new()
{
    public T Deserialize(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        return ParseElement(doc.Root);
    }

    public T DeserializeFromFile(string filePath)
    {
        var doc = XDocument.Load(filePath);
        return ParseElement(doc.Root);
    }

    private T ParseElement(XElement element)
    {
        var result = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var value = GetPropertyValue(element, property.Name, property.PropertyType);
            if (value != null)
            {
                property.SetValue(result, value);
            }
        }

        return result;
    }

    private object GetPropertyValue(XElement element, string propertyName, Type propertyType)
    {
        var childElement = element.Element(propertyName.ToLower());

        var childElementName = childElement?.Name.ToString().ToNormalizedString();

        switch (childElementName)
        {
            case "ART":
                return element.Elements("art")
                    .Select(e => new Art
                    {
                        Poster = e.Element("poster").Value
                    }).ToList();

            case "ACTOR":
                return element.Elements("actor")
                    .Select(e => new Actor
                    {
                        Name = e.Element("name").Value,
                        Type = e.Element("type").Value
                    }).ToList();

            case "ARTIST":
                return element.Elements("artist").Select(x => x.Value).ToList();

            case "TRACK":
                return element.Elements("track")
                    .Select(e => new Track
                    {
                        Position = int.Parse(e.Element("position").Value),
                        Title = e.Element("title").Value,
                        Duration = e.Element("duration").Value
                    }).ToList();
        }


        if (propertyType == typeof(string))
        {
            return childElement.Value;
        }

        if (propertyType == typeof(int?) || propertyType == typeof(int))
        {
            return int.Parse(childElement.Value);
        }

        if (propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
        {
            return DateTime.Parse(childElement.Value);
        }

        if (propertyType == typeof(bool?) || propertyType == typeof(bool))
        {
            return bool.Parse(childElement.Value);
        }

        return null;
    }
}
