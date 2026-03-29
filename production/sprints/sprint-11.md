# Sprint 11 — 2026-06-24 to 2026-07-08

## Sprint Goal
Level/Stage progression sistemi + altın ekonomisi: boss yenilince yeni level'a geç, her level farklı biome + düşman havuzu + boss, altın drop sistemi, level arası geçiş ekranı. Sprint sonunda: run'lar tek level yerine sonsuz level ilerlemesi sunuyor, her level daha zor ve farklı.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-15)
- Sprint 1-3: MVP ✅
- Sprint 4-5: Vertical Slice ✅
- Sprint 6-7: Alpha ✅
- Sprint 8: Polish + Onboarding ✅
- Sprint 9-10: Content + Meta ✅
- Sprint 11: Level/Stage + Altın ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S11-01 | **LevelData ScriptableObject** — Biome tipi, düşman havuzu (SpawnEntry[]), boss referansı, zorluk çarpanı, level ismi. Her level'ı tanımlayan veri yapısı | 0.5 | — | LevelData SO oluşturulabilir, tüm alanlar Inspector'da düzenlenebilir |
| S11-02 | **LevelManager** — Level geçiş yöneticisi: mevcut level takibi, boss yenilince sonraki level'a geçiş, biome swap, WaveSpawner'a yeni wave table inject, zorluk scaling uygulama. `GameEvents.OnBossDefeated` dinler, run bitirmez yeni level başlatır | 2.0 | S11-01 | Boss yenilince yeni level başlar, biome değişir, düşmanlar zorlaşır |
| S11-03 | **WaveSpawner level entegrasyonu** — Boss yenilince `Complete` yerine `LevelCleared` state'i, LevelManager'dan yeni WaveTableData alıp devam etme, wave sayısını 5'e düşürme per level | 1.0 | S11-02 | WaveSpawner level geçişinde resetlenip yeni wave table ile devam eder |
| S11-04 | **Level geçiş ekranı** — Boss yenilince kısa ara ekran: "LEVEL X COMPLETE", kazanılan altın/loot özeti, "Continue" butonu. TimeScale=0, oyuncu dinlenir | 1.0 | S11-02 | Level tamamlandığında geçiş ekranı gösterilir, Continue ile devam edilir |
| S11-05 | **Altın drop sistemi** — `GoldManager` static class: run içi altın takibi. Düşman öldüğünde altın düşer (tier bazlı: Normal 1-3, Elite 5-10, Boss 50+). Altın UI göstergesi HUD'da | 1.5 | — | Düşmanlar altın droplar, HUD'da altın sayısı görünür |
| S11-06 | **5 level tanımlama** — Level 1-5 LevelData SO'ları: Caverns→Jungle→Temple→Hell→Mixed. Her biri farklı düşman havuzu, farklı boss, artan zorluk çarpanı (1.0→1.3→1.6→2.0→2.5) | 0.5 | S11-01 | 5 level tanımlı, her biri farklı biome ve boss |
| S11-07 | **Sonsuz level scaling** — Level 5 sonrası prosedürel level üretimi: rastgele biome, zorluk çarpanı artan, düşman havuzu karışık. Oyuncu ölene kadar devam | 0.5 | S11-02, S11-06 | Level 5+ sonsuza kadar devam eder, her level daha zor |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1.0 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S11-08 | **Level arası iyileşme** — Geçiş ekranında oyuncuya %30 HP restore, kısa nefes alma anı | 0.25 | S11-04 | Oyuncu level arası can yeniler |
| S11-09 | **Level göstergesi HUD** — Ekranda "Level X" ve "Wave Y/5" göstergesi, wave text güncelleme | 0.25 | S11-02 | Oyuncu hangi level ve wave'de olduğunu görebilir |
| S11-10 | **Zorluk scaling dengesi** — Enemy HP/Speed/Damage per-level çarpan testi, 5+ level playtest | 0.5 | S11-07 | Zorluk artışı smooth, ne çok kolay ne imkansız |
| S11-11 | **Run sonunda level bilgisi** — Ölüm/zafer ekranında "Reached Level X" gösterimi, leaderboard'a level ekleme | 0.25 | S11-02 | Run özeti level bilgisi içerir |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S11-12 | **Level geçiş animasyonu** — Biome swap sırasında fade transition, "Entering Level X" banner | 0.5 | S11-04 | Level geçişi dramatik hissettiriyor |
| S11-13 | **Altın toplama VFX + SFX** — Altın düştüğünde parlama efekti, toplandığında satisfying ses | 0.5 | S11-05 | Altın toplama tatmin edici |
| S11-14 | **Boss zorluk varyasyonları** — Aynı boss farklı level'da farklı faz sayısı/hız. Level 3+ boss'lar 4 faz | 0.5 | S11-06 | Yüksek level boss'lar daha karmaşık |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| — | Tüm Sprint 10 tamamlandı | — |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| WaveSpawner refactor karmaşık olabilir | Orta | 1 gün | Mevcut yapıyı koruyarak LevelManager overlay yaklaşımı |
| Level geçişinde memory leak (eski wave objleri) | Düşük | 0.5 gün | ObjectPool zaten temizliyor, sadece state reset gerekli |
| Sonsuz scaling dengesi tutmayabilir | Orta | 0.5 gün | Zorluk çarpan cap'i (max 5x), playtest ile ayarla |
| Altın ekonomisi çok cömert/cimri olabilir | Orta | 0.25 gün | Config SO ile data-driven, playtest ile tune |
| Biome swap mid-run performans spike | Düşük | 0.25 gün | Tilemap swap zaten hızlı (mevcut BiomeManager kanıtladı) |

## Dependencies on External Factors
- Mevcut WaveSpawner + BiomeManager altyapısı (✅)
- 5 biome tileset'i (✅ mevcut: Base, Jungle, Temple, Hell, Caverns)
- 2 boss asset'i (✅ mevcut: BossData, CavernGuardianData) — ek boss'lar gerekebilir
- Rogue Adventure boss sprite pack'i (✅ 10 boss sprite mevcut)

## Sprint 11 Sonunda Beklenen Durum
- **Run yapısı**: Tek level → sonsuz level ilerlemesi
- **Her level**: 5 wave + boss, farklı biome, artan zorluk
- **Altın sistemi**: Düşmanlardan altın drop, HUD göstergesi
- **Level geçiş ekranı**: Boss sonrası ara ekran, altın özeti
- **5 tanımlı level** + sonsuz prosedürel level scaling
- **Uzun run'lar**: 10dk yerine 30-60dk+ run'lar mümkün

## Definition of Done for this Sprint
- [ ] Boss yenilince run bitmez, yeni level başlar
- [ ] Her level farklı biome görünümüne sahip
- [ ] 5 tanımlı level farklı düşman/boss kombinasyonu
- [ ] Level 6+ sonsuz scaling çalışır
- [ ] Altın drop sistemi aktif, HUD'da gösterilir
- [ ] Level geçiş ekranı çalışır
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
