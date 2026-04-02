# Sprint 21 — 2026-05-27 to 2026-06-10

## Sprint Goal
Content Expansion: yeni düşman tipleri (3), yeni mutasyonlar (10), synergy sistemi aktivasyonu (8 synergy) ve biome-specific düşman havuzları. Sprint sonunda: her biome'un kendine özgü düşman kompozisyonu var, synergy keşfi çalışıyor, mutasyon çeşitliliği artmış.

## Milestone
- **Alpha Polish** (Sprint 19-22)
- Sprint 19: Runtime Playtest + Audio + Lore v1 + Build Profiling ✅
- Sprint 20: Tutorial/Onboarding + UI Polish + Balance Tuning ✅
- Sprint 21: Content Expansion ← **Buradayız**
- Sprint 22: Alpha Gate Check + Bug Fix + Final Polish

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S21-01 | **3 yeni düşman tipi** — **Shielder** (ön kalkan, arkadan vurulmalı), **Teleporter** (kısa mesafe ışınlanma + saldırı), **Swarm** (küçük, hızlı, gruplar halinde spawn). Her biri için EnemyData SO, Brain script, prefab, sprite placeholder. Mevcut EnemyBrain pattern'ını takip et | 2.5 | — | 3 yeni düşman spawn oluyor, farklı davranışlar çalışıyor, pooling ile |
| S21-02 | **10 yeni mutasyon** — 5 slot (kol/bacak/sırt/baş) + 5 pasif. Mevcut 57'ye ek olarak. Her mutasyonun synergy_tags'ı tanımlı. MutationDatabase'e eklenmeli. Mutasyon etkileri CombatStatBlock veya özel mekanikler | 2.0 | — | 10 mutasyon level-up'ta sunuluyor, stat etkileri çalışıyor, synergy tag'leri tanımlı |
| S21-03 | **Synergy sistemi aktivasyonu (8 synergy)** — SynergyDefinition SO'lar, SynergyDetector runtime tarama, SynergyBanner UI bildirimi. 8 synergy: 2 slot+slot, 3 slot+pasif, 3 pasif+pasif. Her synergy'nin bonus efekti (stat boost veya özel). Synergy keşfi GameEvents üzerinden | 2.0 | S21-02 | 8 synergy definition çalışıyor, tetiklendiğinde banner gösteriliyor, bonus efektler uygulanıyor |
| S21-04 | **Biome-specific düşman havuzları** — Her biome'un TrialChamberData spawn pool'u biome tematik düşman seti kullanacak. Atrium: Chaser+Runner, Assay: +Shooter, Deep Archive: +Exploder+Poisoner, Collapse: +Summoner+Tank, Corruption: +Shielder+Teleporter, Null: hepsi+Swarm. Mevcut SO'lar güncellenmeli | 1.5 | S21-01 | Her biome'da farklı düşman kompozisyonu, biome arttıkça çeşitlilik artıyor |

**Toplam Must Have: 8.0 gün** (kapasite: 8 gün)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S21-05 | **Runtime playtest (S19/S20 carryover)** — 10 tam run, balance + yeni content doğrulama | 1.5 | S21-04 | 10 run raporu yazılmış |
| S21-06 | **Standalone build profiling (S19/S20 carryover)** — Development Build + frame budget | 1.0 | S21-05 | Frame budget doldurulmuş |
| S21-07 | **Yeni düşman SFX** — Shielder kalkan sesi, Teleporter ışınlanma, Swarm vızıltı. Procedural fallback | 0.5 | S21-01 | 3 düşmanın SFX'i çalışıyor |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S21-08 | **Synergy ipucu sistemi** — Mutasyon seçim ekranında mevcut mutasyonlarla synergy olasılığı vurgulama. Keşfedilmemiş synergy "???" olarak işaretlenir | 0.5 | S21-03 | Mutasyon seçiminde synergy ipucu görünüyor |
| S21-09 | **Ek unit testler** — Synergy detection, yeni düşman brain logic. Hedef: 55+ | 0.5 | S21-03 | 55+ test geçiyor |
| S21-10 | **Biome düşman tanıtım popup** — Yeni biome'a girişte "Yeni Tehdit: [düşman adı]" bildirimi | 0.25 | S21-04 | Biome geçişinde yeni düşman bildirimi |

## Carryover from Sprint 20

| Task | Reason | New Estimate |
|------|--------|-------------|
| S20-06 Runtime playtest | Development Build gerekli — deferred | S21-05 olarak 1.5 gün |
| S20-07 Standalone build profiling | Development Build gerekli — deferred | S21-06 olarak 1.0 gün |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Yeni düşman brain'leri mevcut pooling sistemiyle uyumsuz olabilir | Düşük | 0.5 gün | EnemyBrain abstract class pattern'ı takip et, mevcut pool kullanılır |
| Synergy detection performans sorunu (57+ mutasyon × 8 synergy) | Düşük | 0.25 gün | Synergy tarama sadece mutasyon eklendiğinde çalışır (her frame değil) |
| Biome düşman havuzu değişikliği mevcut 100 odanın dengesini bozabilir | Orta | 1 gün | Sadece spawn pool çeşitlendirilir, pressure/multiplier değişmez |
| 10 yeni mutasyon balance sorunu | Orta | 0.5 gün | Mevcut stat range'leri içinde kal, playtest ile doğrula |

## Dependencies on External Factors
- Sprint 20 tamamlanmış (✅ tutorial, UI polish, balance, lore SOs)
- EnemyBrain abstract class + pooling (✅)
- MutationDatabase + MutationSelectionUI (✅)
- SynergyBanner UI mevcut (✅)
- 100 TrialChamberData SO (✅)
- GameEvents synergy event (✅ OnSynergyActivated)

## Definition of Done for this Sprint
- [ ] 3 yeni düşman tipi çalışıyor ve poollanmış
- [ ] 10 yeni mutasyon level-up'ta sunuluyor
- [ ] 8 synergy tetikleniyor ve bonus efektler uygulanıyor
- [ ] Her biome'da farklı düşman kompozisyonu
- [ ] 0 S1/S2 bug
- [ ] Git'e commit
