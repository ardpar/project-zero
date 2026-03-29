# World Map & Persistent Progression System

> **Status**: Approved
> **Created**: 2026-03-29

## Overview

SYNTHBORN'u stage-select RPG yapısına dönüştürme. Oyuncu kalıcı bir karakter yaratır, dünya haritasından level seçer, wave'leri temizler, loot kazanır, haritaya döner.

## Core Flow

```
[Main Menu]
  ├─ New Game → Character Creation → World Map
  ├─ Load Game → World Map
  └─ Settings / Quit

[World Map] (hub ekranı)
  ├─ Level Grid (100 level, kilitli/açık)
  ├─ Inventory butonu → Equipment ekranı
  ├─ Skill Tree butonu → Pasif skill ağacı
  ├─ Character Info (isim, sınıf, level, statlar)
  └─ Save & Quit

[Level Gameplay]
  ├─ 5 Wave + Boss (mevcut WaveSpawner)
  ├─ Mutasyonlar run-içi (sıfırlanır)
  ├─ Loot + Gold + XP kazanılır
  └─ Level Complete → World Map'e dön

[Level Complete]
  ├─ Sonraki level açılır
  ├─ Loot/Gold/XP özeti
  └─ "Return to Map" butonu
```

## Scene Yapısı

| Scene | İçerik |
|-------|--------|
| MainMenu | New Game / Load Game / Settings |
| WorldMap | Dünya haritası, inventory, skill tree, karakter bilgisi |
| Gameplay (SampleScene) | Wave combat (mevcut) |

## Karakter Sistemi

### Karakter Yaratma
- İsim girişi (InputField)
- Sınıf seçimi (4 sınıf)
- Kaydet → WorldMap'e geç

### Sınıflar
| Sınıf | HP | DMG | Speed | Crit | Armor |
|-------|-----|------|-------|------|-------|
| Warrior | +20% | 0 | -10% | 0 | +10 |
| Rogue | -15% | 0 | +20% | +15% | 0 |
| Mage | -20% | +25% | 0 | 0 | -5 |
| Sentinel | +10% | +10% | +10% | +5% | +5 |

### Kalıcı Level + Stat
- XP kazanarak level-up (level 1-99)
- Her level: +1 stat point (STR/VIT/AGI/LCK/WIS)
- Level-up eşik: `base_xp * (1 + level * 0.15)`

## Dünya Haritası

### 100 Level Grid
- 10x10 grid veya linear path
- Her level: isim, zorluk yıldızı (★-★★★★★), biome ikonu
- Kilitli level: koyu, tıklanamaz
- Açık level: parlak, tıklanabilir
- Tamamlanmış level: yeşil tik
- Level 1 başta açık, her tamamlanan level sonrakini açar

### Level Tanımı
Mevcut `LevelData` SO genişletilir:
- `levelNumber` (1-100)
- `isUnlocked` → SaveData'dan okunur
- `isCompleted` → SaveData'dan okunur
- `starRating` (1-5 zorluk göstergesi)

## Equipment / Inventory

### 6 Slot
| Slot | Stat Etkisi |
|------|-------------|
| Helmet | HP, özel efekt |
| Armor | Armor, Damage Reduction |
| Weapon | Base Damage, Attack Speed |
| Gloves | Crit Chance, Crit Damage |
| Boots | Move Speed, Dash CD |
| Accessory | Herhangi stat |

### Item Rarity
| Rarity | Renk | Stat Range |
|--------|------|-----------|
| Common | Beyaz | +5-10% |
| Uncommon | Yeşil | +10-20% |
| Rare | Mavi | +20-35% |
| Epic | Mor | +35-50% |
| Legendary | Altın | +50-100% |

### Loot Kaynakları
- Wave düşmanları: Common/Uncommon
- Elite: Uncommon/Rare
- Boss: Rare/Epic garantili, Legendary şansı
- Yüksek level = daha iyi loot

## Pasif Skill Tree

### 4 Dal, ~40 Node
| Dal | Node'lar |
|-----|----------|
| Might | +5% DMG, +2% Crit, +10% Crit DMG, +5% AtkSpd... |
| Vitality | +10% HP, +3 Armor, +2% HP Regen, +5% Heal... |
| Agility | +5% Speed, -10% Dash CD, +3% Dodge, +1 Dash... |
| Fortune | +10% Gold, +5% Drop Rate, +10% XP, +5% Rare Drop... |

### Point Kaynağı
- Her karakter level-up: +1 skill point
- Bazı achievement'lar: +1 bonus skill point

## Save Sistemi

### SaveData Genişletme
```
CharacterSaveData:
  - characterName
  - classType (0-3)
  - characterLevel
  - characterXP
  - statPoints[5] (STR/VIT/AGI/LCK/WIS)
  - equippedItems[6] (slot → item ID)
  - inventoryItems[] (tüm sahip olunan itemler)
  - skillTreeNodes[] (açılmış node ID'leri)
  - completedLevels[] (tamamlanmış level numaraları)
  - currentGold
```

## Implementation Sırası

| Sprint | İçerik |
|--------|--------|
| 12 | MainMenu redesign + Character Creation + Save/Load overhaul |
| 13 | World Map UI + Level select + Level unlock sistemi |
| 14 | Inventory + Equipment + Item drop sistemi |
| 15 | Pasif Skill Tree + Stat point dağıtımı |
| 16 | 100 level tanımlama + balans + polish |
