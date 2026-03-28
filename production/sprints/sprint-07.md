# Sprint 7 — 2026-04-26 to 2026-05-10

## Sprint Goal
Community-ready build: daha fazla content (40 mutasyon), collection ekranı UI, run history UI, enemy projectile layer, balance pass, ve bug-free polish. Sprint sonunda: arkadaşlara gönderilebilir, feedback toplanabilir build.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Alpha** (Sprint 6-7)
- Sprint 6: Meta-progression + Persistence ✅
- Sprint 7: Content Expansion + Community Polish ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S7-01 | **15 yeni mutasyon (40 toplam)** — Daha fazla çeşitlilik: 4 yeni slot alternatifi + 11 yeni pasif. Yeni synergy tag'leri | 1.5 | — | MutationDatabase 40 mutasyon, yeni kartlar çıkar |
| S7-02 | **5 yeni synergy (16 toplam)** — Yeni mutasyonların tag'leriyle oluşan kombinasyonlar | 0.5 | S7-01 | 16 synergy tanımlı ve tetiklenebilir |
| S7-03 | **Collection Screen UI** — Main Menu'den erişim: tüm mutasyonlar grid'de, keşfedilmiş/keşfedilmemiş, detay popup | 1.0 | — | Koleksiyon ekranı mutasyonları gösterir, keşfedilmemişler gizli |
| S7-04 | **Run History UI** — Main Menu'den erişim: son 10 run listesi, tarih/süre/kill/level/cells | 0.5 | — | Geçmiş run'lar tablo halinde gösterilir |
| S7-05 | **Enemy Projectile Layer** — EnemyProjectile layer (11), Shooter projectile bu layer'da, Player ile collision açık, Player Projectile ile kapalı | 1.0 | — | Düşman projectile'ı farklı renk+layer, oyuncuya hasar verir |
| S7-06 | **Comprehensive Bug Fix** — Tüm bilinen bugları düzelt, edge case'leri test et, NullRef temizliği | 1.0 | — | 0 bilinen bug, console'da 0 error |
| S7-07 | **Final Balance Pass** — 10+ run playtest, SO değerleri fine-tune: XP curve, spawn rate, damage, mutation power, cell economy | 1.5 | S7-01→S7-06 | 12-15dk run, smooth zorluk eğrisi, 10 run'da cell economy dengeli |
| S7-08 | **Build & Distribution** — Standalone build (Windows/Mac), README, version numbering | 0.5 | S7-07 | Build çalışır, README var, versiyonlanmış |

**Toplam Must Have: 7.5 gün** (kapasite: 8 gün — 0.5 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S7-09 | **Adaptive Music** — Dark Fantasy müzik paketinden loop, wave intensity'ye göre stem switch | 1.0 | — | Müzik çalar, yoğunluk arttıkça müzik yoğunlaşır |
| S7-10 | **Tutorial/Onboarding** — İlk run'da basit overlay: WASD hareket, Space dash, otomatik saldırı, gem topla | 0.5 | — | İlk kez oynayan oyuncu kontrolleri anlayabilir |
| S7-11 | **Leaderboard (local)** — En iyi 10 run sıralaması (survival time) Main Menu'de | 0.25 | — | Top 10 local leaderboard gösterilir |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S7-12 | **Daily Seed** — Günlük sabit seed | 0.25 | — | Aynı gün aynı seed |
| S7-13 | **Screenshot share** — Victory ekranında screenshot butonu | 0.25 | — | Screenshot kaydedilir |
| S7-14 | **Performance profiling** — 200 enemy + VFX 60fps doğrulama | 0.5 | — | Profiler data toplanmış, bottleneck'ler belgelenmiş |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| S6-11 Enemy projectile visual | Ayrı layer gerekiyor | S7-05 içinde |
| S6-12 Daily Seed | Ertelendi | S7-12 (Nice to Have) |
| S6-13 Minimap/Radar | Ertelendi | Post-Alpha |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| 40 mutasyon balansı çok zor | Yüksek | 1-2 gün | SO'lar data-driven, playtest ile iterasyon |
| Standalone build Unity 6.3 uyumsuzluğu | Düşük | 0.5 gün | Build early, test early |
| Cell economy çok generous/stingy | Orta | Balance bozuk | SO'dan ayarlanabilir, 10 run playtest |
| EnemyProjectile layer collision matrix karmaşıklığı | Düşük | 0.5 gün | GameBootstrap'ta force matrix |

## Dependencies on External Factors
- ElvGames Dark Fantasy Music paketleri (✅ mevcut)
- Build target platform: Windows + Mac standalone

## Sprint 7 Sonunda Beklenen Durum (Alpha Complete)
- **40 mutasyon**, 16 synergy — çeşitli run deneyimleri
- **Collection ekranı** — keşif motivasyonu
- **Run history** — ilerleme görünümü
- **Düşman projectile** — Shooter gerçek tehdit
- **0 bilinen bug**
- **Standalone build** — paylaşılabilir
- **Community-ready** — feedback toplanabilir

## Definition of Done for this Sprint
- [ ] 40 mutasyon seçilebilir
- [ ] 16 synergy tetiklenebilir
- [ ] Collection + Run History UI çalışır
- [ ] Enemy projectile ayrı layer'da, oyuncuya hasar verir
- [ ] 10+ playtest run tamamlanmış
- [ ] Standalone build (Win+Mac) çalışır
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit + tag (v0.1-alpha)
