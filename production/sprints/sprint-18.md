# Sprint 18 — 2026-04-15 to 2026-04-29

## Sprint Goal
Roguelite ARPG Evolution milestone'ını kapatma: balance pass, TextMeshPro migration, runtime profiling baseline, adaptasyon balance, audio entegrasyonu ve milestone gate check. Sprint sonunda: tüm sistemler entegre ve balanslanmış, performans bütçesi ölçülmüş, milestone tamamlanmış.

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-18)
- Sprint 11: Level/Stage + Gold ✅
- Sprint 12: WorldMap + Character + Save/Load + Items + Inventory ✅
- Sprint 13: Skill Tree + Loot + Stat Points + Craft + Shop ✅
- Sprint 14: Trial Chamber + Biome + Pressure Scaling ✅
- Sprint 15: Resource Economy + Arena Terminology + Biome 2 ✅
- Sprint 16: Sentez Genişletme + Rarity Rename + 100 Oda + 6 Biome ✅
- Sprint 17: Performance + Adaptasyon Noktaları + Test Altyapısı ✅
- Sprint 18: Polish + Balance + Milestone Kapatma ← **Buradayız**

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S18-01 | **Balance pass: 100 oda + 6 biome** — Pressure scaling eğrisi doğrulama (oda 1-100 arası zorluk artışı düzgün mü?). Düşman kompozisyonu her biome'da tematik. Drop rate'ler biome'a göre ayarlanmış. 10 tam run playtest, notlar alınacak. Kırık veya "ölü" odalar tespit edilecek | 2.0 | — | 10 run'da hiçbir oda geçilemez değil, her biome zorluk artışı hissedilir, drop rate'ler rarity dağılımına uygun |
| S18-02 | **Adaptasyon Noktaları balance + entegrasyon** — 5 parametre (MASS/RESILIENCE/VELOCITY/VARIANCE/YIELD) etki formülleri playtest ile ince ayar. Hiçbir parametre diğerlerini domine etmemeli. AdaptationPointManager → SaveManager entegrasyonu: run-içi state save/load (kalibrasyon aralığında save yapılınca adaptasyon noktaları da kaydedilmeli) | 1.5 | — | 10 test run'da parametre dağılımı dengeli, save/load'da adaptasyon noktaları korunuyor |
| S18-03 | **TextMeshPro migration (L6)** — `UnityEngine.UI.Text` → `TextMeshProUGUI` tüm HUD ve in-game text bileşenlerinde. PrototypeHUD, KillCounter, LevelHUD, GoldHUD, FragmentHUD, WaveBanner, DamageNumber. Font atlas kurulumu, SDF font oluşturma. Legacy Text referansları kaldırma | 1.0 | — | Tüm oyun-içi text'ler TMP ile render ediliyor, blurry text yok, draw call azalmış |
| S18-04 | **Runtime profiling baseline** — Unity Profiler ile gerçek gameplay ölçümü (wave 1, wave 5, boss fight, 200 düşman sahnesi). Frame-time budget tablosu doldurma. Memory ceiling belirleme. `technical-preferences.md` güncelleme. Sprite Atlas Analyzer ile draw call analizi | 1.5 | — | Frame budget tablosu gerçek verilerle doldurulmuş, memory ceiling belirlenmiş, draw call sayısı belgelenmiş |
| S18-05 | **Milestone gate check** — Tüm Sprint 11-18 deliverable'ları gözden geçirme. Kalan bug'lar triajlanmış. Eksik GDD güncelemeleri tamamlanmış. ADR'ler yazılmış (en az pool sistemi + adaptation points). Perf report güncellenmiş. Sonraki milestone planı taslağı | 1.0 | S18-01, S18-04 | Gate check raporu yazılmış, 0 S1/S2 bug, tüm Must Have feature'lar çalışıyor |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1.0 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S18-06 | **Audio entegrasyonu: biome müzikleri + boss müziği** — AdaptiveMusicManager'a 6 biome stem ekleme. Boss karşılaşmasında müzik geçişi. Biome değişiminde crossfade. Mevcut SFXManager ile sentez/craft efektleri | 1.0 | — | Her biome'da farklı müzik, boss fight'ta tansion artışı, sentez SFX |
| S18-07 | **Sentez balance pass** — 3 sentez formülü (Baseline/Reinforced/Architect-Grade) playtest ile doğrulama. Komponent birleştirme stat dengesi. Kalibrasyon Aralığı ekonomisi: tedarik fiyatları vs. kazanç hızı | 0.5 | S18-01 | Sentez ödüllendirici ama broken değil, fragment ekonomisi sürdürülebilir |
| S18-08 | **Equipment slot balance** — 6 slot (Cranial Module, Carapace Plate, Appendage Core, Sensory Array, Locomotion Frame, Auxiliary Port) stat etkileri dengeli. Boss-specific Architect-Grade item'lar güçlü ama run'ı kırmıyor | 0.5 | S18-01 | Hiçbir slot combination degenerate strateji oluşturmuyor |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S18-09 | **Kalibrasyon Ağacı balance** — 4 dal (~40 node) maliyet/etki dengesi. Erken node'lar ucuz ve hissedilir, geç node'lar pahalı ve güçlü. Toplam unlock süresi 20-30 tam run | 0.5 | — | Node maliyet eğrisi logaritmik, her dal viable |
| S18-10 | **UI polish pass** — Button hover efektleri tutarlı, ekran geçişleri fade ile, popup'lar animasyonlu, rarity renkleri tüm ekranlarda tutarlı | 0.5 | S18-03 | Tüm ekranlarda tutarlı visual language |
| S18-11 | **Ek unit testler** — Sentez formülleri, LootDropper rarity dağılımı, pressure scaling testleri. Hedef: 60+ toplam test | 0.5 | — | 60+ test geçiyor |
| S18-12 | **Stat point narrative rename** — STR→MASS, VIT→RESILIENCE, AGI→VELOCITY, LCK→VARIANCE, WIS→YIELD. Sprint 16'dan kalan Nice to Have (S16-12) | 0.25 | — | Stat point dağıtımında Arena terminolojisi |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| S17-08 TextMeshPro migration | Should Have — zamanı yetmedi, Sprint 18'de Must Have olarak taşındı (S18-03) | 1.0 gün |
| S17-09 Adaptasyon balance pass | Nice to Have — S18-02'ye dahil edildi | — |
| S17-10 Profiling baseline | Nice to Have — Sprint 18'de Must Have (S18-04) | 1.5 gün |
| S16-12 Stat point rename | Nice to Have — S18-12 olarak taşındı | 0.25 gün |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Balance pass 10 run süresi beklenenden uzun olabilir | Orta | 0.5 gün | Her run 10-15 dk, 10 run = 2.5 saat. Paralel olarak not al, toplu fix uygula |
| TextMeshPro migration UI layout bozulmaları | Düşük | 0.5 gün | Font size ve anchor'lar 1:1 korunur, ekran ekran doğrulama |
| Profiling 60fps target'ı karşılanmıyor | Orta | 1 gün | Sprint 17 perf fixleri uygulandı, kalan sorunlar profiler ile tespit edilir ve belgelenir (fix Sprint 19'a kayabilir) |
| Biome müzik asset'leri eksik olabilir | Düşük | 0.25 gün | Placeholder asset'ler kullanılabilir, asıl asset'ler sonraki milestone'da |
| Milestone gate check'te beklenmeyen blocker çıkabilir | Düşük | 0.5 gün | Gate check son göreve bırakıldı, buffer bunu kapsar |

## Dependencies on External Factors
- Sprint 17 tüm Must Have tamamlanmış (✅ perf fixes, adaptation points, test infra)
- 100 TrialChamberData SO (✅)
- AdaptationPointManager + AdaptationConfig (✅ Sprint 17)
- 46 unit test geçiyor (✅ Sprint 17)
- SaveManager (✅ Sprint 12)
- AdaptiveMusicManager + SFXManager (✅ mevcut)
- Sound-bible.md + SFX-specification.md (✅ Approved)

## Definition of Done for this Sprint
- [ ] 10 tam run playtest tamamlanmış, balance notları uygulanmış
- [ ] Adaptasyon noktaları save/load ile korunuyor
- [ ] Tüm in-game text TextMeshPro'ya geçirilmiş
- [ ] Frame budget tablosu gerçek verilerle doldurulmuş
- [ ] Memory ceiling belirlenmiş ve `technical-preferences.md`'ye yazılmış
- [ ] Milestone gate check raporu yazılmış
- [ ] 0 S1/S2 bug
- [ ] Git'e commit

## Milestone Kapatma Kontrol Listesi
- [ ] 100 oda oynanabilir ve balanslanmış
- [ ] 6 biome tematik olarak ayrışık
- [ ] Envanter + ekipman + sentez çalışıyor
- [ ] Kalibrasyon Ağacı + Adaptasyon Noktaları çalışıyor
- [ ] Rarity sistemi (5 tier) tutarlı
- [ ] Kaynak ekonomisi sürdürülebilir
- [ ] Performans 60fps hedefinde veya belgelenmiş sapmalarla
- [ ] 46+ unit test geçiyor
- [ ] ADR'ler güncel
