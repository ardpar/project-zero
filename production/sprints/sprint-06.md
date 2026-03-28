# Sprint 6 — 2026-04-12 to 2026-04-26

## Sprint Goal
Alpha aşamasına geçiş: Meta-progression (unlock sistemi), Save/Load, Settings, biome rotation, ve final polish. Sprint sonunda: birden fazla run oynandığında ilerleme hissedilen, kayıt edilebilen, ayarlanabilen bir oyun.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Alpha** (Post-Vertical Slice, Sprint 6-7)
- Sprint 6: Meta-progression + Persistence + Polish ← **Buradayız**
- Sprint 7: Content expansion + Community-ready build

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S6-01 | **Meta-progression: Unlock System** — Currency (Cells) earned per run based on performance. Unlock new mutations for future runs. UnlockManager + CellsData SO | 1.5 | — | Run sonunda Cell kazanılır, Menu'de unlock shop'tan yeni mutasyon açılır |
| S6-02 | **Save/Load System** — PlayerPrefs veya JSON file ile kayıt: unlocked mutations, toplam cells, run istatistikleri, settings | 1.0 | S6-01 | Oyun kapatıp açınca unlock'lar ve ayarlar korunur |
| S6-03 | **Settings Screen** — Ana menü + Pause'dan erişim: Master/SFX volume slider, screen shake toggle, fullscreen toggle | 1.0 | — | Ses ayarları çalışır, shake kapatılabilir, ayarlar kaydedilir |
| S6-04 | **Biome Rotation** — Her run'da rastgele biome seçimi (Crypt/Temple/Jungle/Hell). Tilemap runtime swap | 0.5 | — | Her yeni run farklı biome'da başlayabilir, tileset doğru değişir |
| S6-05 | **Run Summary + Cells Reward** — Victory/GameOver'da detaylı istatistik + kazanılan Cell miktarı. Cells = kills × 0.1 + waves × 5 + (victory ? 50 : 0) | 0.5 | S6-01 | Run sonunda Cell ödülü gösterilir, toplam Cell'e eklenir |
| S6-06 | **Unlock Shop UI** — Main Menu'de basit shop: mutation listesi, kilit/açık durumu, unlock butonu + Cell maliyeti | 1.5 | S6-01, S6-02 | Mutasyonlar Cell ile açılabilir, açılan mutasyonlar run'da havuza girer |
| S6-07 | **Bug Fix Pass** — Bilinen sorunları düzelt: Shooter enemy projectile collision, enemy alive count negatif olma riski, wave timer display | 1.0 | — | Bilinen 0 bug |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S6-08 | **Run History** — Son 10 run'ın istatistiklerini kaydet ve Main Menu'den görüntüle | 0.5 | S6-02 | Geçmiş run'lar listelenebilir |
| S6-09 | **Mutation Collection Screen** — Tüm mutasyonların listesi: keşfedilmiş/keşfedilmemiş, istatistikler | 0.5 | S6-02 | Koleksiyon ekranı Main Menu'den erişilebilir |
| S6-10 | **Loading Screen** — Scene geçişlerinde basit loading ekranı veya fade transition | 0.25 | — | Scene değişirken siyah fade |
| S6-11 | **Enemy projectile visual** — Shooter'ın projectile'ına farklı sprite (kırmızı/turuncu) | 0.25 | — | Düşman projectile'ı player projectile'dan ayırt edilebilir |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S6-12 | **Daily Seed** — Günlük sabit seed ile aynı run'ı paylaşma | 0.5 | — | Aynı gün aynı seed = aynı spawn pattern |
| S6-13 | **Minimap/Enemy Radar** — Küçük radar düşman yoğunluğunu gösterir | 0.5 | — | HUD'da düşman konumları görünür |
| S6-14 | **Kill counter popup** — 10/50/100 kill milestone popup | 0.25 | — | Kill milestone'larında ekranda kısa popup |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| S5-07 Playtest Balance Pass 2 | Devam eden — mutasyon güç dengesi playtest gerektiriyor | S6-07 içinde (bug fix + balance) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Save data corruption (JSON parse hatası) | Orta | Kayıp ilerleme | Try-catch + fallback defaults, save versioning |
| Unlock progression çok yavaş/hızlı | Orta | Motivasyon kaybı | Cell formülü SO-driven, playtest ile ayarla |
| Biome swap runtime'da tile glitch | Düşük | Görsel bozulma | Tilemap.ClearAllTiles + RefreshAllTiles |
| Settings PlayerPrefs platform uyumsuzluğu | Düşük | 0.25 gün | PlayerPrefs cross-platform çalışır, JSON fallback |

## Dependencies on External Factors
- Tüm Sprint 1-5 sistemleri çalışır durumda (✅)
- 4 biome tileset hazır (✅)
- 25 mutasyon + 11 synergy tanımlı (✅)

## Sprint 6 Sonunda Beklenen Durum (Alpha Ready)
- **Meta-progression:** Run'lar arası ilerleme — Cell kazanma, mutasyon açma
- **Persistence:** Unlock'lar ve ayarlar kayıtlı — oyun kapatıp açınca korunur
- **Settings:** Ses, shake, fullscreen ayarlanabilir
- **Biome variety:** Her run farklı biome'da olabilir
- **Unlock Shop:** Main Menu'den mutasyon açma
- **Alpha-ready build:** Dış test grubuna verilebilir

## Definition of Done for this Sprint
- [ ] Cell kazanma + harcama döngüsü çalışır
- [ ] En az 5 mutasyon başlangıçta kilitli, Cell ile açılabilir
- [ ] Save/Load: oyun kapatıp açınca unlock'lar korunur
- [ ] Settings: ses volume + shake toggle çalışır
- [ ] Her run farklı biome olabilir
- [ ] Bilinen 0 kritik bug
- [ ] Git'e commit edilmiş

## Post-Alpha (Sprint 7+)
- Leaderboard (online)
- Daha fazla mutasyon (40+ hedef)
- Daha fazla düşman türü
- Adaptive müzik sistemi
- Custom pixel art (ElvGames'i değiştir)
- Mobile port
- Steam page hazırlığı
