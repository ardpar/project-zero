# Sprint 1 — 2026-03-28 to 2026-04-11

## Sprint Goal
Unity projesini kurup Foundation + Core Layer sistemlerini production-quality olarak implement et. Sprint sonunda: oyuncu hareket eder, otomatik saldırır, düşmanlar gelir, XP toplanır, level atlanır. Playable core loop (mutasyon seçimi hariç).

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün (unplanned work, bug fixing)
- Available: 8 gün

## Milestone
- **MVP** (4-6 hafta, Sprint 1-3)
- Sprint 1: Foundation + Core (oynanabilir savaş döngüsü)
- Sprint 2: Feature Layer (mutasyonlar, synergy, dalgalar)
- Sprint 3: Presentation + Polish (HUD, UI, VFX, ses)

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | GDD | Acceptance Criteria |
|----|------|-----------|-------------|-----|-------------------|
| S1-01 | **Unity proje kurulumu** — 2D URP proje, folder structure (src/ layout), Assembly Definitions, Input System package, layer/tag setup | 0.5 | — | ADR-001 | Proje compile eder, Input System aktif, folder structure ADR-001'e uygun |
| S1-02 | **GameEvents + ObjectPool** — Event bus (static events), generic object pool, IPoolable interface | 0.5 | S1-01 | ADR-001 | Event fire/subscribe çalışır, pool Get/Return çalışır, 0 GC allocation |
| S1-03 | **Player Controller** — 360° hareket, dash (3u/0.15s/3s CD), circle collider, Input System, 6 state | 1.0 | S1-01 | player-controller.md | WASD+gamepad hareket, dash çalışır, arena bounds'ta durur, normalize edilmiş çapraz hareket |
| S1-04 | **Entity Health System** — HP component (oyuncu+düşman), TakeDamage, invulnerability window, OnDeath event, HP orb drop | 1.0 | S1-02 | entity-health-system.md | Hasar alır, min 1 damage, invuln çalışır, HP≤0'da OnDeath event, HP orb iyileştirir |
| S1-05 | **Camera System** — SmoothDamp follow, look-ahead, screen shake (TriggerShake API), arena confine | 0.5 | S1-03 | camera-system.md | Oyuncuyu takip eder, look-ahead yön değişiminde yumuşak, shake tetiklenebilir, arena dışı gösterilmez |
| S1-06 | **Auto-Attack System** — Cone targeting (±60°), attack interval, FireProjectile çağrısı, attack slots (başlangıçta 1) | 1.0 | S1-03, S1-04 | auto-attack-system.md | En yakın düşmana otomatik ateş, koni içi öncelik, menzilde düşman yoksa ateşlemez |
| S1-07 | **Projectile/Damage System** — Projectile hareket, collision, hasar hesabı (base × modifier × crit), object pool, max 300 | 1.0 | S1-06, S1-02 | projectile-damage-system.md | Projectile ateşlenir ve düşmana hasar verir, crit çalışır, pool kullanılır, 300 limit |
| S1-08 | **XP & Level-Up System** — XP gem spawn (düşman ölümünde), magnet pickup, stepped XP curve, level-up event | 1.0 | S1-04, S1-02 | xp-levelup-system.md | Gem spawn + manyetik toplama, XP bar dolar, level-up event tetiklenir, overflow XP taşınır |
| S1-09 | **Düşman AI (Chaser only)** — Basit chase hareket, temas hasarı, spawn from pool, wave-scaled HP/speed | 1.0 | S1-03, S1-04, S1-02 | enemy-ai-system.md | Düşman oyuncuya yürür, temas hasar verir, ölünce pool'a döner, HP dalga bazlı ölçeklenir |
| S1-10 | **Temel Wave Spawner** — İlk 3 dalga tablosu, zaman bazlı spawn, spawn pozisyonu (ekran dışı) | 0.5 | S1-09 | wave-spawning-system.md | 3 dalga doğru aralıklarla spawn eder, spawn ekran dışında, max alive limit çalışır |

**Toplam Must Have: 8.0 gün** (kapasite: 8 gün — sıkı ama yapılabilir)

### Should Have

| ID | Task | Est. Days | Dependencies | GDD | Acceptance Criteria |
|----|------|-----------|-------------|-----|-------------------|
| S1-11 | **Basit HP Bar** — Oyuncu HP göstergesi (UI Toolkit veya UGUI, minimal) | 0.5 | S1-04 | gameplay-hud.md | HP bar ekranda görünür, hasar alınca azalır |
| S1-12 | **Basit XP Bar** — XP dolum göstergesi + level numarası | 0.5 | S1-08 | gameplay-hud.md | XP bar dolar, level numarası güncellenir |
| S1-13 | **ScriptableObject data setup** — EnemyData, ProjectileData, PlayerConfig SO'ları | 0.5 | S1-01 | ADR-001 | Tüm hardcoded değerler SO'ya taşınmış, Inspector'dan düzenlenebilir |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S1-14 | **Düşman ölüm efekti** — Basit patlama partikül | 0.25 | S1-09 | Düşman ölünce küçük patlama görünür |
| S1-15 | **Hasar flash** — Düşman/oyuncu hasar alınca kısa beyaz flash | 0.25 | S1-04 | Hasar alınca sprite beyaz flash yapar |
| S1-16 | **Placeholder ses** — 2-3 temel SFX (ateş, hasar, ölüm) | 0.25 | S1-07 | Temel SFX duyulur |

## Carryover from Previous Sprint
_İlk sprint — carryover yok._

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Unity 6.3 Input System kurulumu beklenenden uzun sürer | Orta | 0.5 gün kayıp | Prototip scriptlerinden referans al, Input System dökümantasyonunu önceden oku |
| Object pool reset logic'i hatalı (state leak) | Orta | 1 gün debug | IPoolable interface ile reset garantisi, her component OnPoolReturn implement etsin |
| 200 düşman performansı yetersiz | Düşük | 1 gün optimize | Prototype'ta test edildi (basit chase), Burst Jobs Sprint 2'ye ertelenebilir |
| Collision layer setup karmaşıklığı | Düşük | 0.5 gün | Layer matrix'i önceden planla: Player, Enemy, Projectile, Wall, XPGem |

## Dependencies on External Factors
- Unity 6.3 LTS Hub'dan indirilmiş ve çalışır durumda olmalı
- Placeholder sprite'lar (basit renkli kareler) yeterli — asset üretimi gerekmez

## Collision Layer Matrix (Referans)

| | Player | Enemy | Projectile | Wall | XPGem |
|---|---|---|---|---|---|
| **Player** | — | ✓ hasar | — | ✓ solid | ✓ pickup |
| **Enemy** | ✓ hasar | — (overlap) | ✓ hasar | ✓ solid | — |
| **Projectile** | — | ✓ hit→destroy | — | — | — |
| **Wall** | ✓ | ✓ | — | — | — |
| **XPGem** | ✓ pickup | — | — | — | — |

## Sprint 1 Sonunda Beklenen Durum
- Unity projesi kurulu, production-ready folder structure
- Oyuncu hareket eder (WASD + dash), otomatik saldırır
- Düşmanlar spawn olur, kovalama yapar, temas hasarı verir
- Düşman ölünce XP gem düşürür, gem toplanır, XP bar dolar
- Level-up event tetiklenir (henüz mutasyon seçimi yok — Sprint 2'de)
- 3 dalga çalışır, zorluk artar
- Basit HP ve XP bar'ı ekranda (Should Have'den)

## Definition of Done for this Sprint
- [ ] Tüm Must Have task'lar tamamlandı
- [ ] Tüm task'lar acceptance criteria'yı karşılıyor
- [ ] Hiçbir S1/S2 bug yok
- [ ] Kod ADR-001 pattern'lerine uygun (EventBus, ObjectPool, ScriptableObject)
- [ ] GDD'lerden sapma varsa dokümente edilmiş
- [ ] 150 düşman ekranda 60fps korunuyor
- [ ] Git'e commit edilmiş

## Next Sprints (Yol Haritası)
- **Sprint 2** (Apr 11-25): Mutasyon Sistemi, Synergy Matrisi, Sprite Compositing, tam dalga tablosu, boss fight
- **Sprint 3** (Apr 25-May 9): HUD polish, Mutasyon Seçim UI, VFX/Juice, ses, playtest + balans
