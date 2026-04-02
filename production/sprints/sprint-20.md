# Sprint 20 — 2026-05-13 to 2026-05-27

## Sprint Goal
Tutorial/onboarding akışı, UI polish pass, balance tuning (sentez + ekipman + skill tree) ve lore içerik yazımı. Sprint sonunda: yeni oyuncu ilk 3 odayı rehberli geçebiliyor, tüm ekranlar visual tutarlı, ekonomi balanslanmış, 20 lore fragment oynanabilir.

## Milestone
- **Alpha Polish** (Sprint 19-22)
- Sprint 19: Runtime Playtest + Audio + Lore v1 + Build Profiling ✅
- Sprint 20: Tutorial/Onboarding + UI Polish + Balance Tuning ← **Buradayız**
- Sprint 21: Content Expansion (yeni düşmanlar, mutasyonlar, synergy'ler)
- Sprint 22: Alpha Gate Check + Bug Fix + Final Polish

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S20-01 | **Tutorial/Onboarding sistemi** — İlk run'da tetiklenen TutorialManager: hareket (WASD), dash (Space), otomatik saldırı, XP/level-up, mutasyon seçimi, kalibrasyon aralığı adımları. TutorialOverlay UI zaten mevcut — adım sistemi ve tetikleme mantığı eklenmeli. `TutorialStep` SO (id, trigger event, text, highlight rect). İlk 3 oda boyunca kademeli öğretim. Skip butonu. Save'e "tutorialCompleted" flag | 2.5 | — | Yeni oyuncu ilk 3 odayı rehberli geçebiliyor, tekrar gösterilmiyor, skip çalışıyor |
| S20-02 | **UI polish pass** — Tüm ekranlarda tutarlı visual language: buton hover efektleri (color tint + scale), ekran geçişleri (SceneFader fade in/out), popup animasyonları (scale + alpha), rarity renkleri (Baseline gri → Architect-Grade altın) tüm UI'da tutarlı. TMP font boyutları normalize edilmeli | 2.0 | — | Tüm butonlar hover feedback veriyor, ekran geçişleri fade ile, rarity renkleri tutarlı |
| S20-03 | **Balance tuning: sentez + ekipman + skill tree** — Sentez formülleri (3 tarif): malzeme maliyeti vs. çıktı değeri dengeli. Komponent birleştirme: stat bonusu anlamlı ama broken değil. 6 ekipman slotu: stat etkileri dengeli. Kalibrasyon Ağacı: 4 dal node maliyetleri logaritmik, erken ucuz geç pahalı. Fragment ekonomisi: tedarik fiyatları vs. kazanç hızı sürdürülebilir | 2.0 | — | 5 test run'da sentez kullanışlı, ekipman çeşitli build'ler destekliyor, skill tree her dal viable, fragment ekonomisi sürdürülebilir |
| S20-04 | **Lore content: 20 fragment yazımı** — 6 biome için 3-4 lore fragment. Arena tarihini, Mimar dönemini, Subject deneylerini anlatan kısa metinler. Expansion-vision narrative framework'e uyumlu. `LoreFragment` SO'lar oluştur ve `LoreDatabase`'e bağla | 1.5 | — | 20 LoreFragment SO oluşturulmuş, biome'lara dağıtılmış, LoreDatabase wired |

**Toplam Must Have: 8.0 gün** (kapasite: 8 gün)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S20-05 | **Placeholder biome müzik stem'leri** — 6 biome için procedural sine/square wave ambient stem'ler (her biri 30-60 sn, looping). BiomeConfig SO'lara bağla. Gerçek müzik asset'leri gelince değiştirilecek | 0.5 | — | Her biome'da farklı müzik çalıyor (procedural) |
| S20-06 | **Runtime playtest (S19-01 carryover)** — Development Build, 10 tam run, playtest raporu | 1.5 | — | 10 run raporu yazılmış |
| S20-07 | **Standalone build profiling (S19-05 carryover)** — Development Build + Profiler, frame budget, memory | 1.0 | S20-06 | Frame budget doldurulmuş |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S20-08 | **Ek unit testler** — Tutorial system, lore drop logic, sentez formül testleri. Hedef: 60+ | 0.5 | S20-01 | 60+ test geçiyor |
| S20-09 | **SignalArchiveScreen erişim noktaları** — Arena Map + Kalibrasyon Aralığı'ndan "Sinyal Arşivi" butonu | 0.25 | — | Her iki ekrandan arşive erişim çalışıyor |
| S20-10 | **Run istatistik ekranı polish** — TotalStatsScreen + RunHistoryScreen: TMP migration + Arena terminolojisi | 0.25 | — | Stat ekranları TMP ile, Arena terminolojisiyle |

## Carryover from Sprint 19

| Task | Reason | New Estimate |
|------|--------|-------------|
| S19-01 Runtime playtest (10 run) | Development Build gerekli — deferred | S20-06 olarak 1.5 gün |
| S19-05 Standalone build profiling | Development Build gerekli — deferred | S20-07 olarak 1.0 gün |
| S19-06 Sentez balance pass | Should Have — S20-03'e dahil | — |
| S19-07 Equipment slot balance | Should Have — S20-03'e dahil | — |
| S19-08 Lore content yazımı | Should Have — S20-04 olarak Must Have | 1.5 gün |
| S19-09 Skill tree balance | Nice to Have — S20-03'e dahil | — |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Tutorial sistemi TutorialOverlay mevcut yapısına uyum zorluğu | Orta | 0.5 gün | TutorialOverlay zaten var — sadece step logic ve event trigger eklenmeli |
| UI polish 2 günde bitmeyebilir (40+ UI dosyası) | Yüksek | 1 gün | Sadece gameplay HUD + en çok kullanılan ekranlar (ArenaMap, CharacterScreen, CalibrationInterval) odaklı. Diğerleri Sprint 22'ye |
| Balance tuning subjektif — "deneyimle" belirlenmeli | Orta | 0.5 gün | Formül bazlı ayarlamalar (SO değerleri) + 5 playtest run |
| 20 lore fragment yazımı yaratıcı blok | Düşük | 0.5 gün | Expansion-vision narrative framework ve world-lore-bible mevcut — template'ten üretim |

## Dependencies on External Factors
- Sprint 19 code tasks tamamlanmış (✅ lore system, audio, balance fix)
- TutorialOverlay mevcut (✅ Sprint 12+)
- Expansion-vision narrative framework (✅)
- World-lore-bible + boss-lore-samples mevcut (✅ design/narrative/)
- SceneFader mevcut (✅ UI/)
- Development Build capability (Unity 6.3 LTS)

## Definition of Done for this Sprint
- [ ] Tutorial ilk 3 odada çalışıyor, skip edilebilir
- [ ] Buton hover + ekran fade + popup animasyonları tutarlı
- [ ] Sentez/ekipman/skill tree balanslanmış (5 run playtest)
- [ ] 20 lore fragment yazılmış ve oynanabilir
- [ ] 0 S1/S2 bug
- [ ] Git'e commit
