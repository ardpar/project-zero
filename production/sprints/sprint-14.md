# Sprint 14 — 2026-08-07 to 2026-08-21

## Sprint Goal
Trial Chamber sistemi: mevcut level-based yapıyı multi-room run'a dönüştür, basınç (pressure) scaling ekle, biome katman sistemi kur. Sprint sonunda: oyuncu oda-oda ilerleyebilir, her oda artan basınçla zorlaşır, biome'lar görsel olarak ayrışır.

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-15)
- Sprint 11: Level/Stage + Gold ✅
- Sprint 12: WorldMap + Character + Save/Load + Items + Inventory ✅
- Sprint 13: Skill Tree + Loot + Stat Points + Craft + Shop ✅
- Sprint 14: Trial Chamber + Biome + Pressure Scaling ← **Buradayız**

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S14-01 | **TrialChamberData SO** — Mevcut LevelData'yı genişlet: `chamberNumber` (1-100), `biomeLayer` enum (6 biome), `pressureRating` (1-5), `adjacentChambers[]` (unlock zinciri). İlk 16 oda tanımlanır (The Atrium biome) | 1.0 | — | 16 oda SO oluşturulmuş, Inspector'da düzenlenebilir, biome/pressure bilgisi var |
| S14-02 | **TrialManager** — Yeni MonoBehaviour: oda geçişi orkestratörü. Boss (Stabilized) yenilince run bitirmez → "Kalibrasyon Aralığı" ekranı göster → sonraki oda seç veya haritaya dön. `WaveSpawner` ile entegrasyon: Stabilized yenilgisinde event fire | 1.5 | S14-01 | Boss yenilince run bitmez, geçiş ekranı gösterilir, sonraki odaya devam edilebilir |
| S14-03 | **Basınç (Pressure) Scaling** — Formül: `base_stat * (1 + chamber * 0.3)`. Düşman HP, damage, spawn count, wave hızı basınca göre scale edilir. WaveSpawner + EnemyBrain basınç çarpanı alır | 1.0 | S14-01 | Oda 5'teki düşmanlar oda 1'den belirgin şekilde güçlü, formül doğru uygulanır |
| S14-04 | **Biome Enum + Görsel Tema** — 6 biome enum: Atrium, AssayChambers, DeepArchive, CollapseStratum, CorruptionLayer, NullChamber. Her biome için background renk/tint. Gameplay scene biome'a göre görsel değişim (background color, tint) | 1.0 | S14-01 | Farklı biome odalarında görsel fark belirgin |
| S14-05 | **Arena Map UI Evrimi** — Mevcut WorldMap level grid'ini 10x10 grid'e dönüştür. Oda durumları: kilitli (koyu), açık (parlak), tamamlanmış (mavi izi). Biome bölgeleri renk-kodlu. Oda tıklayınca bilgi popup + "Denemeye Başla" butonu | 2.0 | S14-01, S14-04 | 100 oda grid görünür, biome renkleri belirgin, oda seçimi çalışır |
| S14-06 | **Kalibrasyon Aralığı Ekranı** — Oda tamamlanınca gösterilen ara ekran: kazanılan loot listesi, "Sonraki Oda" + "Haritaya Dön" butonları, basit envanter erişimi. TrialManager'dan trigger | 1.0 | S14-02 | Oda bitiminde ara ekran gösterilir, loot gösterilir, iki seçenek çalışır |

**Toplam Must Have: 7.5 gün** (kapasite: 8 gün — 0.5 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S14-07 | **Oda tamamlama → bitişik odaları unlock** — Oda tamamlayınca `adjacentChambers[]` listesindeki odalar açılır, SaveData güncellenir | 0.5 | S14-05 | Tamamlanan odanın komşuları haritada açılır |
| S14-08 | **Arena terminolojisi UI güncellemesi** — "Level" → "Deneme Odası", "Boss" → "Stabilized", "Gold" → "Substrate Fragment" UI metinlerinde. HUD, WorldMap, Level Complete ekranları | 0.5 | — | Tüm UI metinleri Arena terminolojisini kullanır |
| S14-09 | **Basınç yıldızları Arena Map'te** — Her oda butonunda ★ ile basınç seviyesi gösterilir (1-5) | 0.25 | S14-05 | Basınç seviyesi görsel olarak belirgin |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S14-10 | **Biome geçiş animasyonu** — Farklı biome'a geçerken kısa fade/transition efekti | 0.25 | S14-04 | Biome geçişi yumuşak hissettiriyor |
| S14-11 | **Oda bilgi popup detayları** — Biome ismi, basınç, düşman önizleme, tahmini zorluk | 0.5 | S14-05 | Popup'ta oda hakkında bilgi gösterilir |
| S14-12 | **Substrate Fragment kaynak dönüşümü** — Gold → Substrate Fragment: mevcut gold sistemini fragment'a rename, drop miktarlarını biome/pressure'a göre scale et | 0.5 | S14-03 | Fragment miktarı basınca göre artar |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| VFX polish (HitStop, DeathBurst, DamageNumbers) | Uncommitted WIP from Sprint 13 polish | 0 gün (commit only) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| 100 oda grid UI performansı | Düşük | 0.5 gün | Object pooling, sadece görünür odaları render et |
| WaveSpawner → TrialManager entegrasyonu karmaşık olabilir | Orta | 1 gün | Mevcut WaveSpawner event'lerini kullan, minimal refactor |
| Multi-room run state management | Orta | 0.5 gün | TrialManager tek sorumlu, SaveData'ya her oda sonunda yaz |
| 16 oda tanımlamak zaman alabilir | Düşük | 0.25 gün | Template SO kullan, basınç + wave table parametrik |

## Dependencies on External Factors
- Mevcut WaveSpawner boss yenilgisi event desteği (✅ zaten mevcut — wave complete event)
- WorldMap scene ve grid sistemi (✅ Sprint 12'den mevcut)
- LevelData SO yapısı (✅ genişletilebilir)
- SaveData completed levels alanı (✅ mevcut, chambers'a genişletilecek)

## Definition of Done for this Sprint
- [ ] TrialChamberData SO ile 16 oda (Atrium biome) tanımlı
- [ ] Boss yenilince oda bitmez, Kalibrasyon Aralığı ekranı gösterilir
- [ ] Sonraki odaya devam edilebilir (multi-room run)
- [ ] Basınç scaling formülü çalışır (oda 5 belirgin şekilde daha zor)
- [ ] Arena Map 100 oda grid gösterir, biome renkleri belirgin
- [ ] Tamamlanan oda bitişik odaları açar
- [ ] Haritaya dönüş çalışır
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
