# Düşman AI System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Kolay Giriş/Derin Derinlik

## Overview

Düşman AI System, tüm düşman türlerinin hareket ve davranış mantığını yöneten sistemdir. Temel davranış oyuncuya doğru chase (kovalama), bunun üstüne tür bazlı varyasyonlar eklenir: kaçan, sıçrayan, ateş eden, patlayan düşmanlar. Her düşman türü basit bir state machine kullanır ama state sayısı minimum tutulur (2-4 state per type). Boss'lar MVP'de büyük + güçlü chase, Vertical Slice'da faz sistemi eklenir. Yüzlerce düşman aynı anda ekranda olacağı için performans birincil kısıttır.

## Player Fantasy

Düşmanlar "durdurulamaz bir sel" gibi hissetmeli — her yönden gelen, sayıları sürekli artan, boğucu ama yenilebilir bir kuvvet. Farklı düşman türleri oyuncuyu farklı şekillerde zorlamalı: biri hızlı gelir, biri uzaktan ateş eder, biri patlar. Oyuncu "bu dalga hep chase yapıyor, kolay" veya "patlayıcılar geldi, dikkatli olmalıyım" diye düşünmeli.

## Detailed Design

### Core Rules

**Temel Hareket (Tüm Düşmanlar):**
1. Tüm düşmanlar `target_position` (= oyuncu pozisyonu) yönünde hareket eder
2. Hareket hızı: `move_speed` (tür bazlı, data-driven)
3. Düşmanlar birbirleriyle çarpışmaz (overlap OK) — yüzlerce düşman için performans kritik
4. Düşmanlar arena sınırlarıyla çarpışır (geçemez)
5. Steering: basit doğrudan chase (A* yok — açık arenada engel minimal)

**Temas Hasarı:**
6. Düşman oyuncuya temas ettiğinde `contact_damage` kadar hasar verir (Entity Health)
7. Temas hasarı `contact_damage_interval` (varsayılan: 0.5 sn) aralıklarla tekrar eder
8. Temas hasarı Entity Health'in invulnerability window'u tarafından engellenir

**Düşman Türleri ve Davranış Varyasyonları:**

| Tür | Hareket | Özel Davranış | MVP? |
|-----|---------|---------------|------|
| **Chaser** | Oyuncuya doğru düz yürür | Yok — en basit düşman | Evet |
| **Runner** | Oyuncuya doğru hızlı koşar | Normal'den 2x hızlı, düşük HP | Evet |
| **Shooter** | Oyuncuya yaklaşır, menzile girince durur | `shoot_range` içinde projectile atar, `shoot_interval` ile | Evet |
| **Exploder** | Oyuncuya doğru koşar | `explode_range` içine girince şişer (1sn) ve patlar (AoE hasar), ölür | Evet |
| **Leaper** | Yavaş yürür, aralıklı sıçrar | `leap_interval` aralıklarla oyuncuya doğru sıçrar (hızlı dash) | VS |
| **Tank** | Yavaş yürür | Yüksek HP, büyük hitbox, knockback direnci | VS |
| **Splitter** | Chaser gibi | Öldüğünde 2-3 küçük düşmana bölünür | Alpha |

**Boss Davranışı (MVP):**
9. Boss = büyük Chaser. Yüksek HP (10x), büyük hitbox, yavaş hareket, yüksek contact damage
10. Boss özel saldırı yok (MVP). Vertical Slice'da faz sistemi ve özel saldırılar eklenir

**Boss Davranışı (Vertical Slice+):**
11. Boss HP eşiklerinde faz geçişi: %75, %50, %25 HP'de davranış değişir
12. Faz değişikliği: hız artışı, yeni saldırı paterni, minion spawn
13. Her boss tipi ScriptableObject ile tanımlanır

**Spawn Koordinasyonu:**
14. Düşman AI spawn'dan sorumlu değil — Dalga/Spawning System düşmanları oluşturur ve data atar
15. AI sadece davranış yönetir

### States and Transitions

**Chaser/Runner/Tank (Basit):**

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Chase** | Spawn | Ölüm | Oyuncuya doğru hareket, temas hasarı |
| **Dead** | HP ≤ 0 | Despawn (pool return) | Ölüm efekti, XP gem drop, collision kapalı |

**Shooter:**

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Chase** | Spawn / Oyuncu menzil dışında | Oyuncu shoot_range'e girdi | Oyuncuya doğru hareket |
| **Shooting** | Oyuncu shoot_range içinde | Oyuncu menzil dışına çıktı / Ölüm | Dur + projectile ateşle |
| **Dead** | HP ≤ 0 | Despawn | Ölüm efekti |

**Exploder:**

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Chase** | Spawn | explode_range'e girdi / Ölüm | Oyuncuya doğru koşu |
| **Priming** | explode_range'e girdi | 1 sn sonra | Dur, şişme animasyonu, kırmızı flash |
| **Explode** | Prime süresi doldu | — | AoE hasar, kendini öldür |
| **Dead** | HP ≤ 0 (patlama veya oyuncu tarafından) | Despawn | Ölüm efekti (patlamadan öldürülürse patlama YOK) |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Player Controller** | Upstream | Oyuncu pozisyonunu okur (targeting) |
| **Entity Health** | İki yönlü | Düşman HP yönetimi (hasar alma + ölüm). Temas → oyuncuya TakeDamage |
| **Dalga/Spawning** | Upstream | Düşman spawn + data atama. AI spawn'dan sorumlu değil |
| **Projectile/Damage** | İki yönlü | Shooter projectile'ları bu system tarafından ateşlenir. Oyuncu projectile'ları düşmana hasar verir |
| **XP & Level-Up** | Downstream (dolaylı) | Düşman ölümünde Entity Health OnDeath → XP gem spawn |

## Formulas

### Enemy Move Speed (Wave-Scaled)
```
effective_speed = base_speed * (1 + wave_number * speed_scale_per_wave)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_speed | float | 1.0-6.0 | Düşman data | Tür bazlı hız |
| wave_number | int | 1-∞ | Dalga Sistemi | Mevcut dalga |
| speed_scale_per_wave | float | 0.02-0.05 | Tuning knob | Dalga başına hız artışı |

**Örnek:** Chaser (base 2.5), Wave 5, scale 0.03 → `2.5 * (1 + 5 * 0.03) = 2.875`
**Karşılaştırma:** Oyuncu base speed 5.0 — düşmanlar her zaman oyuncudan yavaş (en hızlı Runner bile ~4.0 başlangıçta)

### Shooter Fire Rate
```
shoot_interval = base_shoot_interval * (1 - wave_number * 0.02)
// Minimum: 0.5 sn
```

### Exploder AoE
```
explode_damage = base_damage * (1 - (distance / explode_radius) * 0.5)
// Merkezde tam hasar, kenarda %50
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 200+ düşman chase → performans | Basit steering (normalize direction to player), fizik yok. Burst Jobs ile hareket hesabı. | Performans #1 öncelik |
| Düşmanlar üst üste yığılır | Kabul edilebilir — slight separation force opsiyonel (MVP'de yok) | Vampire Survivors'da da üst üste biner |
| Exploder oyuncu tarafından öldürülür | Patlama YOK — normal ölüm. Ödül: riskli ama kazançlı | Stratejik karar: yaklaşmasını bekle vs erken öldür |
| Exploder diğer düşmanlara hasar verir mi? | Hayır — sadece oyuncuya | Friendly fire karmaşıklık ekler, scope dışı |
| Shooter projectile'ı duvardan geçer mi? | Hayır — duvar collision'ında yok olur | Duvar arkası güvenli alan yaratır |
| Boss HP faz eşiğini tek seferde geçer | İlk atlanan faz'a geçer (faz atlamaz) | Her faz deneyimlenmeli |
| Düşman spawn pozisyonu oyuncunun üstünde | Minimum spawn mesafesi Dalga Sistemi tarafından garanti edilir | Adil spawn |
| Wave scaling çok agresif | speed_scale_per_wave max 0.05, sonuçta düşmanlar oyuncudan hızlı olamaz (hard cap: player_speed * 0.9) | Oyuncu her zaman kaçabilmeli |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Player Controller** | Upstream | Hard — oyuncu pozisyonu (hedef) |
| **Entity Health** | İki yönlü | Hard — düşman HP + temas hasarı |
| **Dalga/Spawning** | Upstream | Hard — spawn ve data atama |
| **Projectile/Damage** | İki yönlü | Soft — shooter projectile (yoksa sadece chase türleri) |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| Chaser `base_speed` | 2.5 | 1.5-4.0 | Daha agresif chase | Yavaş, kolay kaçınma |
| Runner `base_speed` | 4.0 | 3.0-5.5 | Çok hızlı, tehlikeli | Normal chase hisseder |
| Shooter `shoot_range` | 6.0 | 4.0-8.0 | Uzaktan ateş, zor | Yakına gelmeli |
| Shooter `shoot_interval` | 2.0 | 1.0-3.0 | Sık ateş | Seyrek ateş |
| Exploder `explode_range` | 1.5 | 1.0-2.5 | Uzaktan patlama | Çok yakın olmalı |
| Exploder `prime_duration` | 1.0 | 0.5-2.0 | Daha çok kaçış zamanı | Az zaman, tehlikeli |
| Exploder `explode_radius` | 2.0 | 1.0-3.0 | Büyük patlama alanı | Küçük alan |
| `speed_scale_per_wave` | 0.03 | 0.02-0.05 | Hız hızlı artar | Yavaş artış |
| Boss `base_speed` | 1.5 | 1.0-2.5 | Daha mobil boss | Yavaş tank |
| `contact_damage_interval` | 0.5 | 0.3-1.0 | Sık temas hasarı | Seyrek temas hasarı |
| `enemy_speed_hard_cap` | 0.9 * player_speed | — | — | — |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Düşman spawn | Fade-in / portal efekti (kısa) | Hafif "pop" | Medium / Low |
| Chaser yürüme | Basit yürüme animasyonu | — | High / — |
| Runner koşma | Hızlı yürüme anim + toz | — | High / — |
| Shooter ateş | Muzzle flash | "shoot" SFX | High / Medium |
| Exploder prime | Şişme animasyonu, kırmızı pulse, uyarı dairesi | Artan "beep beep" SFX | High / High |
| Exploder patlama | Büyük patlama efekti | "BOOM" SFX | High / High |
| Boss giriş | Slow-mo (0.3sn), kamera zoom-out (opsiyonel), boss isim banner | Epik giriş SFX | High / High |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Düşman HP bar | Düşman üzerinde | Hasar alınca | Sadece Elite ve Boss |
| Boss HP bar | Ekran üstü (büyük bar) | Her frame | Boss savaşı sırasında |
| Boss faz göstergesi | Boss HP bar üzerinde işaretler | Faz değişiminde | Boss faz sistemi olduğunda (VS+) |
| Exploder uyarı dairesi | Yerde, exploder etrafında | Prime sırasında | Exploder prime state'inde |

## Acceptance Criteria

- [ ] Chaser düşmanlar oyuncuya doğru hareket eder
- [ ] Runner, Shooter, Exploder türleri kendi davranışlarını sergiler
- [ ] Temas hasarı doğru aralıklarda uygulanır
- [ ] Shooter menzile girince durur ve ateş eder
- [ ] Exploder prime sırasında görsel/ses uyarısı verir
- [ ] Exploder oyuncu tarafından öldürülünce patlamaz
- [ ] Boss MVP'de büyük chaser olarak çalışır
- [ ] Düşman hızları dalga numarasına göre ölçeklenir
- [ ] Düşman hızı hard cap'i (oyuncu hızının %90'ı) aşmaz
- [ ] **Performance:** 200 düşman AI update < 1ms (Burst Jobs ile)
- [ ] Tüm düşman parametreleri ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Separation force (düşmanlar birbirini hafif itmesi) gerekli mi? | Playtest | MVP | Prototipte test — yığılma çok kötüyse ekle |
| Boss faz sistemi detayları | Game Designer | Vertical Slice | MVP'de basit, VS'de tasarlanacak |
| Pathfinding gerekli mi? (arena engelleri varsa) | Level Designer | Alpha | Provizönel: MVP'de engelsiz arena, doğrudan chase yeterli |
| Leaper sıçrama fizik modeli | Gameplay Programmer | VS | Bezier curve veya physics-based arc |
