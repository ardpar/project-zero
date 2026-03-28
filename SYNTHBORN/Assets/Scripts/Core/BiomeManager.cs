using UnityEngine;
using UnityEngine.Tilemaps;

namespace Synthborn.Core
{
    /// <summary>
    /// Swaps arena tileset at run start for biome variety.
    /// Reads floor/wall Tilemaps and replaces their tiles.
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private TileBase[] _floorTiles;
        [SerializeField] private TileBase[] _wallTiles;

        private void Start()
        {
            if (_floorTiles == null || _floorTiles.Length == 0) return;

            int biomeIndex = Random.Range(0, _floorTiles.Length);
            SwapTiles(_floorTilemap, _floorTiles[biomeIndex]);
            if (_wallTiles != null && biomeIndex < _wallTiles.Length)
                SwapTiles(_wallTilemap, _wallTiles[biomeIndex]);
        }

        private void SwapTiles(Tilemap tilemap, TileBase newTile)
        {
            if (tilemap == null || newTile == null) return;

            var bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(pos))
                        tilemap.SetTile(pos, newTile);
                }
            }
            tilemap.RefreshAllTiles();
        }
    }
}
