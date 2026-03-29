# SYNTHBORN — Expansion Vision: Roguelite ARPG Evolution

> **Status**: Draft
> **Created**: 2026-03-29

## Overview

SYNTHBORN'u basit survivor'dan **çok katmanlı roguelite ARPG**'ye evrimleştirme planı. Mevcut core loop (hareket + otomatik saldırı + mutasyonlar) korunur, üstüne derinlik katmanları eklenir.

---

## Yeni Core Loop

```
Ana Menü → Form Seç → Level 1 Start
  │
  ├─ Wave 1-5 (düşmanlar, loot düşer)
  ├─ Boss Fight (özel item dropu)
  ├─ Level Complete → Inventory/Shop ekranı
  │
  ├─ Level 2 (yeni biome, daha zor)
  ├─ Wave 1-5 + Boss
  ├─ Level Complete → Inventory/Shop
  │
  ├─ Level 3, 4, 5... (sonsuz, zorluk katlanır)
  │
  └─ Ölüm → Run Özeti → Altın/XP kazan → Meta Progression
```

---

## Sistem 1: Level/Stage Progression

**Mevcut**: 1 run = 6 wave + boss → biter
**Yeni**: 1 run = Level 1 (5 wave + boss) → Level 2 (5 wave + boss) → ... sonsuz

### Detaylar
- Her level bir `LevelData` ScriptableObject: biome tipi, düşman havuzu, boss tipi, zorluk çarpanı
- Level geçişinde kısa ara ekran: loot toplama, inventory düzenleme, shop
- Zorluk formülü: `base_difficulty * (1 + level * 0.3)` — her level %30 daha zor
- Biome sırası: Caverns → Jungle → Temple → Hell → rastgele tekrar (harder)
- Her level'ın kendi boss'u: Level 1 Cavern Guardian, Level 2 Jungle Beast, vb.

### Mimari Değişiklik
- `WaveSpawner` → boss yenilince run bitirmez, `LevelManager`'a bildirir
- `LevelManager` yeni component: level geçişi, biome swap, zorluk scaling
- `WaveTableData` per-level veya dinamik scaling ile

---

## Sistem 2: Inventory + Equipment

### Slot Sistemi
| Slot | Etki |
|------|------|
| Weapon | Base damage, attack speed, özel efekt |
| Armor | HP bonus, armor, damage reduction |
| Accessory 1 | Pasif bonus (crit, speed, XP gain vb.) |
| Accessory 2 | Pasif bonus |

### Item Yapısı
```
ItemData (ScriptableObject):
  - id, displayName, description
  - rarity: Common / Uncommon / Rare / Epic / Legendary
  - slotType: Weapon / Armor / Accessory
  - statModifiers: HP, Speed, Damage, Crit, Armor, AttackSpeed
  - specialEffect: string (özel mekanik ID)
  - icon: Sprite
```

### Rarity Sistemi
| Rarity | Drop Chance | Stat Bonus Range | Renk |
|--------|-------------|------------------|------|
| Common | 50% | +5-10% | Beyaz |
| Uncommon | 30% | +10-20% | Yeşil |
| Rare | 15% | +20-35% | Mavi |
| Epic | 4% | +35-50% | Mor |
| Legendary | 1% | +50-100% | Altın |

### Kaynak
- Normal düşmanlardan: Common/Uncommon item + altın
- Elite düşmanlardan: Uncommon/Rare item
- Boss'lardan: Rare/Epic/Legendary item (garantili 1 drop)
- Mini-boss: Uncommon/Rare garantili

---

## Sistem 3: Altın Ekonomisi

### Kazanma
- Her düşman öldürmede: 1-5 altın (tier'e göre)
- Wave tamamlama bonusu: 10-50 altın (level'e göre)
- Boss kill bonusu: 100+ altın

### Harcama
- Level arası shop: item satın al, iyileştirme (heal)
- Craft maliyetleri
- Pasif skill unlock

### Kalıcılık
- Altın run içi harcama (shop, craft)
- Run sonunda kalan altının %20'si meta altına dönüşür (kalıcı)

---

## Sistem 4: Craft Sistemi

### Materyal Tipleri
| Materyal | Kaynak | Kullanım |
|----------|--------|----------|
| Scrap Metal | Normal düşman dropu | Common craft |
| Dark Crystal | Elite/Boss dropu | Rare+ craft |
| Boss Essence | Boss kill (unique per boss) | Legendary craft |

### Craft Mekanikleri
- 3x Scrap Metal → Random Common Item
- 2x Dark Crystal + 1x Scrap → Random Rare Item
- 1x Boss Essence + 3x Dark Crystal → Specific Legendary (boss'a göre)
- İki item birleştirme: stat'ları combine et (daha güçlü tek item)

### Craft UI
- Level arası ekranda veya inventory'den erişim
- Tarif listesi (keşfedilen tarifler)

---

## Sistem 5: Pasif Skill Tree

### Yapı
Halka şeklinde skill tree — ortadan dışa doğru dallanır.

### Dal Tipleri
| Dal | Odak | Örnek Pasifler |
|-----|------|----------------|
| Might | Damage + Crit | +5% DMG, +2% Crit, +10% Crit DMG |
| Fortitude | HP + Armor | +10% HP, +3 Armor, +5% Heal |
| Swiftness | Speed + Dash | +5% Speed, -10% Dash CD, +1 Dash Charge |
| Wisdom | XP + Loot | +10% XP, +5% Drop Rate, +10% Gold |

### Unlock Mekanığı
- Meta altın ile pasif node'ları aç
- Her node 1 kez alınır, kalıcı
- Toplam ~30-40 node, hepsini açmak uzun vadeli hedef

---

## Sistem 6: Player Level (Run İçi)

**Mevcut**: Level-up → mutasyon seç
**Yeni**: Level-up → mutasyon seç + stat point kazan

### Stat Point Dağıtımı
- Her level-up'ta 1 stat point
- 5 stat'a dağıt: STR (damage), VIT (HP), AGI (speed), LCK (crit), WIS (XP)
- Noktalar run içi, run sonunda sıfırlanır
- Mutasyon seçimi de devam eder (ikisi birlikte)

---

## Öncelik Sırası (Sprint Planı)

| Sıra | Sistem | Neden Önce |
|------|--------|------------|
| 1 | **Level/Stage Progression** | Tüm diğer sistemlerin temeli — multi-level yapısı olmadan diğerleri anlamsız |
| 2 | **Altın + Item Drop** | Level'lar arası motivasyon — "ne düşecek?" merakı |
| 3 | **Inventory + Equipment** | Drop'ları kullanalabilmek için gerekli |
| 4 | **Boss Loot + Craft** | Inventory üstüne derinlik |
| 5 | **Pasif Skill Tree** | Uzun vadeli meta-progression |
| 6 | **Stat Point Sistemi** | Level-up'a ek katman |

### Tahmini Sprint Dağılımı
- Sprint 11: Level/Stage + Altın drop
- Sprint 12: Inventory + Equipment + Boss Loot
- Sprint 13: Craft + Shop (level arası)
- Sprint 14: Pasif Skill Tree + Stat Points
- Sprint 15: Polish + Balance + Content (yeni bosslar, itemlar)
