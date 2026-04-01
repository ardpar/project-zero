# Sprint 19 — 2026-04-29 to 2026-05-13

## Sprint Goal
Milestone 3 (Alpha Polish) başlangıcı: runtime balance playtest, Null Chamber pressure düzeltmesi, audio biome entegrasyonu, standalone build profiling ve lore delivery sistemi v1. Sprint sonunda: 10 run playtest tamamlanmış, biome müzikleri çalışıyor, lore fragment'lar toplanabiliyor, performans standalone build'de ölçülmüş.

## Milestone
- **Alpha Polish** (Sprint 19-22)
- Sprint 19: Runtime Playtest + Audio + Lore v1 + Build Profiling ← **Buradayız**
- Sprint 20: Tutorial/Onboarding + UI Polish + Balance Tuning
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
| S19-01 | **Runtime balance playtest (10 run)** — Development Build oluştur, 10 tam run oyna (oda 1→ölüm). Her run için: ölüm odası, biome geçiş hissi, fragment ekonomisi yeterliliği, adaptasyon noktası dağılımı not al. Kırık/ölü odalar tespit et. Sonuç: `tests/balance/playtest-report-s19.md` | 2.0 | — | 10 run tamamlanmış, her run notlandırılmış, tespit edilen sorunlar listelenmiş |
| S19-02 | **Null Chamber pressure düzeltmesi** — Balance raporundaki öneri: oda 85-86 ve 92-93'ü P5→P4'e düşür. Boss odaları (16,33,50,67,84,100) `isBossRoom` flag'i ekle. TrialChamberData SO'ları güncelle | 0.5 | — | 4 oda P4'e düşürülmüş, 6 boss odası flag'lenmiş |
| S19-03 | **Audio biome entegrasyonu** — AdaptiveMusicManager'a 6 biome stem bağla. Biome değişiminde crossfade (0.5s). Boss fight'ta tension stem aktifleştir. Kalibrasyon Aralığı'nda ambient stem. SFXManager'a sentez/craft/level-up efektleri | 2.0 | — | Her biome'da farklı müzik, boss fight tension, biome geçişinde crossfade, sentez SFX |
| S19-04 | **Lore delivery sistemi v1** — SignalArchive: lore fragment'lar düşman/boss'tan drop olur, envanterde toplanır, okunabilir. `LoreFragment` SO (id, title, text, category, biome). `SignalArchiveScreen` UI (kategori listesi, fragment okuma). `LoreDropper` — boss kill'de guaranteed, elite'de %10 şans. Save entegrasyonu (CharacterSaveData.signalArchiveEntries) | 2.0 | — | Lore fragment'lar düşüyor, envanterde toplanıyor, okunabiliyor, save/load çalışıyor |
| S19-05 | **Standalone build profiling** — Development Build oluştur. Unity Profiler bağla. 4 senaryo ölç: wave 1 (az düşman), wave 5 (çok düşman), boss fight, 200 düşman stress test. Frame-time budget tablosunu gerçek verilerle doldur. Draw call sayısı (Frame Debugger). Memory ceiling belirle. `technical-preferences.md` güncelle | 1.5 | S19-01 | Frame budget tablosu doldurulmuş, memory ceiling belirlenmiş, draw call sayısı belgelenmiş |

**Toplam Must Have: 8.0 gün** (kapasite: 8 gün — buffer minimum)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S19-06 | **Sentez balance pass** — 3 sentez formülü playtest ile doğrulama (S18-07 carryover). Fragment ekonomisi: tedarik fiyatları vs. kazanç hızı. Komponent birleştirme stat dengesi | 0.5 | S19-01 | Sentez ödüllendirici ama broken değil |
| S19-07 | **Equipment slot balance** — 6 slot stat etkileri dengeli (S18-08 carryover). Boss-specific Architect-Grade item'lar güçlü ama degenerate strateji oluşturmuyor | 0.5 | S19-01 | Hiçbir slot combination broken değil |
| S19-08 | **Lore content: 20 fragment yazımı** — 6 biome için 3-4 lore fragment her biome'a. Arena tarihini, Mimar dönemini, Subject deneylerini anlatan kısa metinler. Expansion-vision narrative framework'e uyumlu | 0.5 | S19-04 | 20 LoreFragment SO oluşturulmuş, biome'lara dağıtılmış |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S19-09 | **Kalibrasyon Ağacı balance** — 4 dal maliyet/etki dengesi (S18-09 carryover). Erken node'lar ucuz, geç node'lar pahalı. Toplam unlock süresi 20-30 run | 0.5 | S19-01 | Node maliyet eğrisi logaritmik |
| S19-10 | **Ek unit testler** — Sentez formülleri, LoreFragment system, pressure scaling. Hedef: 60+ toplam | 0.5 | S19-04 | 60+ test geçiyor |
| S19-11 | **Boss odası müzik geçişi polish** — Boss spawn anında dramatic sting + tension loop. Boss yenilgisinde victory sting + ambient'e dönüş | 0.25 | S19-03 | Boss fight müzik geçişleri tatmin edici |

## Carryover from Sprint 18

| Task | Reason | New Estimate |
|------|--------|-------------|
| S18-01 Runtime playtest (10 run) | Statik analiz yapıldı, runtime playtest deferred | S19-01 olarak 2.0 gün |
| S18-04 Standalone build profiling | Editor baseline yapıldı, build profiling deferred | S19-05 olarak 1.5 gün |
| S18-06 Audio biome entegrasyonu | Should Have — zamanı yetmedi | S19-03 olarak 2.0 gün |
| S18-07 Sentez balance pass | Should Have — deferred | S19-06 olarak 0.5 gün |
| S18-08 Equipment slot balance | Should Have — deferred | S19-07 olarak 0.5 gün |
| S18-09 Kalibrasyon Ağacı balance | Nice to Have — deferred | S19-09 olarak 0.5 gün |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| 10 run playtest 2 günden fazla sürebilir | Orta | 0.5 gün | Her run 10-15 dk, paralel not alma, toplu fix. Gerekirse 7 run'a düşür |
| Audio asset'leri (müzik stem'leri) mevcut olmayabilir | Yüksek | 1 gün | Placeholder sine/square wave stem'ler kullanılabilir, asıl asset'ler sonraki sprint |
| Lore delivery sistemi SaveManager entegrasyonu karmaşık olabilir | Düşük | 0.5 gün | CharacterSaveData'ya List<string> ekle, mevcut pattern'ı takip et |
| Standalone build profiling beklenmeyen perf sorunları ortaya çıkarabilir | Orta | 1 gün | Sorunları belgele, fix'leri Sprint 20'ye planla. Sprint 19'da sadece ölçüm |
| Buffer minimum (0 gün) — hiçbir Must Have kayamaz | Yüksek | 1 gün | Playtest + profiling (3.5 gün) birlikte yapılabilir (aynı build session). Audio veya lore Nice to Have'e kayabilir |

## Dependencies on External Factors
- Sprint 18 tamamlanmış (✅ commit be63b404 + ecab99d8)
- Milestone 2 gate check PASS (✅)
- AdaptiveMusicManager + SFXManager mevcut (✅)
- Sound-bible.md + SFX-specification.md (✅ Approved)
- Expansion-vision narrative framework (✅ "Arena Remembers, Subject Doesn't")
- Unity Development Build capability (Unity 6.3 LTS)

## Definition of Done for this Sprint
- [ ] 10 tam run playtest tamamlanmış ve raporlanmış
- [ ] Null Chamber pressure düzeltmesi uygulanmış
- [ ] Her biome'da farklı müzik çalıyor
- [ ] Lore fragment drop + toplama + okuma çalışıyor
- [ ] Standalone build frame budget tablosu doldurulmuş
- [ ] Memory ceiling belirlenmiş
- [ ] 0 S1/S2 bug
- [ ] Git'e commit
