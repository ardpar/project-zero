# SYNTHBORN — Expansion Vision: Roguelite ARPG Evolution

> **Status**: Draft (Narrative-Aligned v2)
> **Created**: 2026-03-29
> **Narrative Framework**: Option A — "Arena Remembers, Subject Doesn't"

## Overview

SYNTHBORN'u basit survivor'dan **çok katmanlı roguelite ARPG**'ye evrimleştirme planı. Mevcut core loop (hareket + otomatik saldırı + mutasyonlar) korunur, üstüne derinlik katmanları eklenir.

### Narrative Çerçeve

Arena, SUBJECT-FINAL'ın her denemesini kaydeder. Subject ölür ve sıfırlanır — hafızası silinir, bedeni baseline'a döner. Ama **Arena hatırlar.** Meta-progression, Arena'nın önceki denemelerden öğrendiklerini uygulamasıdır. Oyuncunun "güçlenmesi" aslında Arena'nın deneyini kalibre etmesidir.

> "Sen hatırlamıyorsun. Ama sistem seni izliyor, öğreniyor ve bir sonraki döngüyü buna göre ayarlıyor."

---

## Yeni Core Loop

```
Ana Menü → Substrate Konfigürasyonu Seç → Deneme Odası 1 Start
  │
  ├─ Wave 1-5 (düşmanlar, materyal düşer)
  ├─ Stabilized Karşılaşma (boss — lore fragment dropu)
  ├─ Oda Tamamlandı → Kalibrasyon Aralığı (inventory/tedarik ekranı)
  │
  ├─ Deneme Odası 2 (sonraki biome katmanı, artan basınç)
  ├─ Wave 1-5 + Stabilized
  ├─ Oda Tamamlandı → Kalibrasyon Aralığı
  │
  ├─ Deneme Odası 3, 4, 5... (Arena basıncı katlanır)
  │
  └─ Ölüm → Iteratif Kalibrasyon → Arena verileri günceller → Sonraki Döngü
```

**Lore notu:** Level geçiş ekranı ("Kalibrasyon Aralığı") Arena'nın Subject'i yeniden değerlendirip ortamı ayarladığı kısa bir duraklamadır. Bu bir coğrafi geçiş değil, sistemin nefes almasıdır.

---

## Sistem 1: Deneme Odası Progression

**Mevcut**: 1 run = 6 wave + boss → biter
**Yeni**: 1 run = Oda 1 (5 wave + Stabilized) → Oda 2 (5 wave + Stabilized) → ... Arena basıncı arttıkça

### Detaylar
- Her oda bir `TrialChamberData` ScriptableObject: biome katmanı, düşman havuzu, Stabilized tipi, basınç çarpanı
- Oda geçişinde Kalibrasyon Aralığı: materyal toplama, envanter düzenleme, tedarik
- Basınç formülü: `base_pressure * (1 + chamber * 0.3)` — her oda %30 daha yoğun
- Biome sırası (Arena çağları): The Atrium → The Assay Chambers → The Deep Archive → The Collapse Stratum → The Corruption Layer → tekrar (artan basınç)
- Her odanın kendi Stabilized'ı: farklı terminal mutasyon yolunu temsil eden eski Subject'ler

### Mimari Değişiklik
- `WaveSpawner` → Stabilized yenilince run bitirmez, `TrialManager`'a bildirir
- `TrialManager` yeni component: oda geçişi, biome katman swap, basınç scaling
- `WaveTableData` per-chamber veya dinamik scaling ile

---

## Sistem 2: Envanter + Ekipman

### Lore Çerçeve
Ekipman, Arena'nın önceki denemelerden çıkardığı ve sonraki döngülere yerleştirdiği "kalibrasyon araçlarıdır." Subject bunları "buluyor" ama aslında Arena tarafından kasıtlı olarak yerleştirilmiştir.

### Slot Sistemi
| Slot | Etki |
|------|------|
| Silah | Base damage, attack speed, özel efekt |
| Zırh | HP bonus, armor, damage reduction |
| Modül 1 | Pasif bonus (crit, speed, XP gain vb.) |
| Modül 2 | Pasif bonus |

### Komponent Yapısı
```
ComponentData (ScriptableObject):
  - id, designation, logEntry (klinik açıklama)
  - integrity: Baseline / Calibrated / Reinforced / Anomalous / Architect-Grade
  - slotType: Weapon / Armor / Module
  - statModifiers: HP, Speed, Damage, Crit, Armor, AttackSpeed
  - specialEffect: string (özel mekanik ID)
  - icon: Sprite
```

### Bütünlük Sınıflandırması (Rarity)
| Sınıf | Drop Chance | Stat Bonus | Arena Rengi |
|-------|-------------|------------|-------------|
| Baseline | 50% | +5-10% | Gri |
| Calibrated | 30% | +10-20% | Yeşil |
| Reinforced | 15% | +20-35% | Mavi |
| Anomalous | 4% | +35-50% | Mor |
| Architect-Grade | 1% | +50-100% | Altın |

**Lore notu:** "Architect-Grade" — Arena'nın kendi üretemediği, Mimar döneminden kalan nadir komponent. Bunların varlığı bile bir lore parçasıdır.

### Kaynak
- Normal düşmanlardan: Baseline/Calibrated komponent + kaynak
- Elite düşmanlardan: Calibrated/Reinforced komponent
- Stabilized'dan: Reinforced/Anomalous/Architect-Grade (garantili 1 drop)
- Mini-Stabilized: Calibrated/Reinforced garantili

---

## Sistem 3: Kaynak Ekonomisi

### Lore Çerçeve
"Altın" yoktur — Arena bir ekonomi üretmez. Bunun yerine Subject'in topladığı ham materyaller, Arena'nın kalibrasyon sürecinin yan ürünleridir.

### Kazanma
- Her düşman öldürmede: 1-5 **Substrate Fragment** (tier'e göre)
- Wave tamamlama bonusu: 10-50 fragment (oda'ya göre)
- Stabilized yenme bonusu: 100+ fragment

### Harcama
- Kalibrasyon Aralığı: komponent tedarik, onarım
- Sentez maliyetleri
- Pasif konfigürasyon unlock

### Kalıcılık (Arena Hafızası)
- Fragment'lar run içi harcama (tedarik, sentez)
- Run sonunda kalan fragment'ın %20'si Arena'nın **Kalibrasyon Kaydı**'na işlenir (kalıcı)
- **Lore notu:** Bu, Arena'nın Subject'ten topladığı veridir. Subject hatırlamaz ama Arena bu veriyi sonraki döngüye yansıtır.

---

## Sistem 4: Sentez Sistemi

### Materyal Tipleri
| Materyal | Kaynak | Kullanım |
|----------|--------|----------|
| Substrate Fragment | Normal düşman dropu | Baseline sentez |
| Mutation Residue | Elite/Stabilized dropu | Reinforced+ sentez |
| Stabilized Core | Stabilized kill (unique per boss) | Architect-Grade sentez |

### Sentez Mekanikleri
- 3x Substrate Fragment → Random Baseline Komponent
- 2x Mutation Residue + 1x Fragment → Random Reinforced Komponent
- 1x Stabilized Core + 3x Mutation Residue → Specific Architect-Grade (Stabilized'a göre)
- İki komponent birleştirme: stat'ları combine et (daha güçlü tek komponent)

### Sentez UI
- Kalibrasyon Aralığı'nda veya envanterden erişim
- Sentez şemaları (keşfedilen formüller)

---

## Sistem 5: Kalibrasyon Ağacı (Pasif Skill Tree)

### Lore Çerçeve
Bu ağaç, Arena'nın SUBJECT-FINAL için uyguladığı **çevresel kalibrasyon parametreleridir.** Oyuncu node açtığında, aslında Arena önceki denemelerden öğrendiği bir optimizasyonu sonraki döngüye yerleştiriyor.

### Yapı
Halka şeklinde ağaç — ortadan dışa doğru dallanır.

### Dal Tipleri
| Dal | Odak | Örnek Pasifler |
|-----|------|----------------|
| **Substrate Density** | Damage + Crit | +5% DMG, +2% Crit, +10% Crit DMG |
| **Structural Integrity** | HP + Armor | +10% HP, +3 Armor, +5% Regen |
| **Signal Conductivity** | Speed + Dash | +5% Speed, -10% Dash CD, +1 Dash Charge |
| **Data Yield** | XP + Loot | +10% XP, +5% Drop Rate, +10% Fragment |

### Unlock Mekanığı
- Kalibrasyon Kaydı (meta kaynak) ile node'ları aç
- Her node 1 kez alınır, kalıcı
- Toplam ~30-40 node, hepsini açmak uzun vadeli hedef
- **Lore notu:** Her unlock, Arena'nın "bu parametre verimli sonuç üretir" kararıdır

---

## Sistem 6: Run İçi Adaptasyon (Stat Points)

**Mevcut**: Level-up → mutasyon seç
**Yeni**: Level-up → mutasyon seç + adaptasyon noktası kazan

### Adaptasyon Noktası Dağıtımı
- Her level-up'ta 1 adaptasyon noktası
- 5 parametreye dağıt: MASS (damage), RESILIENCE (HP), VELOCITY (speed), VARIANCE (crit), YIELD (XP)
- Noktalar run içi, ölümde Arena sıfırlar
- Mutasyon seçimi de devam eder (ikisi birlikte)

---

## Öncelik Sırası (Sprint Planı)

| Sıra | Sistem | Neden Önce |
|------|--------|------------|
| 1 | **Deneme Odası Progression** | Tüm diğer sistemlerin temeli |
| 2 | **Kaynak + Komponent Drop** | Odalar arası motivasyon |
| 3 | **Envanter + Ekipman** | Drop'ları kullanabilmek için |
| 4 | **Stabilized Loot + Sentez** | Envanter üstüne derinlik |
| 5 | **Kalibrasyon Ağacı** | Uzun vadeli meta-progression |
| 6 | **Adaptasyon Noktaları** | Level-up'a ek katman |

### Tahmini Sprint Dağılımı
- Sprint 14: Deneme Odası + Kaynak drop
- Sprint 15: Envanter + Ekipman + Stabilized Loot
- Sprint 16: Sentez + Kalibrasyon Aralığı tedarik
- Sprint 17: Kalibrasyon Ağacı + Adaptasyon Noktaları
- Sprint 18: Polish + Balance + Content (yeni Stabilized'lar, komponentler)
