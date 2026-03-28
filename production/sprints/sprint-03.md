# Sprint 3 — 2026-04-25 to 2026-05-09

## Sprint Goal
Presentation + Polish katmanını tamamla: VFX juice, HUD polish, placeholder SFX, Run Manager (win/lose), ve playtest balansı. Sprint sonunda: MVP tamamlanmış, 15 dakikalık oynanabilir ve hissedilen bir run.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün (unplanned work, bug fixing, playtest iterations)
- Available: 8 gün

## Milestone
- **MVP** (4-6 hafta, Sprint 1-3)
- Sprint 1: Foundation + Core ✅
- Sprint 2: Feature Layer ✅
- Sprint 3: Presentation + Polish ← **Buradayız (Final Sprint)**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | GDD | Acceptance Criteria |
|----|------|-----------|-------------|-----|-------------------|
| S3-01 | **VFX: Screen Shake integration** — Hasar alınca, düşman ölünce, boss spawn'da screen shake tetikle. CameraController.TriggerShake() event'lere bağla | 0.5 | — | vfx-juice-system.md | Hasar→light shake, ölüm→medium, boss→heavy. global_shake_multiplier=0 ile kapatılabilir |
| S3-02 | **VFX: Hit Flash** — Düşman/oyuncu hasar alınca sprite beyaz flash (0.05s). SpriteRenderer.material veya coroutine | 0.5 | — | vfx-juice-system.md | Hasar alınca sprite kısa beyaz yanıp söner |
| S3-03 | **VFX: Damage Numbers** — Hasar miktarı ekranda uçan text. Normal=beyaz, crit=sarı+büyük, heal=yeşil. Object pooled | 1.0 | — | vfx-juice-system.md, gameplay-hud.md | Hasar sayısı düşman üstünde belirir, yukarı süzülüp kaybolur. Crit 1.5x büyük + sarı |
| S3-04 | **VFX: Enemy Death Particles** — Düşman ölünce küçük patlama partikül efekti. ParticleSystem veya sprite burst | 0.5 | — | vfx-juice-system.md | Her düşman ölümünde partikül patlar, pool kullanılır |
| S3-05 | **HUD Polish** — Mevcut HP/XP bar'ları iyileştir: HP renk gradyanı (yeşil→kırmızı), armor göstergesi, dash CD indicator, aktif mutasyon ikonları, boss HP bar | 1.0 | — | gameplay-hud.md | HP bar renk değiştirir, dash CD görünür, mutasyon listesi HUD'da, boss'ta HP bar |
| S3-06 | **Run Manager** — Oyun başlangıcı, oyuncu ölümü (game over), boss öldürme (victory) state'leri. Basit Game Over / Victory ekranı | 1.0 | — | — | Oyuncu ölünce "Game Over" ekranı, boss öldürünce "Victory" ekranı, restart butonu |
| S3-07 | **Placeholder SFX (temel 10)** — Procedural/basit SFX: ateş, hasar, düşman ölüm, XP toplama, level-up, dash, mutasyon seçimi, synergy, boss spawn, game over | 1.0 | — | sfx-specification.md | 10 temel SFX duyulur, AudioSource + GameEvents bağlantısı çalışır |
| S3-08 | **Playtest Balance Pass** — 3+ tam run oyna, XP curve, spawn rate, damage tuning. SO değerlerini ayarla | 1.5 | S3-01→S3-07 | — | 15 dk run hedefi, ~15 level-up, zorluk eğrisi hissedilir, boss zorlu ama yenilebilir |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | GDD | Acceptance Criteria |
|----|------|-----------|-------------|-----|-------------------|
| S3-09 | **VFX: Low HP Vignette** — HP %25 altında kırmızı vignette overlay | 0.25 | S3-05 | vfx-juice-system.md | Düşük HP'de ekran kenarları kızarır |
| S3-10 | **VFX: Boss Death Slow-Mo** — Boss ölünce 0.5s slow-motion (timeScale=0.2) | 0.25 | S3-06 | vfx-juice-system.md | Boss ölünce kısa slow-mo, sonra victory ekranı |
| S3-11 | **Mutation Selection Polish** — Synergy hint göstergesi kartlarda, stat değişim preview | 0.5 | — | mutation-selection-ui.md | Kart altında "Synergy: X ile!" yazısı, stat değişiklikleri listelenir |
| S3-12 | **Pause Menu** — ESC ile pause, resume/quit butonları | 0.5 | — | ux-specification.md | ESC→pause overlay, resume devam eder, quit ana menüye döner |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S3-13 | **XP gem magnet visual** — Gem manyetik çekim sırasında iz bırakır | 0.25 | — | Gem oyuncuya çekilirken trail görünür |
| S3-14 | **Wave start/clear banner** — "Wave 3!" ve "Wave Clear!" animasyonlu text | 0.25 | — | Dalga başında/sonunda banner görünür |
| S3-15 | **Knockback on hit** — Düşman hasar alınca hafif geri itilir | 0.25 | — | Projectile hit'te düşman geri itilir |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| — | Sprint 2 tamamen tamamlandı, carryover yok | — |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Placeholder SFX kalitesi düşük → oyun hissi kötü | Orta | Oyun hissi %30 azalır | Prosedürel beep/boop yeterli, hacimden ziyade timing önemli |
| Damage numbers performans sorunu (200 düşman) | Düşük | Frame drop | Object pool + max 20 concurrent limit |
| Balance pass yetersiz kalır (1 sprint'te) | Yüksek | Oyun çok kolay/zor | SO'lar data-driven, hotfix ile hızlı ayarlanabilir |
| ParticleSystem Unity 6.3 uyumsuzluğu | Düşük | 0.5 gün | Sprite-based basit burst fallback hazırla |

## Dependencies on External Factors
- Tüm Sprint 1+2 sistemleri çalışır durumda (✅)
- Placeholder SFX için harici tool gerekmez (AudioSource.PlayOneShot + basit clip)
- Balance pass için en az 3 tam run gerekli

## Sprint 3 Sonunda Beklenen Durum (MVP Complete)
- **Tam 15 dakikalık run:** 6 dalga + boss, mutasyonlar, synergy'ler
- **Juice/Feel:** Screen shake, hit flash, damage numbers, death particles, dash trail
- **Audio:** 10 temel SFX (ateş, hasar, ölüm, XP, level-up, dash, mutasyon, synergy, boss, game over)
- **UI:** Polished HUD (HP gradient, armor, dash CD, mutasyon ikonları, boss HP)
- **Game Flow:** Run başlangıcı → gameplay → game over / victory → restart
- **Balance:** XP curve, spawn rate, damage değerleri playtest ile ayarlanmış
- **MVP DONE** — playtest'e ve feedback'e hazır

## Definition of Done for this Sprint (MVP DoD)
- [ ] Tüm Must Have task'lar tamamlandı
- [ ] 15 dakikalık tam run oynanabilir (baştan sona)
- [ ] Oyuncu ölünce Game Over, boss yenince Victory ekranı
- [ ] En az 10 SFX çalışıyor
- [ ] Screen shake + hit flash + damage numbers çalışıyor
- [ ] HUD tüm bilgileri gösteriyor (HP, XP, wave, mutations, dash CD)
- [ ] 3+ playtest run'ı yapılmış, balance ayarlanmış
- [ ] 150 düşman + VFX ile 60fps korunuyor
- [ ] Hiçbir S1/S2 bug yok
- [ ] Git'e commit edilmiş

## Post-MVP (Sprint 4+)
- Meta-progression (unlock system)
- Main menu + settings
- Daha fazla mutasyon (25+ hedef)
- Daha fazla düşman türü (Shooter aktifleştir)
- Gerçek pixel art sprite'lar
- Adaptive müzik sistemi
- Save/Load
- Leaderboard
