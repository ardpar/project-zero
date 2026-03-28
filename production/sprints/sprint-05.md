# Sprint 5 — 2026-03-29 to 2026-04-12

## Sprint Goal
Vertical Slice tamamla: Main Menu, Pause Menu, Run End istatistikleri, Shooter düşman, daha fazla mutasyon (25+ hedef), ve oyun akışı polish. Sprint sonunda: baştan sona profesyonel oyun akışı olan, demo paylaşılabilir bir build.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Vertical Slice** (Post-MVP, Sprint 4-5)
- Sprint 4: Asset Entegrasyonu ✅
- Sprint 5: Game Flow + Content Expansion ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S5-01 | **Main Menu** — Basit sahne: Play butonu, Quit butonu, oyun logosu/ismi, arka plan | 0.5 | — | Main Menu sahnesinden Play'e basınca oyun başlar, Quit kapatır |
| S5-02 | **Pause Menu** — ESC ile pause overlay: Resume, Main Menu, Quit butonları | 0.5 | — | ESC→pause, Resume devam eder, Main Menu→ana menüye döner |
| S5-03 | **Run End Stats Ekranı** — Game Over/Victory'de: sürvive süresi, öldürülen düşman, toplanan XP, ulaşılan level, edinilen mutasyonlar | 1.0 | — | İstatistikler doğru gösterilir, Restart ve Main Menu butonları çalışır |
| S5-04 | **Shooter Enemy aktifleştirme** — ShooterData SO, Shooter prefab, WaveTable'a ekle (Wave 3+) | 0.5 | — | Shooter düşman menzilden projectile atar, WaveTable'da doğru dalga'dan spawn olur |
| S5-05 | **11 yeni mutasyon (25 toplam)** — 4 yeni slot mutasyonu (alternatif Arms/Legs/Back/Head) + 7 yeni pasif. MutationDatabase güncelle | 1.5 | — | MutationDatabase 25+ mutasyon içerir, yeni kartlar seçim ekranında çıkar |
| S5-06 | **3 yeni synergy (11 toplam)** — Yeni mutasyonlarla oluşan synergy kombinasyonları | 0.5 | S5-05 | 11+ synergy tanımlı, yeni kombinasyonlar tetiklenir |
| S5-07 | **Playtest Balance Pass 2** — 5+ tam run, XP curve, spawn rate, damage, mutation güç dengesi. SO değerleri fine-tune | 1.5 | S5-01→S5-06 | 12-15 dk run hedefi, zorluk eğrisi smooth, mutasyon seçimleri anlamlı |
| S5-08 | **Scene Flow** — Main Menu → Gameplay → Game Over/Victory → Main Menu. Scene transition, GameEvents.Cleanup arası reset | 1.0 | S5-01, S5-03 | Oyun döngüsü kesintisiz: menü→oyun→bitiş→menü→tekrar oyun, state leak yok |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S5-09 | **Enemy Animator'ları** — Chaser, Runner, Exploder, Boss idle/walk animasyonları (ElvGames spritesheets) | 1.0 | — | Tüm düşmanlar animasyonlu hareket eder |
| S5-10 | **Mutation Kart detayları** — Kart üzerinde stat değişiklikleri listesi (+15% damage gibi), synergy hint ("Blade + Energy = Blade Storm!") | 0.5 | — | Oyuncu kart seçerken stat etkisini görebilir |
| S5-11 | **Wave Start/Clear Banner** — "WAVE 3" ve "WAVE CLEAR!" animasyonlu büyük text | 0.25 | — | Her dalga başında/sonunda banner görünür |
| S5-12 | **Settings Screen** — Ses volume slider (Master/SFX/Music), screen shake toggle | 0.5 | S5-01 | Settings ana menü ve pause'dan erişilebilir, ayarlar kaydedilir |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S5-13 | **Dash cooldown UI indicator** — Circular veya bar göstergesi HUD'da | 0.25 | — | Dash CD görsel olarak dolum gösterir |
| S5-14 | **Active mutation icons HUD** — Edinilen mutasyonların küçük ikonları HUD kenarında | 0.25 | — | Mutasyon ikonları HUD'da listelenmiş |
| S5-15 | **Ekstra tileset** — Temple veya Jungle alternatif arena zemini | 0.25 | — | 2. tileset hazır, değiştirilebilir |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| S4-05 Arena Tileset polish | Floor tile çok grid-like görünüyor, daha iyi tile seçimi gerekebilir | 0.25 gün (S5-07 içinde) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Scene transition state leak (event subscriber'lar kalıntı) | Yüksek | 1 gün debug | GameEvents.Cleanup() + SceneManager.LoadScene ile domain reset |
| 25 mutasyon dengesi playtest gerektiriyor | Orta | Balance bozuk | SO'lar data-driven, hotfix hızlı |
| Shooter enemy projectile player'a çarpıyor | Düşük | 0.5 gün | Collision matrix'te EnemyProjectile layer ekle |
| Main Menu → Game scene geçişinde loading delay | Düşük | 0.25 gün | SampleScene küçük, async load gerekmez |

## Dependencies on External Factors
- ElvGames asset paketi (✅ entegre edildi)
- Unity Scene Management API (built-in)

## Sprint 5 Sonunda Beklenen Durum (Vertical Slice Complete)
- **Profesyonel oyun akışı:** Main Menu → Play → 6 Wave + Boss → Victory/Game Over → Stats → Restart/Menu
- **25+ mutasyon** ile çeşitlilik — her run farklı
- **11+ synergy** — keşif heyecanı
- **4 düşman türü** — Chaser, Runner, Exploder, Shooter
- **Pause menü** — ESC ile erişilebilir
- **Run-end istatistikleri** — oyuncu performansını görebilir
- **Demo paylaşılabilir** build

## Definition of Done for this Sprint
- [ ] Main Menu → Play → Game → End → Menu döngüsü çalışır
- [ ] Pause menü ESC ile açılır/kapanır
- [ ] Run sonunda istatistik ekranı gösterilir
- [ ] 25+ mutasyon seçilebilir
- [ ] 11+ synergy tetiklenebilir
- [ ] Shooter düşman projectile atar
- [ ] 5+ playtest run tamamlanmış
- [ ] State leak yok (menü→oyun→menü→oyun döngüsü temiz)
- [ ] Git'e commit edilmiş

## Post-Vertical Slice (Sprint 6+)
- Meta-progression (unlock system, currency)
- Save/Load system
- Adaptive müzik
- Leaderboard
- Daha fazla biome/tileset
- Real pixel art sprites (custom, ElvGames'i değiştir)
- Mobile/Gamepad input polish
