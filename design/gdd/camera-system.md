# Camera System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Kolay Giriş/Derin Derinlik

## Overview

Camera System, oyuncuyu yumuşak takip eden ve hareket yönünde hafif "look-ahead" uygulayan 2D top-down kamera yöneticisidir. Screen shake, zoom ve sınır sınırlama (confine) özelliklerini de yönetir. Oyuncunun "ne olduğunu" ve "neyin geldiğini" her zaman görmesini sağlar. Kamera doğrudan kontrol edilmez — tamamen otomatik ve tepkiseldir.

## Player Fantasy

Oyuncu kamerayı fark etmemeli — sadece her şeyi rahatça görmeli. "Düşmanlar sağdan geliyor" diye hareket yönüne baktığında ilerisini görebilmeli. Büyük hasar anlarında screen shake "etki hissi" vermeli. Kamera hiçbir zaman oyuncuyu kaybetmemeli veya desorientasyon yaratmamalı.

## Detailed Rules

### Core Rules

**Takip:**
1. Kamera, oyuncunun pozisyonunu yumuşak interpolasyon (SmoothDamp) ile takip eder
2. Takip hızı: `follow_smoothing` (varsayılan: 0.1 sn — neredeyse anında ama hafif yumuşatma)
3. Kamera hiçbir zaman oyuncudan `max_offset` (varsayılan: 2.0 birim) daha fazla uzaklaşamaz

**Look-Ahead:**
4. Kamera, oyuncunun hareket yönünde `look_ahead_distance` (varsayılan: 1.5 birim) kadar öne bakar
5. Look-ahead geçişi `look_ahead_smoothing` (varsayılan: 0.3 sn) ile yumuşatılır
6. Oyuncu durduğunda look-ahead yavaşça sıfıra döner
7. Dash sırasında look-ahead geçici olarak artırılır (dash yönünde `dash_look_ahead_multiplier` = 1.5x)

**Screen Shake:**
8. Diğer sistemler `TriggerShake(float intensity, float duration)` çağrısı ile screen shake tetikler
9. Shake, rastgele offset olarak uygulanır (Perlin noise tabanlı, kaba değil)
10. Birden fazla shake aynı anda tetiklenirse, en güçlü olanı baskın olur (stack etmez)
11. Shake sırasında kamera takip pozisyonundan sapmaz (pozisyon + shake offset)

**Zoom:**
12. Varsayılan zoom: `base_orthographic_size` (varsayılan: 8.0 birim — 16 birim genişlik)
13. Zoom değişimi şu an statik — dinamik zoom MVP'de yok (Full Vision'da eklenebilir)

**Arena Sınırlama (Confine):**
14. Kamera, arena sınırlarının dışını göstermez
15. Oyuncu kenara yaklaştığında kamera durur, oyuncu ekranın kenarına kayar
16. Confine bounds, Collider2D veya manual Rect ile tanımlanır

**Input:**
17. Kamera doğrudan oyuncu tarafından kontrol edilmez (pan/rotate yok)
18. Kamera her zaman ortographic (perspective yok — 2D oyun)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Following** | Run başlangıcı | Run sonu / Pause | Yumuşak takip + look-ahead aktif |
| **Shaking** | TriggerShake çağrısı | Shake süresi dolar | Following + shake offset eklenir |
| **Paused** | Pause menü / Level-up ekranı | Menü kapanır | Kamera pozisyonu sabit kalır |
| **Transitioning** | Arena/biome geçişi (varsa) | Hedef pozisyona ulaşıldı | Yumuşak geçiş animasyonu (pan) |

**Not:** Shaking, Following'in alt-state'i — takip devam eder, üzerine shake eklenir.

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Player Controller** | Upstream | Kamera, oyuncu transform pozisyonunu ve hareket yönünü okur |
| **Entity Health** | Upstream trigger | Oyuncu hasar alınca screen shake tetikler (intensity hasar oranında) |
| **VFX / Juice** | Upstream trigger | Patlama, boss ölüm gibi olaylarda screen shake tetikler |
| **Dalga/Spawning** | Upstream trigger | Dalga geçişlerinde kısa shake (opsiyonel) |
| **Arena sınırları** | Upstream | Confine bounds'u arena tanımından okur |

## Formulas

### Camera Target Position
```
look_ahead_offset = move_direction * look_ahead_distance * look_ahead_multiplier
target_position = player_position + look_ahead_offset
camera_position = SmoothDamp(current_position, target_position, follow_smoothing)
camera_position = Clamp(camera_position, confine_bounds)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| player_position | Vector2 | arena bounds | Player Controller | Oyuncu pozisyonu |
| move_direction | Vector2 | normalized | Player Controller | Hareket yönü (birim vektör) |
| look_ahead_distance | float | 0.5-3.0 | Tuning knob | Look-ahead mesafesi |
| look_ahead_multiplier | float | 1.0-1.5 | State bağımlı | Dash sırasında 1.5, normal 1.0 |
| follow_smoothing | float | 0.05-0.3 | Tuning knob | Takip yumuşatma süresi (saniye) |

### Screen Shake Offset
```
shake_offset = PerlinNoise(time * shake_frequency) * shake_intensity * shake_decay
shake_decay = 1 - (elapsed_time / shake_duration)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| shake_intensity | float | 0.05-0.5 | Trigger kaynağı | Shake şiddeti (birim) |
| shake_duration | float | 0.1-0.5 | Trigger kaynağı | Shake süresi (saniye) |
| shake_frequency | float | 15-30 | Tuning knob | Shake titreşim hızı |

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Oyuncu arena köşesinde | Kamera confine'a takılır, oyuncu ekranın köşesine kayar | Kamera asla arena dışını göstermez |
| Ani yön değişimi (180°) | Look-ahead yumuşak geçiş yapar (0.3sn), ani sıçrama olmaz | Yumuşak kamera = profesyonel his |
| Dash + screen shake aynı anda | Her ikisi de uygulanır (look-ahead artırılır + shake offset) | Bağımsız sistemler, çakışmaz |
| Çok güçlü shake (intensity > 0.5) | 0.5'e clamp | Aşırı shake desorientasyon yaratır |
| Oyuncu ölür | Kamera son pozisyonda kalır (takip durur), death animation'ı gösterir | Ölüm anı net görünmeli |
| Follow smoothing çok yüksek (0.3+) | Kamera geride kalır, düşmanları görmek zorlaşır | 0.3 max, oyuncudan kopmaması için |
| Pause sırasında shake devam eder mi? | Hayır — pause tüm kamera hareketini dondurur | Menüde shake frustration yaratır |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Player Controller** | Upstream | Hard — pozisyon ve yön verisi |
| **Entity Health** | Upstream trigger | Soft — hasar shake'i (yoksa shake çalışmaz, sorun değil) |
| **VFX / Juice** | Upstream trigger | Soft — efekt shake'leri |
| **Arena sınırları** | Upstream | Hard — confine bounds |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `follow_smoothing` | 0.1 | 0.05-0.3 | Daha yavaş takip (cinematic) | Daha sert takip (responsive) |
| `look_ahead_distance` | 1.5 | 0.5-3.0 | İlerisini daha çok görür | Oyuncu daha ortalarda |
| `look_ahead_smoothing` | 0.3 | 0.1-0.5 | Yavaş look-ahead geçişi | Hızlı yön değişimi |
| `dash_look_ahead_multiplier` | 1.5 | 1.0-2.0 | Dash sırasında daha çok ilerisi | Normal look-ahead |
| `base_orthographic_size` | 8.0 | 5.0-12.0 | Daha geniş görüş (zoom out) | Daha yakın (zoom in) |
| `max_shake_intensity` | 0.5 | 0.2-0.8 | Güçlü shake efekti | Hafif shake |
| `shake_frequency` | 20.0 | 15-30 | Hızlı titreşim | Yavaş sallanma |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Screen shake (hasar) | Kamera pozisyon offset | — (hasar SFX zaten Entity Health'te) | High / — |
| Screen shake (patlama) | Kamera pozisyon offset | — (patlama SFX zaten VFX'te) | High / — |
| Look-ahead geçişi | Yumuşak kamera kaydırma | — | Medium / — |

**Not:** Camera System'in kendi ses efekti yoktur. Tüm feedback görseldir (pozisyon offset).

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| — | — | — | Camera System'in UI gösterimi yok. Kamera parametreleri debug menüsünde ayarlanabilir (development only). |

## Acceptance Criteria

- [ ] Kamera oyuncuyu yumuşak takip eder (SmoothDamp)
- [ ] Look-ahead hareket yönünde uygulanır, durduğunda sıfıra döner
- [ ] Kamera arena sınırlarının dışını göstermez (confine)
- [ ] Screen shake tetiklenebilir ve Perlin noise tabanlı doğal hisseder
- [ ] Birden fazla shake stack etmez — en güçlü baskın olur
- [ ] Pause sırasında kamera hareketi durur
- [ ] Dash sırasında look-ahead artırılır
- [ ] Oyuncu ölünce kamera son pozisyonda kalır
- [ ] **Performance:** Camera update < 0.02ms (negligible)
- [ ] Tüm kamera parametreleri ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Dinamik zoom (düşman yoğunluğuna göre) eklenecek mi? | Creative Director | Full Vision | Provizönel: MVP'de yok, Full Vision'da değerlendirilir |
| Boss fight'ta özel kamera davranışı olacak mı? (wider zoom, cinematic) | Boss GDD | Alpha | Provizönel: Boss fight'ta orthographic size hafif artırılır |
