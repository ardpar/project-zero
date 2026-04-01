# Sprint 17 — 2026-04-01 to 2026-04-15

## Sprint Goal
Performans optimizasyonu (13 bilinen issue), Adaptasyon Noktaları sistemi (run-içi stat dağıtımı), unit test altyapısı kurulumu ve data-driven compliance düzeltmeleri. Sprint sonunda: GC spike'lar çözülmüş, damage number bug fixlenmiş, run-içi stat allocation çalışır, temel sistemlerde unit test mevcut.

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-18)
- Sprint 11: Level/Stage + Gold ✅
- Sprint 12: WorldMap + Character + Save/Load + Items + Inventory ✅
- Sprint 13: Skill Tree + Loot + Stat Points + Craft + Shop ✅
- Sprint 14: Trial Chamber + Biome + Pressure Scaling ✅
- Sprint 15: Resource Economy + Arena Terminology + Biome 2 ✅
- Sprint 16: Sentez Genişletme + Rarity Rename + 100 Oda + 6 Biome ✅
- Sprint 17: Performance + Adaptasyon Noktaları + Test Altyapısı ← **Buradayız**

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S17-01 | **Critical perf fixes (C1-C3)** — DeathParticleSpawner pooling (C1), DamageNumberSpawner pooling + broken counter fix (C2), DeathBurstVFX serialized material + leak fix (C3). Mevcut `ObjectPool<T>` altyapısı kullanılır | 1.5 | — | DeathParticle ve DamageNumber poollanmış, damage numbers 20'den sonra da çalışıyor, material leak yok, Shader.Find kaldırılmış |
| S17-02 | **High perf fixes (H1-H4)** — EnemyBrain.TickContactDamage GetComponent cache (H1), TargetingSystem cached IDamageable (H2), LootDropper list reuse (H3), DashTrail ghost pooling (H4) | 1.0 | — | GetComponent hot-loop'lardan kaldırılmış, LootDropper zero-alloc, DashTrail poollanmış |
| S17-03 | **High perf fixes (H5-H8)** — PrototypeHUD FindObjectsOfTypeAll kaldırma (H5), WaveSpawner FindFirstObjectByType kaldırma (H6), SummonerBrain lambda alloc fix (H7), PoisonerBrain trail pooling (H8) | 1.0 | — | Runtime Find* çağrıları kaldırılmış, SummonerBrain zero-alloc iteration, poison trail poollanmış |
| S17-04 | **Adaptasyon Noktaları sistemi** — Level-up'ta +1 adaptasyon noktası. 5 parametre: MASS (damage), RESILIENCE (HP), VELOCITY (speed), VARIANCE (crit), YIELD (XP). UI: level-up ekranında mutasyon seçimi yanında stat allocation paneli. Noktalar run-içi, ölümde sıfırlanır. `AdaptationPointManager` + `AdaptationPointUI` + `AdaptationConfig` SO | 2.0 | — | Level-up'ta adaptasyon noktası dağıtılabiliyor, 5 parametre çalışıyor, stat etkileri uygulanıyor, ölümde reset |
| S17-05 | **Unit test altyapısı + core system testleri** — Unity Test Framework kurulumu, test assembly tanımlama. EntityHealth, XPManager, CombatStatBlock, ObjectPool için unit testler. Minimum 20 test | 1.5 | — | Test runner çalışıyor, 20+ test geçiyor, CI'da çalıştırılabilir |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1.0 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S17-06 | **Medium perf fixes (M1-M6)** — HitFlash WaitForSeconds cache (M1), EnemyBrain coroutine alloc azaltma (M2), XPGem FindFirstObjectByType kaldırma (M3), sqrMagnitude migration (M4-M5), ExploderBrain warning cache (M6) | 0.5 | S17-01 | Tüm medium issue'lar fixlenmiş |
| S17-07 | **Data-driven compliance (L1-L5)** — ContactDamageInterval → EnemyScalingConfig (L1), player base speed injection (L2), PoisonTrail tick → PoisonerData (L3), DamageNumber → DamageNumberConfig SO (L4), HPOrb → HPOrbConfig SO (L5) | 0.5 | — | Hardcoded değerler SO'lara taşınmış, Inspector'dan düzenlenebilir |
| S17-08 | **TextMeshPro migration (L6)** — PrototypeHUD, KillCounter, LevelHUD, GoldHUD: `UnityEngine.UI.Text` → `TextMeshProUGUI`. Font atlas setup | 0.5 | — | Tüm HUD text'leri TMP ile render ediliyor, draw call azalması |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S17-09 | **Adaptasyon Noktaları balance pass** — 5 parametre için etki formülleri ince ayar, playtest ile doğrulama. Her noktanın hissedilir ama broken olmayan etkisi | 0.5 | S17-04 | 10 test run'da hiçbir parametre dominant değil |
| S17-10 | **Performans profiling baseline** — Unity Profiler ile runtime ölçüm, frame-time budget tablosu doldurma, memory ceiling belirleme. `technical-preferences.md` güncelleme | 0.5 | S17-01, S17-02 | Frame budget tablosu gerçek verilerle doldurulmuş |
| S17-11 | **Sprite Atlas Analyzer geçişi** — Unity 6.3 Sprite Atlas Analyzer ile draw call analizi, gerekli atlas grouping | 0.5 | S17-08 | Draw call sayısı ölçülmüş ve belgelenmiş |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| — | Sprint 16 tamamen tamamlandı (Must Have + Should Have + Nice to Have dahil). Ek olarak 100 oda, 6 biome, boss legendaries, synthesis discovery, rarity glow tamamlandı. | Carryover yok |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| ObjectPool refactor'ü mevcut VFX davranışını bozabilir | Orta | 1 gün | Her pool fix'ten sonra manual playtest: wave 5+ düşman öldürme, VFX doğrulama |
| DamageNumber counter fix'i beklenmeyen UI davranışına yol açabilir | Düşük | 0.25 gün | Fix öncesi mevcut davranışı videoya al, fix sonrası karşılaştır |
| Adaptasyon Noktaları mevcut stat pipeline'ına entegrasyon karmaşıklığı | Orta | 0.5 gün | CombatStatBlock'a additive modifier olarak ekle, mevcut mutation stat'larıyla çakışma riski düşük |
| Unity Test Framework + proje setup uyumsuzluğu | Düşük | 0.5 gün | Assembly definition doğru referanslarla, asmdef circular dependency'den kaçın |
| TextMeshPro migration mevcut UI layout'ları bozabilir | Düşük | 0.25 gün | Font size ve anchor'lar 1:1 korunur, her ekran manual doğrulama |

## Dependencies on External Factors
- Sprint 16 tüm sistemler (✅ commit 02698f99)
- 100 TrialChamberData SO (✅)
- ObjectPool<T> altyapısı (✅ src/core/)
- CombatStatBlock (✅ src/core/)
- Performance report (✅ tests/performance/perf-report-2026-03-29.md)
- Unity Test Framework (Unity 6.3 LTS ile birlikte gelir)

## Definition of Done for this Sprint
- [ ] C1-C3 critical perf issues fixlenmiş (pool + material leak + counter bug)
- [ ] H1-H8 high perf issues fixlenmiş (GetComponent cache, zero-alloc patterns)
- [ ] Adaptasyon Noktaları: 5 parametre, level-up UI, run-içi etki, ölümde reset
- [ ] 20+ unit test geçiyor (EntityHealth, XPManager, CombatStatBlock, ObjectPool)
- [ ] Damage numbers run boyunca çalışıyor (C2 counter bug fixlenmiş)
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
