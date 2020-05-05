using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OsmSharp;
using OsmSharp.Db.Tiled.OsmTiled;
using RouteableTiles.IO.JsonLD;

namespace RouteableTiles.API
{
    internal static class OsmDbExtensions
    {
        internal static IEnumerable<OsmGeo> GetRouteableTile(this OsmTiledDbBase db, (uint x, uint y) tile, 
            Func<OsmGeo, bool> isRelevant, byte[]? buffer = null)
        {
            return db.Get(new[] {tile}).Select(x => x.osmGeo).GetRouteableTile(
                isRelevant, key => db.Get(key, buffer)?.osmGeo);
        }
    }
}