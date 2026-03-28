# Sprint 4 — 2026-03-29 to 2026-04-12

## Sprint Goal
ElvGames asset paketini entegre et: gerçek sprite'lar, tileset arena, gerçek SFX, spell ikonları, UI kit. Sprint sonunda: placeholder renkli kareler yerine gerçek pixel art ile oynanan MVP.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Vertical Slice** (Post-MVP, Sprint 4-5)
- Sprint 4: Asset Entegrasyonu + Visual Polish ← **Buradayız**
- Sprint 5: Meta-progression, Main Menu, Save/Load

## Available Asset Inventory (ElvGames)
- 30 Hero character spritesheets (192×384, 16×16 frames)
- 24 enemy types × 4 variants (192×160 spritesheets)
- 10 boss spritesheets (480×480)
- 18+ tilesets (Crypt, Graveyard, Temple, Jungle, Ruins, Hell, Atlantis...)
- 11 spell icon sheets (fire, ice, lightning, dark, poison, earth/water)
- 279 SFX files (magic, UI, footsteps, achievements)
- 4 UI kits (buttons, panels, frames)
- Pixel fonts (5x5, 6x6, 7x7)

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S4-01 | **Sprite Import Settings** — Tüm sprite'lar için import ayarları: Pixels Per Unit=16, Filter Mode=Point, Compression=None, Sprite Mode=Multiple | 0.5 | — | Tüm sprite'lar pixel-perfect, bulanık değil |
| S4-02 | **Player Sprite** — Hero_01 spritesheet'i slice et (idle/walk/dash animasyonları), Player prefab'a ata, Animator Controller oluştur | 1.0 | S4-01 | Player gerçek karakter sprite'ı ile görünür, idle+walk animasyonu çalışır |
| S4-03 | **Enemy Sprites** — Enemy_001→Chaser, Enemy_002→Runner, Enemy_003→Exploder. Spritesheet slice, prefab'lara ata, idle/walk anim | 1.0 | S4-01 | 3 düşman türü farklı sprite ile görünür, hareket animasyonu var |
| S4-04 | **Boss Sprite** — Boss04 (Minotaur) spritesheet slice, Boss prefab'a ata, idle/walk anim | 0.5 | S4-01 | Boss büyük sprite ile görünür, animasyonlu |
| S4-05 | **Arena Tileset** — Crypt veya Graveyard tileset'ini arena zemin olarak ata. Tilemap oluştur, duvarları tile ile çiz | 1.5 | S4-01 | Arena düz renk yerine tileset ile çizilmiş, duvarlar görsel |
| S4-06 | **Spell Icons → Mutation Kartları** — Spell icon sheet'lerinden slice, 14 mutasyonun icon field'ına ata, MutationSelectionUI'da göster | 1.0 | S4-01 | Mutasyon kartlarında gerçek ikon görünür |
| S4-07 | **Gerçek SFX Entegrasyonu** — Magic & Spells → ateş/hasar/ölüm SFX. UI Pack → click/level-up/mutasyon. SFXManager'daki procedural tonları gerçek clip'lerle değiştir | 1.0 | — | Procedural beep yerine gerçek SFX duyulur |
| S4-08 | **Projectile Sprite** — Spell Icons'dan uygun bir ikon veya basit ateş topu sprite'ı kes, Projectile prefab'a ata | 0.5 | S4-01 | Projectile görsel olarak tanınabilir (ateş topu/enerji) |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S4-09 | **XP Gem + HP Orb sprite** — İtem icon sheet'inden gem ve orb sprite'ları kes, prefab'lara ata | 0.25 | S4-01 | Gem yeşil kristal, HP orb kırmızı kalp görünümünde |
| S4-10 | **UI Kit Entegrasyonu** — UGUI panel/buton arka planlarını ElvGames UI Kit sprite'ları ile değiştir (mutation panel, game over, victory, HUD frame) | 1.0 | S4-01 | UI elementleri pixel art çerçeveli, profesyonel görünüm |
| S4-11 | **Pixel Font** — Neatpixels fontunu HUD, kart, panel text'lerine uygula | 0.25 | — | Tüm oyun içi text'ler pixel font ile |
| S4-12 | **Enemy Death Anim** — Düşman ölüm sprite animasyonu (poof/explode frame'leri) veya mevcut particle'ı iyileştir | 0.5 | S4-03 | Düşman ölünce görsel patlama animasyonu |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S4-13 | **Tileset Varyasyonları** — 2-3 farklı tileset hazırla (Crypt, Temple, Jungle) gelecekte biome rotation için | 0.5 | S4-05 | 3 tileset palette hazır, değiştirilebilir |
| S4-14 | **Karakter Animasyon Polish** — Dash animasyonu, hasar alma animasyonu (hit reaction) | 0.5 | S4-02 | Dash'te farklı frame, hasar alınca kısa anim |
| S4-15 | **Boss Giriş Animasyonu** — Boss spawn olunca kısa cinematic (zoom + intro frame) | 0.5 | S4-04 | Boss sahneye dramatik giriş yapar |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| S3-08 Playtest Balance Pass | SO değerlerinin ince ayarı devam ediyor | Ongoing (asset entegrasyonu sonrası) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Spritesheet slice boyutları tahmin edilenden farklı | Orta | 0.5 gün | İlk sprite'ı slice edip boyut doğrula, sonra diğerlerine uygula |
| Tilemap Unity 6.3 2D uyumluluğu | Düşük | 0.5 gün | ElvGames Crypt tileset zaten Unity example scene içeriyor |
| Animator Controller karmaşıklığı | Orta | 1 gün | Basit 2-state (idle→walk) yeterli, dash/attack sonraya |
| 279 SFX dosyasından doğru olanları seçmek zaman alır | Düşük | 0.5 gün | Kategori bazlı hızlı seçim: Magic→attack, UI→feedback |

## Dependencies on External Factors
- ElvGames asset paketi import edilmiş durumda (✅ Assets/ElvGames/)
- Unity Tilemap package kurulu olmalı (URP 2D default olarak var)

## Sprint 4 Sonunda Beklenen Durum
- Placeholder renkli kareler YOK — tüm entity'ler gerçek pixel art sprite
- Arena tileset ile çizilmiş (Crypt/Graveyard temalı)
- Mutasyon kartlarında spell ikonları
- Gerçek SFX (magic, UI, footsteps)
- Pixel font ile HUD text
- Profesyonel görünen, oynanabilir demo

## Definition of Done for this Sprint
- [ ] Player, 3 düşman, boss gerçek sprite ile görünür
- [ ] En az idle + walk animasyonu çalışır
- [ ] Arena tileset ile çizilmiş
- [ ] 14 mutasyon kartında gerçek ikon var
- [ ] Procedural SFX yerine gerçek ses dosyaları çalar
- [ ] Projectile, XP gem, HP orb gerçek sprite ile
- [ ] 150 düşman + sprite'lar ile 60fps korunuyor
- [ ] Git'e commit edilmiş
