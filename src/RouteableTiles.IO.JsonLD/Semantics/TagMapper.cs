using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using OsmSharp;
using OsmSharp.Tags;

namespace RouteableTiles.IO.JsonLD.Semantics
{
    public static class TagMapper
    {
        private static readonly Lazy<Dictionary<string, TagMapperConfig>> LazyMappingConfigs = new Lazy<Dictionary<string, TagMapperConfig>>(
            () =>
            {
                using var embeddedStream = Assembly.GetAssembly(typeof(TagMapper))
                    .GetManifestResourceStream("RouteableTiles.IO.JsonLD.mapping_config.json");
                using var streamReader = new StreamReader(embeddedStream);
                return TagMapperConfigParser.ParseFromJson(streamReader.ReadToEnd());
            });

        /// <summary>
        /// Gets the default embedded mapping configs.
        /// </summary>
        public static Dictionary<string, TagMapperConfig> DefaultMappingConfigs => LazyMappingConfigs.Value;
        
        private static readonly Lazy<Dictionary<string, TagMapperKey>> LazyMappingKeys = new Lazy<Dictionary<string, TagMapperKey>>(
            () =>
            {
                using var embeddedStream = Assembly.GetAssembly(typeof(TagMapper))
                    .GetManifestResourceStream("RouteableTiles.IO.JsonLD.mapping_keys.json");
                using var streamReader = new StreamReader(embeddedStream);
                return TagMapperConfigParser.ParseKeysFromJson(streamReader.ReadToEnd());
            });

        /// <summary>
        /// Gets the default embedded mapping keys.
        /// </summary>
        public static Dictionary<string, TagMapperKey> DefaultMappingKeys => LazyMappingKeys.Value;
        
        /// <summary>
        /// Returns true if there is a mapping for any of the tags in the given osm object.
        /// </summary>
        /// <param name="osmGeo">The osm object.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="mappings">The mappings.</param>
        /// <returns>True, if this tags collection is relevant.</returns>
        public static bool IsRelevant(this OsmGeo osmGeo, Dictionary<string, TagMapperKey> keys, Dictionary<string, TagMapperConfig> mappings)
        {
            if (osmGeo?.Tags == null) return false;
            
            foreach (var tag in osmGeo.Tags)
            {
                if (!keys.TryGetValue(tag.Key, out var key)) continue;
                
                if (!string.IsNullOrWhiteSpace(key.Value) &&
                    key.Value != tag.Value) continue;

                switch (osmGeo.Type)
                {
                    case OsmGeoType.Node:
                        if (!key.Node) continue;
                        break;
                    case OsmGeoType.Way:
                        if (!key.Way) continue;
                        break;
                    case OsmGeoType.Relation:
                        if (!key.Relation) continue;
                        break;
                }

                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Maps the given tag using the given semantic mappings.
        /// </summary>
        /// <param name="tag">The tag to map.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="writer">The json writer.</param>
        /// <returns>True if there was a mapping for this tag.</returns>
        internal static async Task<bool> Map(this Tag tag, Dictionary<string, TagMapperConfig> mappings, JsonWriter writer)
        {
            if (!mappings.TryGetValue(tag.Key, out var mapperConfig)) return false;

            if (mapperConfig.mapping == null)
            {
                await writer.WritePropertyAsync(mapperConfig.predicate, tag.Value, true, true);
                return true;
            }
            if (mapperConfig.mapping.TryGetValue(tag.Value, out var mapped))
            {
                var mappedString = string.Empty;
                if (mapped != null) mappedString = mapped.ToInvariantString();
                if (mapped is int ||
                    mapped is long)
                {
                    await writer.WritePropertyAsync(mapperConfig.predicate, mappedString, false, false);
                }
                else
                {
                    await writer.WritePropertyAsync(mapperConfig.predicate, mappedString, true, true);
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}