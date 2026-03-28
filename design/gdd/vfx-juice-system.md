# VFX / Juice System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Görsel Evrim Tatmini

## Overview

VFX/Juice System, oyundaki tüm görsel geri bildirimleri (screen shake, partiküller, flash efektleri, damage numbers, slow-mo) merkezi olarak yöneten sistemdir. Diğer sistemler event yayınlar, bu sistem uygun görsel/his efektini tetikler. "Juice" oyunun "hissini" belirler — mekanikler aynı kalsa bile juice olmadan oyun cansız hisseder. Her eylem, her hasar, her ölüm bir görsel tepki almalı.

## Player Fantasy

Oyun "canlı" hissetmeli. Düşman öldüğünde patlama, hasar aldığında ekran titreyişi, level-up'ta flash, boss yenildiğinde slow-mo + confetti. Her eylem oyuncuya "bir şey oldu ve önemliydi" mesajı vermeli. Kaos güzel ve kontrollü görünmeli — görsel gürültü değil, koreografili bir dans.

## Detailed Rules

### Core Rules

**Event-Driven Mimari:**
1. VFX System event listener'dır — kendi başına efekt tetiklemez
2. Diğer sistemler `VFXEvent(type, position, intensity, data)` yayınlar
3. VFX System event'i alır ve uygun efekti oynar
4. Efektler object pool'dan alınır (GC allocation yok)

**Efekt Kategorileri:**

| Kategori | Örnekler | Kaynak Sistem |
|----------|---------|---------------|
| **Screen Shake** | Hasar alma, patlama, boss ölüm | Camera System (tetikleme) |
| **Flash** | Hasar alma (kırmızı), level-up (beyaz), crit (sarı) | Entity Health, XP, Projectile |
| **Partiküller** | Düşman ölüm, XP gem toplama, dash trail, patlama | Hepsi |
| **Damage Numbers** | Hasar rakamları (normal, crit), heal rakamları | Projectile/Damage, Entity Health |
| **Slow-Mo** | Boss ölüm, synergy aktivasyonu | Dalga Sistemi, Synergy |
| **Afterimage** | Dash sırasında oyuncu afterimage trail | Player Controller |
| **Overlay** | Low HP vignette, level-up flash | Entity Health, XP |

**Screen Shake** (Camera System'e delegate):
5. `TriggerShake(intensity, duration)` — Camera System Perlin noise shake uygular
6. Shake intensity sınıfları: Light(0.1), Medium(0.25), Heavy(0.4), Epic(0.5)

**Hit Flash:**
7. Entity sprite'ına kısa beyaz/kırmızı flash (0.05-0.1 sn)
8. Shader property override veya SpriteRenderer color tint
9. Flash stack etmez — yeni flash eskisini override eder

**Particle Effects:**
10. Tüm partiküller Unity Particle System ile
11. Düşman ölüm: küçük patlama (8-12 parçacık, 0.3 sn)
12. XP gem toplama: sparkle trail (gem → oyuncu)
13. Dash afterimage: yarı-saydam oyuncu kopyası (0.1 sn interval, 0.2 sn fade)

**Damage Numbers:**
14. World-space text popup, yukarı doğru yüzer (1 sn)
15. Normal: beyaz, küçük font
16. Crit: sarı/turuncu, büyük font, bounce (scale up → down)
17. Heal: yeşil, küçük font, yukarı kayma
18. Object pool: max 30 aktif damage number

**Time Scale Effects:**
19. `SlowMo(scale, duration)`: time scale geçici olarak düşürülür
20. Boss ölüm: 0.2x time scale, 0.5 sn → normal'e smooth dönüş
21. Synergy: 0.5x, 0.3 sn
22. Slow-mo sırasında UI normal hızda (unscaled time)

### States and Transitions

Stateless — event bazlı. Her event bağımsız efekt tetikler.

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Entity Health** | Upstream | Hasar alma → flash + shake. Ölüm → patlama. Low HP → vignette. |
| **Projectile/Damage** | Upstream | İsabet → impact efekti + damage number. Crit → büyük efekt. AoE → patlama dairesi. |
| **Player Controller** | Upstream | Dash → afterimage trail. |
| **XP & Level-Up** | Upstream | Gem toplama → sparkle. Level-up → screen flash. |
| **Dalga/Spawning** | Upstream | Boss ölüm → slow-mo + confetti. Dalga geçişi → hafif shake. |
| **Synergy Matrisi** | Upstream | Synergy aktivasyon → slow-mo + banner flash. |
| **Mutasyon Sistemi** | Upstream | Slot mutasyonu ekleme → morph parıltı. |
| **Camera System** | Downstream | Screen shake komutları. |

## Formulas

### Shake Intensity Mapping
| Event | Intensity | Duration |
|-------|-----------|----------|
| Oyuncu hasar | 0.15 | 0.1 sn |
| Düşman ölüm (normal) | 0.0 (yok) | — |
| Elite ölüm | 0.2 | 0.15 sn |
| Boss ölüm | 0.5 | 0.3 sn |
| Exploder patlama | 0.3 | 0.2 sn |
| Level-up | 0.1 | 0.1 sn |
| Synergy aktivasyon | 0.2 | 0.15 sn |

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 50 düşman aynı frame'de ölür | Patlama efektleri pool'dan — max 30. Fazlası skip. Shake stack etmez. | Performans |
| Slow-mo sırasında level-up | Time scale zaten düşük → level-up time scale 0'a override eder | Level-up her zaman pause |
| Damage number çok fazla | Max 30 pool, en eskisi recycle | Okunabilirlik |
| Pause sırasında efektler | Tüm efektler time scale'e bağlı → pause'da donar | Tutarlı pause |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| Tüm gameplay sistemleri | Upstream | Soft — event dinler. VFX yoksa oyun çalışır, sadece "cansız" hisseder |
| **Camera System** | Downstream | Soft — shake komutları |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect |
|-----------|---------|------------|--------|
| `global_shake_multiplier` | 1.0 | 0.0-2.0 | Tüm shake'leri scale (0 = shake kapalı, accessibility) |
| `damage_number_duration` | 0.8 | 0.5-1.5 | Rakam ekranda kalma |
| `max_particles_per_effect` | 12 | 6-20 | Patlama parçacık sayısı |
| `afterimage_interval` | 0.05 | 0.03-0.1 | Dash trail sıklığı |
| `afterimage_fade_duration` | 0.2 | 0.1-0.4 | Trail kaybolma hızı |
| `slowmo_boss_scale` | 0.2 | 0.1-0.4 | Boss ölüm slow-mo miktarı |
| `slowmo_boss_duration` | 0.5 | 0.3-1.0 | Boss ölüm slow-mo süresi |

## Visual/Audio Requirements

VFX System'in kendisi ses üretmez — görsel efekt tetikler, ses diğer sistemlerin sorumluluğunda. Ancak bazı efektler ses ile eşleşmeli:

| VFX Efekti | Eşleşen Ses (başka sistemde) |
|------------|-------------------------------|
| Patlama partikülleri | "death pop" SFX |
| Crit damage number | "crit" SFX |
| Slow-mo | Ses pitch düşer (audio time scale) |
| Screen flash (level-up) | Level-up jingle |

## Acceptance Criteria

- [ ] Screen shake event'lere göre tetiklenir
- [ ] Hit flash düşman/oyuncu sprite'ında çalışır
- [ ] Düşman ölüm partikül efekti oynar
- [ ] Damage numbers doğru renk/boyutta gösterilir (normal vs crit vs heal)
- [ ] Dash afterimage trail çalışır
- [ ] Slow-mo time scale doğru uygulanır, UI etkilenmez
- [ ] Low HP vignette efekti çalışır
- [ ] Efektler object pool kullanır (GC yok)
- [ ] **Performance:** Tüm VFX update < 1ms (200 düşman senaryosunda)
- [ ] `global_shake_multiplier = 0` ile shake tamamen kapatılabilir (accessibility)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Accessibility: shake/flash kapatma seçeneği settings'e eklensin mi? | Accessibility Specialist | Vertical Slice | Evet — global_shake_multiplier + flash_enabled toggle |
| Hit-stop (freeze frame on hit) eklensin mi? | Game Designer | MVP prototype | Prototipte test — 1-2 frame hit-stop tatmin edici olabilir |
