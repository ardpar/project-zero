# Sprint 16 — 2026-09-04 to 2026-09-18

## Sprint Goal
Sentez sistemi genişletme, rarity narrative rename (Baseline→Architect-Grade), Kalibrasyon Aralığı'nda sentez/tedarik, Deep Archive (biome 3) odaları, Kalibrasyon Ağacı narrative rename. Sprint sonunda: sentez formülleri expansion-vision'a uyumlu, 50 oda oynanabilir, rarity sistemi Arena terminolojisinde.

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-18)
- Sprint 11: Level/Stage + Gold ✅
- Sprint 12: WorldMap + Character + Save/Load + Items + Inventory ✅
- Sprint 13: Skill Tree + Loot + Stat Points + Craft + Shop ✅
- Sprint 14: Trial Chamber + Biome + Pressure Scaling ✅
- Sprint 15: Resource Economy + Arena Terminology + Biome 2 ✅
- Sprint 16: Sentez Genişletme + Rarity Rename + Biome 3 ← **Buradayız**

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S16-01 | **Rarity narrative rename** — `ItemRarity` enum: Common→Baseline, Uncommon→Calibrated, Rare→Reinforced, Epic→Anomalous, Legendary→Architect-Grade. Tüm UI rarity referansları güncellenir. RarityColor palette: Gri/Yeşil/Mavi/Mor/Altın. 30 mevcut item SO'su yeni rarity isimleriyle | 1.5 | — | Rarity her yerde Arena terminolojisiyle gösterilir, renkler doğru |
| S16-02 | **Sentez formülleri genişletme** — Expansion-vision sentez mekanikleri: 3x Residual Compound → Baseline Komponent, 2x Mutation Residue + 1x Compound → Reinforced Komponent, 1x Stabilized Core + 3x Residue → Architect-Grade (Stabilized'a özgü). CraftingManager + CraftScreen güncelleme | 1.0 | S16-01 | 3 sentez tarifi çalışır, Architect-Grade sentezi boss-specific |
| S16-03 | **Komponent birleştirme** — İki aynı rarity komponent birleştirilerek bir üst rarity'ye yükseltilebilir. UI: envanterde iki item seç → "Birleştir" butonu. Stat'lar average + %10 bonus | 1.5 | S16-01 | İki Baseline birleşince Calibrated olur, stat'lar combine |
| S16-04 | **Kalibrasyon Aralığı sentez + tedarik** — CalibrationIntervalScreen'e sentez ve shop butonları ekle. Odalar arasında sentez yapılabilir, material satın alınabilir (fragment ile) | 1.0 | S16-02 | Kalibrasyon Aralığı'nda sentez ve tedarik erişilebilir |
| S16-05 | **Deep Archive odaları (34-50)** — 17 TrialChamberData SO: biome=DeepArchive, pressure 3-4, ağır düşman kompozisyonu, yeni boss rotasyonu. Oda 33→34 köprüsü. Biome 3 görsel tema uygulanmış | 1.5 | — | 17 yeni oda, biome 3 renkleri, oda 33'ten geçiş çalışır |
| S16-06 | **Kalibrasyon Ağacı narrative rename** — SkillTreeData + SkillTreeScreen: dal isimleri → Substrate Density, Structural Integrity, Signal Conductivity, Data Yield. Node açıklamaları Arena terminolojisiyle | 0.5 | — | Skill tree UI'da Arena terminolojisi |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1.0 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S16-07 | **Equipment slot narrative rename** — Helmet→Cranial Module, Armor→Carapace Plate, Weapon→Appendage Core, Gloves→Sensory Array, Boots→Locomotion Frame, Accessory→Auxiliary Port. ItemData + CharacterScreen güncelleme | 0.5 | S16-01 | Ekipman slotları Arena isimleriyle gösterilir |
| S16-08 | **Sentez animasyonu** — Sentez yapılınca kısa pulse + parçacık efekti | 0.25 | S16-02 | Sentez tatmin edici hissettiriyor |
| S16-09 | **Rarity border glow iyileştirmesi** — Envanter ve ekipman slotlarında rarity'ye göre animated glow | 0.25 | S16-01 | Anomalous ve Architect-Grade item'lar görsel olarak etkileyici |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S16-10 | **Stabilized-specific Architect-Grade** — Her boss tipi yenilince benzersiz Architect-Grade item drop. 5 boss = 5 unique legendary | 0.5 | S16-02 | Her boss'un kendine özgü item'ı var |
| S16-11 | **Sentez tarif keşfi** — İlk sentez yapılınca "yeni tarif keşfedildi" bildirimi, keşfedilen tarifler listesi | 0.5 | S16-02 | Sentez tarif keşfi çalışır |
| S16-12 | **Stat point narrative rename** — STR→MASS, VIT→RESILIENCE, AGI→VELOCITY, LCK→VARIANCE, WIS→YIELD | 0.25 | — | Stat point dağıtımında Arena terminolojisi |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| — | Sprint 15 tamamen tamamlandı (Must Have + Should Have + Nice to Have) | Carryover yok |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| ItemRarity enum rename mevcut save dosyalarını bozabilir | Yüksek | 1 gün | Serialized int olarak saklanıyor, enum sırası korunursa sorun yok. Sıra: 0=Baseline, 1=Calibrated, 2=Reinforced, 3=Anomalous, 4=Architect-Grade (eski sırayla aynı) |
| Komponent birleştirme UI karmaşıklığı | Orta | 0.5 gün | Basit "iki item seç + birleştir" modal, drag-drop gerekmez |
| 17 oda tanımlama (Deep Archive) | Düşük | 0.25 gün | Script batch oluşturma (Sprint 14-15'te kanıtlanmış) |
| Sentez tarif balansı | Orta | 0.5 gün | Expansion-vision formüllerini takip et, playtest ile ayarla |

## Dependencies on External Factors
- Sprint 15 FragmentManager + CraftingManager (✅ commit 02756a58)
- 33 TrialChamberData SO (✅)
- 6 BiomeConfig SO (✅ Deep Archive zaten tanımlı)
- 30 mevcut item + 5 boss (✅)
- SkillTreeData SO (✅ Sprint 13'ten)
- CalibrationIntervalScreen inventory erişimi (✅ Sprint 15 polish)

## Definition of Done for this Sprint
- [ ] Rarity isimleri: Baseline/Calibrated/Reinforced/Anomalous/Architect-Grade tüm UI'da
- [ ] 3 sentez tarifi çalışır (Baseline, Reinforced, Architect-Grade)
- [ ] Komponent birleştirme: iki aynı rarity → bir üst rarity
- [ ] Kalibrasyon Aralığı'nda sentez ve tedarik erişilebilir
- [ ] 50 oda oynanabilir (Atrium 16 + Assay 17 + Deep Archive 17)
- [ ] Biome 2→3 geçişi çalışır
- [ ] Kalibrasyon Ağacı Arena terminolojisiyle
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
