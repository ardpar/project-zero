# Sprint 15 — 2026-08-21 to 2026-09-04

## Sprint Goal
Arena kaynak ekonomisi: Substrate Fragment dönüşümü, basınca göre loot/materyal scaling, Arena terminolojisi UI genişletme, Assay Chambers (biome 2) oda içeriği, Trial Chamber akışı bugfix + integration test. Sprint sonunda: tüm ekonomi Arena narrative'ine uyumlu, basınç loot'u etkiliyor, 33 oda oynanabilir.

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-18)
- Sprint 11: Level/Stage + Gold ✅
- Sprint 12: WorldMap + Character + Save/Load + Items + Inventory ✅
- Sprint 13: Skill Tree + Loot + Stat Points + Craft + Shop ✅
- Sprint 14: Trial Chamber + Biome + Pressure Scaling ✅
- Sprint 15: Resource Economy + Arena Terminology + Biome 2 Content ← **Buradayız**

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S15-01 | **Substrate Fragment dönüşümü** — `GoldManager` → `FragmentManager` rename. Gold → Substrate Fragment tüm kodda. Drop miktarları basınca göre scale: `base * (1 + pressure * 0.25)`. CharacterSaveData.gold alanı `substrateFragments` olarak rename | 1.0 | — | Fragment kazanılır, UI'da "Substrate Fragment" gösterilir, basınca göre artar |
| S15-02 | **Materyal drop pressure scaling** — `CraftingManager.AwardMaterials` basınç parametresi alsın. Yüksek basınç = daha fazla scrapMetal/darkCrystal şansı. Scrap → "Residual Compound", Dark Crystal → "Mutation Residue", Boss Essence → "Stabilized Core" rename | 1.0 | S15-01 | Oda 10'da materyal düşme oranı oda 1'den belirgin yüksek |
| S15-03 | **Loot rarity pressure scaling** — `LootDropper` basınç bilgisini alsın (TrialManager veya PlayerPrefs üzerinden). Yüksek basınç odalarında Rare+ drop şansı artar. Formül: `baseChance * (1 + pressure * 0.15)` | 0.5 | — | Basınç 3+ odalarda Epic item görülme sıklığı artmış |
| S15-04 | **Arena terminolojisi UI güncellemesi** — Tüm UI'da: "Level" → "Deneme Odası", "Boss" → "Stabilized", "Gold" → "Substrate Fragment", "Wave" → "Dalga". PrototypeHUD, WorldMapScreen, ArenaMapScreen, LevelTransitionScreen, CharacterScreen, ShopScreen, CraftScreen | 1.5 | S15-01 | Oyunun hiçbir yerinde eski terminoloji kalmamış |
| S15-05 | **Assay Chambers odaları (17-33)** — 17 TrialChamberData SO: biome=AssayChambers, pressure 2-3, daha zor düşman havuzları (tank, poisoner, charger ağırlıklı), farklı boss rotasyonu. Adjacency: oda 16 → 17 bağlantısı (biome geçişi) | 1.5 | — | 17 yeni oda tanımlı, Assay Chambers biome rengiyle gösterilir, oda 16'dan geçiş çalışır |
| S15-06 | **Trial Chamber akış testi + bugfix** — Tam akışı test et: Arena Map → oda seç → gameplay → boss yenilgisi → Kalibrasyon Aralığı → sonraki oda → haritaya dönüş. SaveData doğru yazılıyor mu? Oda unlock zinciri çalışıyor mu? Multi-room run state doğru mu? | 1.5 | S15-05 | Tam akış 3 oda boyunca kesintisiz çalışır, save/load sonrası ilerleme korunur |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1.0 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S15-07 | **Kalibrasyon Aralığı envanter erişimi** — Kalibrasyon ekranında basit envanter butonu: item equip/unequip yapılabilir | 0.5 | S15-06 | Odalar arasında ekipman değiştirilebilir |
| S15-08 | **Biome geçiş efekti** — Atrium → Assay Chambers geçişinde kısa fade-to-black transition | 0.25 | S15-05 | Biome geçişi yumuşak |
| S15-09 | **Oda bilgi popup genişletme** — ArenaMapScreen popup'ına: düşman tipi ikonu, tahmini süre, biome lore açıklaması | 0.5 | S15-05 | Popup'ta oda hakkında zengin bilgi var |
| S15-10 | **HUD'da oda numarası + biome ismi** — PrototypeHUD'a chamber bilgisi: "Deneme Odası 7 — The Atrium" | 0.25 | S15-04 | HUD'da hangi odada olduğu görünür |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S15-11 | **Materyal toplama pop-up** — DamageNumber benzeri floating text: "+1 Residual Compound" düşman öldürünce | 0.25 | S15-02 | Materyal kazanımı görsel olarak belli |
| S15-12 | **Substrate Konfigürasyon rename** — CharacterCreation'da sınıf isimleri narrative'e uyumlu hale getir: Warrior → Dense Lattice, Rogue → Severed Thread, Mage → Null Cascade, Sentinel → Balanced Frame | 0.5 | S15-04 | Karakter yaratma ekranında Arena terminolojisi |
| S15-13 | **Craft ekranı materyal rename** — CraftScreen'de yeni materyal isimleri: Residual Compound, Mutation Residue, Stabilized Core | 0.25 | S15-02 | Craft ekranı narrative terminolojisi kullanır |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| S14-08: Arena terminolojisi UI güncellemesi | Should Have olarak planlanmıştı, yapılamadı | S15-04'e dahil edildi |
| S14-10: Biome geçiş animasyonu | Nice to Have, yapılamadı | S15-08'e dahil edildi |
| S14-11: Oda bilgi popup detayları | Nice to Have, kısmen yapıldı | S15-09'a dahil edildi |
| S14-12: Substrate Fragment dönüşümü | Nice to Have, yapılamadı | S15-01'e dahil edildi (Must Have) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Gold → Fragment rename geniş kapsamlı (çok dosya etkiler) | Orta | 1 gün | Arama-değiştir + derleme testi, CharacterSaveData backward compat |
| Trial Chamber akışında beklenmeyen bug'lar | Yüksek | 1.5 gün | S15-06 için yeterli zaman ayrıldı, buffer mevcut |
| Eski save dosyaları yeni field adlarıyla uyumsuz | Orta | 0.5 gün | Migration: gold → substrateFragments JsonUtility ile fallback |
| 17 oda tanımlama süresi | Düşük | 0.25 gün | Script ile batch oluşturma (Sprint 14'te kanıtlanmış) |

## Dependencies on External Factors
- Sprint 14 Trial Chamber sistemi (✅ commit c44a143d)
- Mevcut GoldManager/CraftingManager altyapısı (✅)
- 16 Atrium TrialChamberData SO'ları (✅)
- 6 BiomeConfig SO'ları (✅)
- Mevcut 30 item + 5 boss varlığı (✅)

## Definition of Done for this Sprint
- [ ] Gold → Substrate Fragment dönüşümü tüm kodda tamamlanmış
- [ ] Materyal isimleri narrative-aligned (Residual Compound, Mutation Residue, Stabilized Core)
- [ ] Basınca göre loot/materyal drop artışı çalışır
- [ ] 33 oda (Atrium 16 + Assay Chambers 17) oynanabilir
- [ ] Biome 1 → Biome 2 geçişi çalışır
- [ ] Arena terminolojisi tüm UI ekranlarında
- [ ] Tam Trial Chamber akışı 3+ oda boyunca kesintisiz
- [ ] Save/load sonrası oda ilerlemesi korunur
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
