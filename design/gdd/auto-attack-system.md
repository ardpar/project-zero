# Auto-Attack System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Kolay Giriş/Derin Derinlik

## Overview

Auto-Attack System, oyuncunun saldırılarını otomatik olarak tetikleyen ve yönlendiren sistemdir. Oyuncu saldırı butonu basmaz — hareket ederken saldırılar belirli aralıklarla otomatik ateşlenir. Targeting, oyuncunun hareket yönündeki en yakın düşmana yöneliktir. Başlangıçta basit bir projectile ateşler; mutasyonlar eklendikçe saldırı türleri, sayıları ve davranışları tamamen değişir. Bu sistem Mutasyon Sistemi'nin birincil çıkış noktasıdır — mutasyonlar buradaki parametreleri modifiye eder.

## Player Fantasy

Oyuncunun "silah" olarak hissettiği şey kendi vücudu. Saldırılar otomatik olduğu için oyuncu "savaşıyorum" yerine "yönlendiriyorum" hissetmeli. Doğru pozisyonda durduğunda düşman sürülerini biçtiğini görmeli — kaos güzel ve kontrollü. Her mutasyon eklendikçe saldırılar görsel ve mekanik olarak büyümeli — "güçlenme spirali" hissi.

## Detailed Rules

### Core Rules

**Otomatik Ateş:**
1. Oyuncu hareket ederken veya dururken saldırılar otomatik tetiklenir
2. Saldırı aralığı: `attack_interval` (varsayılan: 1.0 sn)
3. Mutasyonlarla modifiye edilir: `effective_interval = base_interval * (1 - attack_speed_modifier)`
4. Minimum attack interval: 0.1 sn (saldırı spam'ı sınırı)

**Targeting:**
5. Targeting, oyuncunun hareket yönünden ±`targeting_cone_angle` (varsayılan: 60°) koni içindeki en yakın düşmanı seçer
6. Oyuncu dururken: son hareket yönündeki koni kullanılır
7. Koni içinde düşman yoksa: 360° tarama yapılarak en yakın düşman seçilir (fallback)
8. Hedef menzili: `attack_range` (varsayılan: 8.0 birim)
9. Menzilde düşman yoksa: saldırı tetiklenmez, cooldown ilerlemez (ammo israfı yok)

**Başlangıç Saldırısı (Mutasyon Öncesi):**
10. Varsayılan saldırı: tek projectile, hedef düşmana doğru ateşlenir
11. Projectile hızı, hasarı ve menzili Projectile/Damage System tarafından yönetilir
12. Bu sistemin çıktısı: `FireProjectile(direction, attackData)` çağrısı

**Mutasyon Entegrasyonu:**
13. Her mutasyon `AttackModifier` sağlayabilir: ek projectile, AoE, pierce, chain, vb.
14. Birden fazla saldırı türü aynı anda aktif olabilir (örn: öne projectile + etrafta AoE)
15. Her aktif saldırı türünün kendi `attack_interval`'ı olabilir
16. Auto-Attack System mutasyonları bilmez — sadece `AttackModifier` listesini okur ve uygular

**Saldırı Slotları:**
17. Başlangıçta 1 saldırı slotu (varsayılan projectile)
18. Mutasyonlar yeni saldırı slotları ekleyebilir (kol mutasyonu = yeni saldırı türü)
19. Maksimum `max_attack_slots` (varsayılan: 6) eşzamanlı saldırı türü
20. Her slot bağımsız cooldown'a sahip

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Active** | Run başlangıcı | Run sonu / Ölüm / Pause | Saldırılar cooldown'a göre otomatik tetiklenir |
| **Firing** | Cooldown doldu VE hedef menzilde | Projectile ateşlendi | Ateş animasyonu/efekti, FireProjectile çağrısı |
| **No Target** | Menzilde düşman yok | Düşman menzile girdi | Saldırı tetiklenmez, cooldown ilerlemez |
| **Paused** | Pause / Level-up ekranı | Menü kapanır | Tüm cooldown'lar dondurulur |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Player Controller** | Upstream | Pozisyon (ateş kaynağı) ve hareket yönü (targeting cone yönü) okur |
| **Entity Health** | Upstream | Düşman Entity Health'e dolaylı hasar — Projectile/Damage aracılığıyla |
| **Projectile/Damage** | Downstream | `FireProjectile(origin, direction, attackData)` çağrısı yapar. Projectile oluşturma ve hasar hesabı orada |
| **Mutasyon Sistemi** | Upstream sağlayıcı | `AttackModifier` listesi sağlar: attack_speed_modifier, ek saldırı slotları, projectile türü değişiklikleri |
| **Düşman AI** | Upstream veri | Düşman pozisyonları targeting için okunur (spatial query) |
| **VFX / Juice** | Downstream | Ateş etme event'i → muzzle flash, ateş efekti |
| **Gameplay HUD** | Downstream | Aktif saldırı slotlarını ve cooldown'larını gösterir (opsiyonel) |

## Formulas

### Effective Attack Interval
```
effective_interval = max(base_interval * (1 - attack_speed_modifier), min_attack_interval)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_interval | float | 0.5-2.0 | Saldırı slot data | Temel saldırı aralığı (sn) |
| attack_speed_modifier | float | 0.0-0.9 | Mutasyon Sistemi | Hız artış yüzdesi |
| min_attack_interval | float | 0.1 | Tuning knob (sabit) | Minimum aralık (spam sınırı) |

**Expected range:** 0.1 - 2.0 sn
**Örnek:** base 1.0, modifier 0.3 → `max(1.0 * 0.7, 0.1) = 0.7 sn`

### Targeting Score
```
score = distance_to_enemy / (1 + cone_alignment_bonus)
// En düşük score = en iyi hedef
cone_alignment_bonus = dot(move_direction, enemy_direction) > cos(cone_angle) ? 1.0 : 0.0
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| distance_to_enemy | float | 0-attack_range | Hesaplama | Düşman mesafesi |
| cone_alignment_bonus | float | 0 veya 1 | Hesaplama | Koni içindeyse bonus (mesafe yarıya düşer) |
| cone_angle | float | 30-90° | Tuning knob | Targeting koni açısı (yarım açı) |

**Mantık:** Koni içindeki düşmanlar mesafe açısından 2x avantajlı. Koni dışı sadece fallback.

### DPS Estimate (per slot)
```
dps_per_slot = projectile_damage / effective_interval
total_dps = sum(dps_per_slot for each active slot)
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Menzilde düşman yok | Saldırı tetiklenmez, cooldown ilerlemez | Boşa ateş = görsel gürültü |
| Birden fazla düşman eşit mesafede | Rastgele biri seçilir (deterministic seed ile frame'e göre) | Tutarlı ama öngörülemez — herbirine sırayla basmaz |
| Oyuncu dururken targeting | Son hareket yönündeki koni kullanılır | Durmak = savunmasız değil |
| 6 slot dolu + yeni mutasyon saldırı ekliyor | Yeni saldırı eklenemez; mutasyon seçim UI'da uyarı gösterilir | Slot limiti = stratejik karar |
| Dash sırasında saldırı | Saldırılar devam eder (dash hareketi targeting'i günceller) | Dash = kaçış, ama saldırı devam etmeli |
| Pause sırasında cooldown | Dondurulur — pause bittikten sonra kaldığı yerden devam | Pause adil olmalı |
| attack_speed_modifier > 0.9 | 0.9'a clamp, minimum interval 0.1sn | Sonsuz saldırı hızı = performans riski |
| Hedef düşman ölür (projectile havadayken) | Projectile hedefsiz devam eder (doğrusal) veya kaybolur — Projectile System kararı | Auto-Attack sadece ateşler, projectile yönetimi başka sistemde |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Player Controller** | Upstream | Hard — pozisyon ve yön (ateş kaynağı ve targeting cone) |
| **Entity Health** | Upstream (dolaylı) | Hard — düşman pozisyonları + hasar verme (Projectile aracılığıyla) |
| **Projectile/Damage** | Downstream | Hard — FireProjectile çağrısı ile projectile oluşturur |
| **Mutasyon Sistemi** | Upstream sağlayıcı | Soft — AttackModifier listesi (yoksa varsayılan tek projectile) |
| **Düşman AI** | Upstream veri | Soft — düşman pozisyon listesi (spatial query) |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `base_attack_interval` | 1.0 | 0.5-2.0 | Yavaş saldırı, güçlü hisli | Hızlı saldırı, makinalı tüfek hissi |
| `min_attack_interval` | 0.1 | 0.05-0.2 | Saldırı hızı tavanı yükselir | Çok hızlı saldırıya izin verir |
| `attack_range` | 8.0 | 4.0-12.0 | Uzak menzil, güvenli saldırı | Yakın menzil, riskli |
| `targeting_cone_angle` | 60 | 30-90 | Geniş koni, kolay hedefleme | Dar koni, pozisyonlama önemli |
| `max_attack_slots` | 6 | 4-8 | Daha fazla eşzamanlı saldırı | Daha az, stratejik slot yönetimi |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Projectile ateşleme | Muzzle flash (küçük), projectile sprite spawn | "pew" SFX (hafif, tekrarlayan) | High / Medium |
| Yeni saldırı slotu eklendi | Slot UI animasyonu, kısa parıltı | "upgrade" pling | High / High |
| Saldırı hızı artışı | Mevcut saldırılar daha hızlı görsel olarak fark edilir | Pitch shift (SFX biraz yükselir) | Low / Low |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Aktif saldırı slotları | HUD kenarında küçük ikonlar | Slot değiştiğinde | Her zaman (en az 1 slot) |
| Slot cooldown (opsiyonel) | İkon üzerinde dolum animasyonu | Her frame | Geliştirme aşamasında değerlendir |

## Acceptance Criteria

- [ ] Saldırılar otomatik tetiklenir (oyuncu butonu basmaz)
- [ ] Targeting koni içindeki en yakın düşmanı seçer
- [ ] Koni dışında düşman yoksa 360° fallback çalışır
- [ ] Menzilde düşman yoksa saldırı tetiklenmez
- [ ] attack_speed_modifier saldırı aralığını doğru azaltır
- [ ] Minimum attack interval 0.1sn'nin altına düşmez
- [ ] Birden fazla saldırı slotu bağımsız çalışır
- [ ] Pause sırasında cooldown'lar dondurulur
- [ ] **Performance:** 200 düşmanla targeting hesabı < 0.5ms (spatial hash/quadtree kullan)
- [ ] Tüm parametreler ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Spatial query için hangi yapı? (QuadTree vs spatial hash vs Physics2D.OverlapCircle) | Gameplay Programmer | MVP prototype | Prototipte benchmark — muhtemelen spatial hash |
| Mutasyon bazlı targeting override (AoE mutasyonu targeting'i değiştirir mi?) | Mutasyon Sistemi GDD | Mutasyon tasarımında | Provizönel: AttackModifier targeting_type enum içerebilir |
| Saldırı slotları UI'da mı gösterilsin yoksa implicit mi? | UX Designer | Vertical Slice | Prototipte test — başlangıçta implicit |
