# Sprint 23 — 2026-06-24 to 2026-07-08

## Sprint Goal
Teknik borç temizliği: save versioning (kritik), assembly yapısı düzeltme, hardcoded değerleri SO'lara taşıma, PlayerPrefs kaldırma, Resources.Load kaldırma, ölü kod temizliği ve test coverage genişletme. Sprint sonunda: save dosyası güvenli, mimari ADR-001'e uyumlu, 80+ test.

## Milestone
- **Tech Debt Paydown** (Sprint 23, standalone)
- Milestone 3 borçları + mimari inceleme bulguları

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S23-01 | **Save data versioning** — `SaveData` ve `CharacterSaveData`'ya `int saveVersion` field ekle. `SaveManager.LoadSlot()` ve `LoadGlobal()`'da version check + migration logic. Version 0→1 migration: yeni field'lar default ile doldurulur. Write-to-temp-then-rename pattern: `.tmp` yaz → eski → `.bak` → `.tmp` → live | 1.5 | — | Save version 1 yazılıyor, eski save'ler okunabiliyor, write-then-rename çalışıyor |
| S23-02 | **Assembly yapısı düzeltme** — `GameBootstrap.cs` ve `LootDropper.cs` → yeni `Synthborn.Bootstrap` asmdef. `Lore/` → `Synthborn.Lore` asmdef (Core, Waves, Enemies, Persistence referansları). `Editor/` → `Synthborn.Editor` asmdef (Editor platform only). Tüm assembly referansları güncel | 1.0 | — | 0 script Assembly-CSharp'ta, compile temiz, circular dependency yok |
| S23-03 | **Hardcoded değerler → SO** — GameBootstrap stat multiplier'ları (0.02/0.03/0.02/0.01) → `StatPointConfig` SO. LootDropper drop rate'leri (0.05/0.30/0.10/0.30/0.15) → `LootConfig` SO. FragmentManager tier values → `FragmentEconomyConfig` SO. GameBootstrap class name array → ClassData SO referansı | 1.5 | S23-02 | 0 hardcoded gameplay değeri, tüm tuning Inspector'dan |
| S23-04 | **PlayerPrefs → RunSessionData** — 17 PlayerPrefs çağrısını kaldır. `RunSessionData` static class veya DontDestroyOnLoad GO ile sahne arası state. `SelectedChamber`, `SelectedLevel`, `LastPlayedSlot` gibi değerler save-agnostic runtime state olarak | 1.0 | — | 0 PlayerPrefs.GetInt/SetInt gameplay kodunda |
| S23-05 | **Resources.Load kaldırma** — 4 `Resources.Load` çağrısını `[SerializeField]` injection'a çevir. GameBootstrap: ItemDatabase + SkillTreeData serialized. CraftScreen: ItemDatabase serialized. SkillTreeScreen: SkillTreeData serialized | 0.5 | S23-02 | 0 Resources.Load runtime'da |
| S23-06 | **Ölü kod + cleanup** — `GoldManager.cs` sil (FragmentManager supersede etti). `OnGoldChanged` legacy alias kaldır (subscriber'lar `OnFragmentChanged`'a geç). GameEvents duplicate XML doc düzelt. GameEvents Raise method isimlendirme tutarlılığı. Lambda subscriptions → named methods | 1.0 | — | GoldManager yok, GameEvents temiz, lambda subscription yok |
| S23-07 | **Test coverage genişletme** — ObjectPool (re-borrow, double-return, grow-on-demand), SaveManager (mock filesystem veya temp dir), LootDropper (drop rate distribution), GameEvents (cleanup, subscribe/unsubscribe). Hedef: 80+ test | 1.5 | S23-01, S23-03 | 80+ test geçiyor, critical sistem'ler kapsanmış |

**Toplam Must Have: 8.0 gün** (kapasite: 8 gün)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S23-08 | **FindAnyObjectByType caching** — PrototypeHUD, LoreDropper, LootDropper, CalibrationIntervalScreen: `Start()`/`Awake()`'te cache, update-path'te kullanma. 20 instance → 0 hot-path Find çağrısı | 0.5 | — | 0 FindAnyObjectByType frame-sensitive kodda |
| S23-09 | **DeathBurstVFX pooling** — `ObjectPool<DeathBurstVFX>` oluştur, mevcut Instantiate/Destroy → pool pattern | 0.5 | — | DeathBurstVFX poollanmış |
| S23-10 | **GameBootstrap lambda → named methods** — Anonymous delegate'ler → private void OnHPOrbRequested, OnPlayerDamageRequested, OnEnemyDiedForBootstrap. Proper unsubscribe in OnDisable | 0.5 | S23-06 | 0 anonymous event subscription |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S23-11 | **SaveManager interface/facade** — `ISaveProvider` interface, SaveManager bunu implement etsin. Consumer'lar interface'e bağlansın — 38 doğrudan bağımlılığı azaltır | 1.0 | S23-01 | SaveManager interface arkasında, en az 10 consumer geçirilmiş |
| S23-12 | **Async save** — `SaveSlotAsync()` + `LoadSlotAsync()` — `Task` veya coroutine ile arka planda dosya I/O | 0.5 | S23-01 | Save/load main thread'i bloklamıyor |

## Carryover

| Task | Reason | New Estimate |
|------|--------|-------------|
| — | Yeni sprint, carryover yok. Tüm task'lar mimari inceleme bulgularından | — |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Assembly restructure compile hataları zincirleme | Yüksek | 1 gün | Her asmdef değişikliğinden sonra compile kontrol, incremental |
| Save version migration eski save'leri bozabilir | Orta | 0.5 gün | Version 0 (mevcut) → Version 1 migration testi yaz |
| PlayerPrefs kaldırma mevcut save slot selection'ı kırabilir | Orta | 0.5 gün | RunSessionData'yı PlayerPrefs'in yerine 1:1 koy, sonra PlayerPrefs sil |
| 38 SaveManager consumer'ı refactor etmek zaman alabilir | Yüksek | 1 gün | Bu sprint'te sadece interface tanımla + 10 consumer geçir, kalanı next sprint |

## Dependencies on External Factors
- Sprint 22 tamamlanmış (✅)
- Mimari inceleme raporu (✅ bu session'da yapıldı)
- 8 ADR mevcut (✅)
- 60 test mevcut (✅)

## Definition of Done for this Sprint
- [ ] Save dosyaları version 1 ile yazılıyor
- [ ] Write-to-temp-then-rename save pattern çalışıyor
- [ ] 0 script Assembly-CSharp'ta
- [ ] 0 hardcoded gameplay değeri
- [ ] 0 PlayerPrefs gameplay kodunda
- [ ] 0 Resources.Load runtime'da
- [ ] GoldManager silinmiş
- [ ] GameEvents temiz (naming, docs, no lambdas)
- [ ] 80+ unit test geçiyor
- [ ] 0 compile error
- [ ] Git'e commit
