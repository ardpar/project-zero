# Arena Map & Persistent Calibration System

> **Status**: Approved (Narrative-Aligned v2)
> **Created**: 2026-03-29
> **Narrative Framework**: Option A — "Arena Remembers, Subject Doesn't"

## Overview

SYNTHBORN'u Arena haritası yapısına dönüştürme. Subject her döngüde sıfırlanır ama Arena önceki denemelerin verisini korur. Oyuncu Arena haritasından deneme odası seçer, wave'leri temizler, materyal kazanır, haritaya döner.

### Narrative Çerçeve

> Arena, SUBJECT-FINAL'ın her denemesini kaydeder. Subject hatırlamaz — ama Arena'nın dosyası büyür. Meta-progression, Arena'nın kalibrasyonudur. Oyuncu güçlenmez; Arena, deneyi optimize eder.

## Core Flow

```
[Ana Menü]
  ├─ Yeni Deneme → Substrate Konfigürasyonu Seç → Arena Haritası
  ├─ Devam Et → Arena Haritası
  └─ Ayarlar / Çıkış

[Arena Haritası] (hub ekranı)
  ├─ Deneme Odası Grid (100 oda, kilitli/açık)
  ├─ Envanter butonu → Komponent ekranı
  ├─ Kalibrasyon Ağacı butonu → Pasif parametre ağacı
  ├─ Subject Dosyası (designasyon, konfigürasyon, kalibrasyon seviyesi, parametreler)
  └─ Kaydet & Çıkış

[Deneme Odası Gameplay]
  ├─ 5 Wave + Stabilized (mevcut WaveSpawner)
  ├─ Mutasyonlar run-içi (ölümde sıfırlanır)
  ├─ Materyal + Fragment + Adaptasyon Verisi kazanılır
  └─ Oda Tamamlandı → Arena Haritası'na dön

[Oda Tamamlandı]
  ├─ Sonraki oda(lar) açılır
  ├─ Materyal/Fragment/Veri özeti
  └─ "Haritaya Dön" butonu
```

## Scene Yapısı

| Scene | İçerik |
|-------|--------|
| MainMenu | Yeni Deneme / Devam Et / Ayarlar |
| ArenaMap | Arena haritası, envanter, kalibrasyon ağacı, Subject dosyası |
| Gameplay (SampleScene) | Wave combat (mevcut) |

## Subject Dosyası Sistemi

### Substrate Konfigürasyonu Seçimi
- Designasyon suffix girişi (InputField) — örn: ARDENT, HOLLOW, SILENT
  - Tam isim: SUBJECT-FINAL-ARDENT
  - Klinik format, kişisel isim değil
- Konfigürasyon seçimi (4 Substrate varyantı)
- Kaydet → Arena Haritası'na geç

### Substrate Konfigürasyonları
| Konfigürasyon | HP | DMG | Speed | Crit | Armor | Lore Notu |
|---------------|-----|------|-------|------|-------|-----------|
| **DENSE LATTICE** | +20% | 0 | -10% | 0 | +10 | Yüksek kütle, yüksek dayanıklılık. Substrate'in en kalın dokuma formu. |
| **SEVERED THREAD** | -15% | 0 | +20% | +15% | 0 | Düşük kütle, yüksek iletkenlik. Substrate bağları gevşetilmiş — hız için stabilite feda edilmiş. |
| **NULL CASCADE** | -20% | +25% | 0 | 0 | -5 | Yüksek mutasyon oranı, yüksek instabilite. Substrate sınır parametreleri devre dışı. |
| **BALANCED FRAME** | +10% | +10% | +10% | +5% | +5 | Standart Substrate kalibrasyonu. Arena'nın varsayılan başlangıç konfigürasyonu. |

### Kalibrasyon Seviyesi (Persistent Level)
- Adaptasyon Verisi kazanarak kalibrasyon seviyesi artar (seviye 1-99)
- Her seviye: +1 kalibrasyon noktası (MASS/RESILIENCE/VELOCITY/VARIANCE/YIELD)
- Seviye eşik: `base_data * (1 + level * 0.15)`
- **Lore notu:** Kalibrasyon seviyesi, Arena'nın bu Subject hakkında biriktirdiği veri miktarıdır. Subject bunu bilmez — sadece Arena'nın sunduğu koşulların değiştiğini hisseder.

## Arena Haritası

### 100 Deneme Odası Grid
- 10x10 grid veya konsantrik halka düzeni (6 biome katmanına dağılmış)
- Her oda: designasyon, basınç seviyesi (★-★★★★★), biome ikonu
- Kilitli oda: koyu, tıklanamaz — "Arena bu odayı henüz açmadı"
- Açık oda: parlak, tıklanabilir — "Arena bu odayı test için hazırladı"
- Tamamlanmış oda: mavi veri izi — "Arena yeterli veri topladı"
- Oda 1 başta açık, her tamamlanan oda bitişik odaları açar

### Biome Dağılımı (6 Arena Çağı)
| Biome | Oda Aralığı | Era |
|-------|-------------|-----|
| The Atrium | 1-16 | Era 1 — Active Operation |
| The Assay Chambers | 17-33 | Era 2 — Peak Use |
| The Deep Archive | 34-50 | Era 3 — Post-Architect |
| The Collapse Stratum | 51-67 | Era 4 — Unknown Incident |
| The Corruption Layer | 68-84 | Era 5 — Current Drift |
| The Null Chamber | 85-100 | Era ? — Off-System |

**Lore notu:** Haritada ilerlemek, Arena'nın tarihinde geriye yolculuktur. Atrium (dış katman) en yeni ve temiz; Null Chamber (merkez) en eski ve tanımsız. Oyuncu dışarıdan içeriye doğru ilerler.

### Deneme Odası Tanımı
Mevcut `LevelData` SO genişletilir → `TrialChamberData`:
- `chamberNumber` (1-100)
- `biomeLayer` (6 biome enum)
- `isUnlocked` → SaveData'dan okunur
- `isCompleted` → SaveData'dan okunur
- `pressureRating` (1-5 basınç göstergesi)

## Ekipman / Envanter

### 6 Slot
| Slot | Stat Etkisi |
|------|-------------|
| Cranial Module | HP, özel efekt |
| Carapace Plate | Armor, Damage Reduction |
| Appendage Core | Base Damage, Attack Speed |
| Sensory Array | Crit Chance, Crit Damage |
| Locomotion Frame | Move Speed, Dash CD |
| Auxiliary Port | Herhangi stat |

### Komponent Bütünlüğü (Rarity)
| Sınıf | Renk | Stat Range |
|--------|------|-----------|
| Baseline | Gri | +5-10% |
| Calibrated | Yeşil | +10-20% |
| Reinforced | Mavi | +20-35% |
| Anomalous | Mor | +35-50% |
| Architect-Grade | Altın | +50-100% |

### Materyal Kaynakları
- Wave düşmanları: Baseline/Calibrated
- Elite: Calibrated/Reinforced
- Stabilized: Reinforced/Anomalous garantili, Architect-Grade şansı
- Yüksek basınç odası = daha yüksek bütünlük materyali

## Kalibrasyon Ağacı

### 4 Dal, ~40 Node
| Dal | Node'lar |
|-----|----------|
| Substrate Density | +5% DMG, +2% Crit, +10% Crit DMG, +5% AtkSpd... |
| Structural Integrity | +10% HP, +3 Armor, +2% Regen, +5% Heal... |
| Signal Conductivity | +5% Speed, -10% Dash CD, +3% Dodge, +1 Dash... |
| Data Yield | +10% Fragment, +5% Drop Rate, +10% XP, +5% Rare Drop... |

### Point Kaynağı
- Her kalibrasyon seviyesi: +1 kalibrasyon noktası
- Bazı Stabilized ilk yenilgisi: +1 bonus nokta

## Save Sistemi

### SaveData Genişletme
```
SubjectFileData:
  - designation (suffix — e.g., "ARDENT")
  - substrateConfig (0-3)
  - calibrationLevel
  - adaptationData (XP equivalent)
  - calibrationPoints[5] (MASS/RESILIENCE/VELOCITY/VARIANCE/YIELD)
  - equippedComponents[6] (slot → component ID)
  - inventoryComponents[] (tüm sahip olunan komponentler)
  - calibrationNodes[] (açılmış node ID'leri)
  - completedChambers[] (tamamlanmış oda numaraları)
  - currentFragments (meta kaynak)
  - signalArchiveEntries[] (açılmış lore fragment ID'leri)
```

## Implementation Sırası

| Sprint | İçerik |
|--------|--------|
| 14 | MainMenu redesign + Substrate Konfigürasyon Seçimi + Save/Load overhaul |
| 15 | Arena Haritası UI + Oda select + Oda unlock sistemi |
| 16 | Envanter + Ekipman + Komponent drop sistemi |
| 17 | Kalibrasyon Ağacı + Kalibrasyon noktası dağıtımı |
| 18 | 100 oda tanımlama + balans + polish |
