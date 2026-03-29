using UnityEngine;
using UnityEngine.Tilemaps;

namespace Synthborn.Core
{
    /// <summary>
    /// Swaps arena tileset at run start for biome variety.
    /// Applies per-biome ambient tint to the camera background.
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private TileBase[] _floorTiles;
        [SerializeField] private TileBase[] _wallTiles;

        [Header("Biome Atmosphere")]
        [Tooltip("Camera background color per biome (index matches floor/wall arrays).")]
        [SerializeField] private Color[] _biomeTints;

        private void Start()
        {
            if (_floorTiles == null || _floorTiles.Length == 0) return;

            int biomeIndex = Random.Range(0, _floorTiles.Length);
            SwapTiles(_floorTilemap, _floorTiles[biomeIndex]);
            if (_wallTiles != null && biomeIndex < _wallTiles.Length)
                SwapTiles(_wallTilemap, _wallTiles[biomeIndex]);

            ApplyBiomeTint(biomeIndex);
        }

        private void ApplyBiomeTint(int biomeIndex)
        {
            if (_biomeTints == null || biomeIndex >= _biomeTints.Length) return;

            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = _biomeTints[biomeIndex];
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
