# Sprint 8 — 2026-05-10 to 2026-05-24

## Sprint Goal
Post-Alpha polish ve eksik feature'lar: Tutorial/Onboarding, Local Leaderboard, adaptive müzik gameplay entegrasyonu, performance profiling, ve build kalitesini yükseltme. Sprint sonunda: feedback-ready, ilk kez oynayan biri bile kolayca oynayabilir build.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Post-Alpha Polish** (Sprint 8+)
- Sprint 1-3: MVP ✅
- Sprint 4-5: Vertical Slice ✅
- Sprint 6-7: Alpha ✅
- Sprint 8: Polish + Onboarding ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S8-01 | **Adaptive Music — Gameplay Wiring** — AdaptiveMusicManager'ı gameplay sahnesine ekle, wave intensity'ye göre stem switch, Dark Fantasy stem'lerini config'e bağla | 1.0 | — | Oyun içi müzik wave'e göre yoğunlaşır, crossfade smooth |
| S8-02 | **Tutorial/Onboarding Overlay** — İlk run'da basit overlay: WASD hareket, Space dash, otomatik saldırı, gem topla. PlayerPrefs ile "ilk kez" kontrolü | 1.0 | — | İlk kez oynayan kontrolleri anlayabilir, tekrar gösterilmez |
| S8-03 | **Local Leaderboard** — Top 10 run sıralaması (survival time + kills), Main Menu'den erişim, SaveData entegrasyonu | 0.5 | — | Top 10 leaderboard gösterilir, yeni run otomatik eklenir |
| S8-04 | **Gameplay müzik stem'leri atama** — calmStem, lowStem, mediumStem, highStem clip'lerini AdaptiveMusicConfig'e bağla, uygun track'leri seç | 0.5 | S8-01 | 4 intensity tier'da farklı müzik çalar |
| S8-05 | **Performance Profiling** — 200 enemy + VFX senaryosunda 60fps doğrulama, Profiler data toplama, bottleneck analizi | 1.0 | — | Profiler raporu var, 60fps hedefi doğrulanmış veya bottleneck'ler belgelenmiş |
| S8-06 | **Bug Fix Pass** — Console error temizliği, edge case testleri, MainMenu duplicate obje kontrolü | 0.5 | — | 0 console error, bilinen bug yok |
| S8-07 | **Build & Playtest** — Güncel standalone build (Win+Mac), 5+ run playtest, feedback toplama | 1.0 | S8-01→S8-06 | Build çalışır, 5 run sorunsuz tamamlanır |

**Toplam Must Have: 5.5 gün** (kapasite: 8 gün — 2.5 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S8-08 | **Daily Seed** — Günlük sabit seed, aynı gün aynı enemy spawning pattern | 0.5 | — | Aynı gün aynı seed üretir, run tekrarlanabilir |
| S8-09 | **Screenshot Share** — Victory/Death ekranında screenshot butonu, dosyaya kaydet | 0.5 | — | Screenshot alınır, belirli klasöre kaydedilir |
| S8-10 | **Settings Genişletme** — Music volume ayrı slider, tutorial reset butonu | 0.5 | S8-02 | Music ve SFX ayrı kontrol edilebilir |
| S8-11 | **Müzik dosyalarını organize et** — Assets/Art'tan Assets/Audio/Music'e taşı, klasör yapısı düzenle | 0.25 | — | Müzik dosyaları doğru klasörde |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S8-12 | **Visual polish** — Ana menü particle effect, buton hover animasyonu | 0.5 | — | Menü daha canlı hissettiriyor |
| S8-13 | **Loading screen** — Sahne geçişlerinde loading indicator | 0.25 | — | Geçişlerde siyah ekran yerine loading var |
| S8-14 | **Controller/Gamepad desteği** — Input System ile gamepad mapping | 1.0 | — | Xbox/PS controller ile oynanabilir |

## Carryover from Sprint 7

| Task | Reason | New Estimate |
|------|--------|-------------|
| S7-09 Adaptive Music (gameplay) | Sadece menu müziği eklendi, gameplay stem wiring eksik | S8-01 + S8-04 (1.5 gün) |
| S7-10 Tutorial/Onboarding | Ertelendi | S8-02 (1.0 gün) |
| S7-11 Local Leaderboard | Ertelendi | S8-03 (0.5 gün) |
| S7-12 Daily Seed | Nice to Have → Should Have | S8-08 (0.5 gün) |
| S7-13 Screenshot Share | Ertelendi | S8-09 (0.5 gün) |
| S7-14 Performance Profiling | Ertelendi | S8-05 (1.0 gün) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Adaptive music stem seçimi zor (hangi track hangi intensity) | Orta | 0.5 gün | Config SO'da deneyerek ayarla, playtest ile iterate |
| Tutorial overlay UI layouting | Düşük | 0.25 gün | Basit Canvas overlay, TextMeshPro |
| Performance profiling ciddi bottleneck bulursa | Orta | 1-2 gün ek | Sadece belgeleme, optimizasyon Sprint 9'a |
| Gamepad input mapping karmaşıklığı | Düşük | Nice to Have, atlanabilir | Input System action map zaten var |

## Dependencies on External Factors
- Dark Fantasy Music Pack stem'leri (✅ mevcut)
- Build target platform: Windows + Mac standalone
- Playtest feedback toplanacak kişiler

## Sprint 8 Sonunda Beklenen Durum
- **Adaptive müzik** gameplay'de aktif — wave yoğunluğuna göre değişir
- **Tutorial overlay** — yeni oyuncu dostu
- **Local leaderboard** — rekabet motivasyonu
- **Performance profili** — bottleneck'ler belgelenmiş
- **Temiz build** — 0 bug, 0 console error
- **Feedback-ready** — dağıtılabilir, anlaşılabilir, çalışır build

## Definition of Done for this Sprint
- [ ] Gameplay müziği wave'e göre değişir (4 intensity tier)
- [ ] Tutorial overlay ilk run'da gösterilir
- [ ] Leaderboard top 10 Main Menu'de görünür
- [ ] Performance profiling raporu var
- [ ] 5+ playtest run sorunsuz
- [ ] Standalone build (Win+Mac) çalışır
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit + tag (v0.2-alpha)
