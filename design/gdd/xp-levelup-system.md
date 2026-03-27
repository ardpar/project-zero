# XP & Level-Up System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Synergy Keşfi, Kolay Giriş/Derin Derinlik

## Overview

XP & Level-Up System, düşman ölümlerinden XP gem'leri düşüren, oyuncunun bunları toplayarak deneyim kazandığı ve level atlayarak mutasyon seçimi yaptığı sistemdir. Run içi ilerlemenin motoru — her level-up bir mutasyon ekleme fırsatı sunar. XP eğrisi kademeli (stepped): ilk level'lar hızlı (sık mutasyon = hızlı güçlenme hissi), geç level'lar yavaş (uzun run'larda güç spirali yavaşlar, zorluk artar).

## Player Fantasy

Level-up anı run'ın en heyecanlı anı olmalı. XP barı dolarken "neredeyse level atlıyorum!" beklentisi, level-up geldiğinde "hangi mutasyonu seçsem?" heyecanı. Gem toplama fiziksel olarak tatmin edici olmalı — manyetik çekim ile gem'ler oyuncuya akıyor, sayılar yükseliyor. "Bir tane daha öldüreyim, level atlayacağım" = "bir tane daha" hissinin mikro versiyonu.

## Detailed Design

### Core Rules

**XP Gem'leri:**
1. Düşman öldüğünde XP gem düşürür (her zaman, %100 şans)
2. Gem XP değeri düşman türüne göre belirlenir: `gem_xp = enemy_base_xp * tier_multiplier`
3. Tier çarpanları: Normal = 1x, Elite = 5x, Boss = 20x
4. Gem'ler fiziksel obje olarak yere düşer, belirli süre kalır
5. Gem `xp_gem_lifetime` (varsayılan: 30 sn) sonra kaybolur

**Gem Toplama:**
6. Oyuncu gem'in `pickup_radius` (varsayılan: 1.0 birim) içine girdiğinde gem manyetik çekime girer
7. Manyetik çekim: gem oyuncuya doğru hızlanarak hareket eder (`magnet_speed`: 15 birim/sn)
8. Mutasyonlarla `magnet_radius` artırılabilir (daha uzaktan çekim)
9. Gem oyuncuya temas ettiğinde XP eklenir ve gem yok olur

**XP ve Level-Up:**
10. XP barı `current_xp` / `xp_to_next_level` olarak dolar
11. `current_xp >= xp_to_next_level` → Level-up tetiklenir
12. Level-up: oyun duraklar (time scale = 0), Mutasyon Seçim UI açılır
13. Mutasyon seçildikten sonra oyun devam eder
14. Fazla XP (overflow) bir sonraki level'a taşınır
15. Başlangıç level: 1, teorik maksimum: sınırsız (ama run süresi sınırlı)

**Kademeli XP Eğrisi:**
16. XP gereksinimleri tasarımcı tarafından level aralıklarına göre tanımlanır (data-driven)
17. Varsayılan eğri:

| Level Aralığı | XP Gereksinimi (per level) | Kümülatif (yaklaşık) | Amaç |
|---------------|---------------------------|----------------------|------|
| 1-5 | 20, 25, 30, 35, 40 | 150 | Hızlı başlangıç, ilk 5 mutasyon ~2 dk |
| 6-10 | 50, 60, 70, 80, 100 | 510 | Orta hız, build şekillenmeye başlar |
| 11-15 | 120, 150, 180, 220, 270 | 1450 | Yavaşlama, her mutasyon değerli |
| 16-20 | 330, 400, 500, 600, 750 | 4030 | Geç oyun, nadiren level atlama |
| 21+ | 1000+ (artarak) | — | Bonus level'lar, çok uzun run'lar için |

18. Bu tablo ScriptableObject olarak saklanır, her entry düzenlenebilir
19. Tanımlanmamış level'lar için fallback: `xp_required = previous * growth_rate` (varsayılan growth_rate: 1.25)

**XP Modifier:**
20. Mutasyonlar `xp_gain_modifier` sağlayabilir: `effective_xp = gem_xp * (1 + xp_gain_modifier)`
21. Modifier gem toplandığında uygulanır (gem'in kendi değeri değişmez)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Collecting** | Run başlangıcı | Level-up / Run sonu | XP barı doluyor, gem'ler toplanıyor |
| **Level-Up** | XP ≥ xp_to_next_level | Mutasyon seçimi yapıldı | Oyun duraklar, Mutasyon Seçim UI açılır |
| **Post-Select** | Mutasyon seçildi | XP barı tekrar dolmaya başlar | Overflow XP uygulanır, çoklu level-up kontrolü |

**Not:** Eğer overflow XP bir sonraki level'ı da karşılıyorsa, hemen tekrar Level-Up tetiklenir (zincir level-up mümkün — boss öldürmede olabilir).

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Entity Health** | Upstream | Düşman OnDeath → gem spawn tetikler (düşman pozisyonunda) |
| **Mutasyon Sistemi** | Downstream | Level-up → Mutasyon Seçim UI tetiklenir → seçilen mutasyon Mutasyon Sistemine eklenir |
| **Mutasyon Seçim UI** | Downstream | Level-up event → UI açılır, 3 mutasyon sunulur |
| **Player Controller** | Upstream | Oyuncu pozisyonu (gem pickup radius kontrolü) |
| **Mutasyon Sistemi** | Upstream sağlayıcı | `xp_gain_modifier`, `magnet_radius_modifier` |
| **Gameplay HUD** | Downstream | XP bar doluluk oranı, level numarası |
| **Dalga/Spawning** | Dolaylı | Daha çok düşman = daha çok XP = daha hızlı level-up (zorluk-ödül dengesi) |

## Formulas

### XP Gem Value
```
gem_xp = enemy_base_xp * tier_multiplier
effective_xp = gem_xp * (1 + xp_gain_modifier)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| enemy_base_xp | int | 1-50 | Düşman data file | Tür bazlı XP değeri |
| tier_multiplier | float | 1/5/20 | Düşman tier | Normal/Elite/Boss |
| xp_gain_modifier | float | 0.0-1.0 | Mutasyon Sistemi | XP kazanım artışı |

**Örnek:** Bat (base 3), Normal, modifier 0 → 3 XP
**Örnek:** Bat, Elite, modifier 0.3 → `3 * 5 * 1.3 = 19.5 → 20 XP`

### Level-Up XP Requirement
```
// Tanımlı level'lar için:
xp_required = xp_table[level]

// Tanımsız level'lar için fallback:
xp_required = xp_table[last_defined_level] * growth_rate ^ (level - last_defined_level)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| xp_table | int[] | per-level | ScriptableObject | Kademeli XP tablosu |
| growth_rate | float | 1.1-1.5 | Tuning knob | Fallback büyüme oranı |

### Level-Up Pace Estimate (12 min run)
- Dakika 0-2: ~Level 5 (5 mutasyon)
- Dakika 2-5: ~Level 10 (10 mutasyon)
- Dakika 5-8: ~Level 13 (13 mutasyon)
- Dakika 8-12: ~Level 15-16 (15-16 mutasyon)

**Hedef:** Ortalama run'da 15 mutasyon seçimi. Bu 4 slotu çeşitli şekillerde doldurur + synergy fırsatları yaratır.

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Overflow XP birden fazla level karşılar | Zincir level-up: her biri için ayrı mutasyon seçimi açılır | Boss kill sonrası 2-3 level atlama mümkün olmalı |
| Gem ekranda çok fazla (500+) | Eski gem'ler erken expire eder (FIFO), yeni gem'ler öncelikli | Performans + ekran temizliği |
| Oyuncu gem'lerden uzakta (toplama zahmetli) | Manyetik çekim mutasyonları bunu çözer — manyetik radius artışı teşvik edilir | Doğal game feel çözümü |
| xp_gain_modifier çok yüksek | Clamp: max 1.0 (%100 bonus). Çok hızlı level-up run dengesini bozar | 2x XP hız sınırı |
| Level-up sırasında düşmanlar | Oyun duraklar (time scale 0) — düşmanlar donar | Seçim stressi olmamalı |
| Run sonunda (boss yenildi) toplanan XP | Meta-XP'ye dönüştürülür veya kaybolur (tasarlanacak) | Run Manager ile koordine |
| Gem lifetime dolu ama toplananamadı | Kaybolma animasyonu (fade + shrink), kaybolmadan önce daha parlak yanıp söner (uyarı) | Oyuncu gem'in kaybolacağını görebilsin |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Entity Health** | Upstream | Hard — düşman ölümü gem spawn tetikler |
| **Player Controller** | Upstream | Hard — pozisyon (pickup radius kontrolü) |
| **Mutasyon Sistemi** | Downstream | Hard — level-up → mutasyon seçimi |
| **Mutasyon Seçim UI** | Downstream | Hard — level-up UI tetikleme |
| **Mutasyon Sistemi** | Upstream sağlayıcı | Soft — xp_gain_modifier, magnet_radius_modifier |
| **Gameplay HUD** | Downstream | Soft — XP bar, level göstergesi |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `xp_table` (per-level) | Bkz. eğri tablosu | Her entry ayrı ayarlanır | Level-up yavaşlar | Level-up hızlanır |
| `growth_rate` (fallback) | 1.25 | 1.1-1.5 | Geç level'lar çok yavaş | Geç level'lar erişilebilir |
| `pickup_radius` | 1.0 | 0.5-2.0 | Kolay toplama | Zor toplama |
| `magnet_speed` | 15.0 | 8.0-25.0 | Hızlı gem çekimi | Yavaş çekim |
| `xp_gem_lifetime` | 30 | 15-60 | Gem'ler uzun kalır | Gem'ler hızlı kaybolur |
| `max_active_gems` | 500 | 200-800 | Daha fazla gem ekranda | Erken temizlik |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Gem spawn | Gem sprite (renkli, boyut = XP değerine göre) | Hafif "clink" | High / Low |
| Gem toplama | Gem oyuncuya doğru uçar, parıltı trail | "collect" pling (pitch = combo) | High / High |
| XP bar dolma | Bar animasyonu (smooth fill) | — | High / — |
| Level-up tetikleme | Ekran flash (beyaz), level-up banner, fanfare | Level-up jingle (kısa, tatmin edici) | High / High |
| Gem kaybolma | Fade out + yanıp sönme (son 3 sn) | — | Medium / — |
| Zincir level-up | Her biri için ayrı flash + banner | Her biri için jingle | Medium / High |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| XP bar | HUD altında veya üstünde (belirgin) | Her frame (smooth) | Her zaman |
| Level numarası | XP bar yanında | Level-up'ta | Her zaman |
| "+XP" popup | Gem toplama pozisyonunda | Her gem toplama | Opsiyonel (devre dışı bırakılabilir — çok sık) |
| Level-up banner | Ekran ortası | Level-up anında (1-2 sn) | Level-up tetiklendiğinde |

## Acceptance Criteria

- [ ] Düşman ölümünde XP gem spawn olur (tür/tier bazlı değer)
- [ ] Gem oyuncu pickup_radius'ına girdiğinde manyetik çekime girer
- [ ] Gem toplandığında XP eklenir (modifier uygulanmış)
- [ ] XP eğrisi kademeli tablodan okunur
- [ ] Level-up'ta oyun duraklar ve Mutasyon Seçim UI açılır
- [ ] Overflow XP sonraki level'a taşınır, zincir level-up çalışır
- [ ] Gem'ler lifetime sonunda kaybolur
- [ ] Max aktif gem limiti çalışır (FIFO temizlik)
- [ ] **Performance:** 500 gem update (hareket + pickup check) < 0.5ms
- [ ] Tüm değerler ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| XP gem boyutu/rengi değere göre değişsin mi? (küçük/orta/büyük) | Art Director | Vertical Slice | Provizönel: 3 boyut (small/medium/large) — tier'a göre |
| Manyetik radius upgrade'ı mutasyon mı meta-upgrade mı? | Mutasyon + Meta GDD | İlgili GDD | Provizönel: Her ikisi de — mutasyon (run-içi) + meta (kalıcı) |
| Boss XP'si nasıl dağıtılır? (tek büyük gem mi, çoklu küçük gem mi?) | Game Designer | MVP | Provizönel: Tek büyük gem + etrafında küçük gem yağmuru (görsel tatmin) |
