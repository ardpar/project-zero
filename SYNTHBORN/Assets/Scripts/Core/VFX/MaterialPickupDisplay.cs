using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Shows floating text when crafting materials are awarded from enemy kills.
    /// Reuses the same pattern as DamageNumber but for material names.
    /// Attach to the DamageNumberSpawner GameObject or any persistent object in SampleScene.
    /// </summary>
    public class MaterialPickupDisplay : MonoBehaviour
    {
        [SerializeField] private int _maxPopups = 10;

        private Pool.ObjectPool<DamageNumber> _pool;

        private void Awake()
        {
            _pool = new Pool.ObjectPool<DamageNumber>(() =>
            {
                var go = new GameObject("MatPopup");
                go.transform.SetParent(transform);
                var dn = go.AddComponent<DamageNumber>();
                dn.SetPool(_pool);
                return dn;
            }, _maxPopups);
        }

        private void OnEnable()
        {
            GameEvents.OnMaterialAwarded += OnMaterialAwarded;
        }

        private void OnDisable()
        {
            GameEvents.OnMaterialAwarded -= OnMaterialAwarded;
        }

        private void OnMaterialAwarded(string materialName, Vector2 worldPos)
        {
            if (_pool.ActiveCount >= _maxPopups) return;

            var dn = _pool.Get();
            // Init at offset position so it doesn't overlap damage numbers
            dn.Init(0, false, worldPos + Vector2.up * 0.8f);

            // Override the TextMesh to show material name instead of "0"
            var textMesh = dn.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = $"+1 {materialName}";
                textMesh.fontSize = 36;
                textMesh.color = GetMaterialColor(materialName);
                textMesh.characterSize = 0.12f;
            }
        }

        private static Color GetMaterialColor(string name)
        {
            if (name.Contains("Stabilized")) return new Color(1f, 0.7f, 0.2f);    // Gold
            if (name.Contains("Mutation")) return new Color(0.6f, 0.3f, 0.9f);     // Purple
            return new Color(0.6f, 0.8f, 0.6f);                                     // Green (Compound)
        }
    }
}
