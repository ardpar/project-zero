# Player Controller

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Kolay Giriş/Derin Derinlik

## Overview

Player Controller, oyuncunun sentetik varlığını arenada kontrol ettiği temel hareket sistemidir. Oyuncu 360° serbest hareket eder ve kısa mesafeli bir dash yeteneğine sahiptir. Saldırılar otomatik olduğu için (Auto-Attack System), oyuncunun tüm beceri ifadesi pozisyonlama ve dash zamanlama üzerinden gerçekleşir. Bu sistem olmadan oyuncunun düşman sürüleri arasında hayatta kalma şansı yoktur — saldırı otomatik ama hayatta kalma tamamen oyuncunun elindedir.

## Player Fantasy

Oyuncu kendini "kaosu yöneten bir varlık" olarak hissetmeli. Ekran düşmanlarla doluyken dar aralıklardan sıyrılmak, tam zamanında dash ile ölümden kaçmak, ve sürüleri arkasından çekerek pozisyon avantajı yaratmak — bunlar "güç fantezisi"nin hareket boyutu. Kontrol anında ve kesin hissettirmeli: oyuncu "yetersiz kontrol yüzünden öldüm" dememeli, "yanlış pozisyon aldım" demeli. Referans his: Vampire Survivors'ın basit ama tatmin edici hareketi + Hades'in responsive dash'i, ama daha düşük hassasiyet beklentisiyle.

## Detailed Rules

### Core Rules

**Hareket:**
1. Oyuncu WASD (klavye) veya sol analog stick (gamepad) ile 360° hareket eder
2. Hareket anlık — ivmelenme/yavaşlama yok. Input bırakıldığında oyuncu anında durur
3. Temel hareket hızı: `base_move_speed` (varsayılan: 5.0 birim/sn)
4. Hareket hızı mutasyonlar tarafından modifiye edilebilir: `effective_speed = base_move_speed * (1 + speed_modifier)`
5. Hareket yönü sprite yönünü belirler (son hareket yönüne bakar)

**Collision:**
6. Oyuncu circle collider kullanır (yarıçap: 0.3 birim)
7. Oyuncu arena sınırlarıyla ve impassable objelerle çarpışır (solid collision)
8. Oyuncu düşmanlarla çarpışır — düşman collision'ı oyuncuya hafif itme (knockback) verir ama geçişi engellemez (overlap OK, temas = hasar)

**Dash:**
9. Dash butonu (Space / sağ bumper) ile aktive edilir
10. Dash, oyuncuyu mevcut hareket yönünde `dash_distance` (varsayılan: 3.0 birim) kadar hızla iter
11. Dash süresi: `dash_duration` (varsayılan: 0.15 sn)
12. Dash sırasında oyuncu hasar alabilir (i-frames YOK)
13. Dash sırasında oyuncu hareket yönünü değiştiremez
14. Dash sonrası `dash_cooldown` (varsayılan: 3.0 sn) bekleme süresi
15. Hareket input'u yokken dash basılırsa, oyuncu baktığı yöne dash atar
16. Dash arena sınırlarında durur (sınırı geçemez)
17. Dash düşmanların içinden geçer (collision ignore during dash)

**Input:**
18. Unity Input System kullanılır (legacy Input değil)
19. Klavye+mouse ve gamepad aynı anda desteklenir
20. Hareket input'u normalize edilir (çapraz hareket daha hızlı olmaz)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Idle** | Hareket input'u yok | Hareket input'u verilir VEYA dash basılır | Oyuncu durur, otomatik saldırılar devam eder |
| **Moving** | Hareket input'u var | Input bırakılır VEYA dash basılır VEYA ölüm | `effective_speed` ile hareket yönünde ilerler |
| **Dashing** | Dash butonu basılır VE cooldown hazır | `dash_duration` süresi dolar VEYA arena sınırına çarpar | Sabit yönde hızlı ilerleme, düşman collision ignore |
| **Dash Cooldown** | Dash biter | `dash_cooldown` süresi dolar | Normal hareket (Idle/Moving), dash butonu devre dışı |
| **Dead** | HP ≤ 0 | Run Manager tarafından reset | Tüm input devre dışı, ölüm animasyonu oynar |
| **Paused** | Pause menü / Level-up seçim ekranı açık | Menü kapanır | Tüm hareket ve input devre dışı, fizik durur |

**Not:** Dash Cooldown, Idle/Moving ile paralel çalışan bir alt-state'tir. Oyuncu cooldown sırasında normal hareket edebilir, sadece tekrar dash atamaz.

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Auto-Attack System** | Bu sisteme bağımlı | Player Controller pozisyon ve yön verisini sağlar. Auto-Attack bu verilere göre saldırı yönü/kaynağı belirler. Player Controller saldırıları bilmez. |
| **Entity Health System** | Bu sisteme bağımlı | HP ≤ 0 olduğunda Player Controller'a `OnDeath` event'i gönderir → Dead state'e geçiş. Düşman temas hasarı Entity Health üzerinden işlenir, Player Controller sadece knockback uygular. |
| **Camera System** | Bu sisteme bağımlı | Camera, Player Controller'ın transform pozisyonunu takip eder. Player Controller kameraya bilgi vermez — camera tek yönlü okur. |
| **Düşman AI** | Bu sisteme bağımlı | Düşmanlar Player Controller'ın pozisyonunu hedef olarak kullanır. Player Controller düşmanları bilmez. |
| **Mutasyon Sistemi** | Bu sisteme bağımlı | Mutasyonlar `speed_modifier` ve `dash_cooldown_modifier` değerlerini Player Controller'a sağlar. Player Controller bu modifier'ları okur ama mutasyon sistemini doğrudan çağırmaz. |
| **Gameplay HUD** | Bu sisteme bağımlı | HUD, dash cooldown durumunu (kalan süre / hazır) Player Controller'dan okur. |
| **VFX / Juice** | Bu sisteme bağımlı | Player Controller dash başlangıç ve bitiş event'leri yayınlar → VFX sistemi afterimage/trail efekti oynar. |

**Arayüz prensibi:** Player Controller bir "sağlayıcı" — pozisyon, yön, state bilgisini yayınlar. Diğer sistemler bu verileri okur. Player Controller kendisine bağımlı sistemleri doğrudan çağırmaz (loose coupling).

## Formulas

### Effective Move Speed
```
effective_speed = base_move_speed * (1 + speed_modifier)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_move_speed | float | 3.0-8.0 | Tuning knob | Temel hareket hızı (birim/sn) |
| speed_modifier | float | -0.5 to 2.0 | Mutasyon Sistemi | Tüm aktif mutasyonların hız modifier toplamı |

**Expected output range:** 2.5 - 15.0 birim/sn
**Edge case:** `speed_modifier < -0.5` ise -0.5'e clamp edilir (minimum hız korunur)

### Dash Speed
```
dash_speed = dash_distance / dash_duration
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| dash_distance | float | 2.0-5.0 | Tuning knob | Dash mesafesi (birim) |
| dash_duration | float | 0.1-0.25 | Tuning knob | Dash süresi (saniye) |

**Expected output range:** 12.0 - 20.0 birim/sn (normal hareketin ~3-4 katı)

### Effective Dash Cooldown
```
effective_dash_cd = base_dash_cooldown * (1 - dash_cd_modifier)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_dash_cooldown | float | 2.0-5.0 | Tuning knob | Temel dash bekleme süresi (sn) |
| dash_cd_modifier | float | 0.0 to 0.6 | Mutasyon Sistemi | Cooldown azaltma yüzdesi |

**Expected output range:** 1.2 - 5.0 sn
**Edge case:** `effective_dash_cd < 0.5` ise 0.5'e clamp edilir (dash spam engellenir)

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Oyuncu köşeye sıkışır (2 duvar arasında) | Düşman knockback'i küçültülür, oyuncu yavaşça itilir. Ölüm garantili değil. | Frustration önleme — köşeye sıkışma = anında ölüm olmamalı |
| Dash sırasında arena sınırına ulaşılır | Dash erken kesilir, oyuncu sınırda durur. Cooldown normal başlar. | Dash mesafesinden bağımsız güvenli davranış |
| Dash sırasında pause/level-up açılır | Dash iptal edilir, oyuncu mevcut pozisyonda durur. Dash cooldown'u başlamaz. | Menü açılması oyuncu aleyhine olmamalı |
| speed_modifier negatife gider (< -0.5) | -0.5'e clamp. Oyuncu hiçbir zaman duramaz veya geriye gidemez. | Minimum hareket hızı garantisi |
| Çok yüksek speed_modifier (> 2.0) | 2.0'a clamp. Maksimum hız = 3x temel hız. | Kontrol edilemeyecek hızlar oynanabilirliği bozar |
| Oyuncu hareket etmezken dash | Son baktığı yöne dash atar | Dash her zaman çalışmalı — "çalışmayan buton" hissi kötü |
| Aynı frame'de dash + ölüm | Ölüm öncelikli — dash iptal edilir, Dead state'e geçilir | Ölüm geciktirilemez |
| Düşman ve duvar arasında sıkışma | Düşman knockback'i azaltılır, oyuncu duvardan geçemez ama düşman üzerinden geçebilir | Soft-body collision: düşmanlar arası geçiş serbest |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Auto-Attack System** | Downstream (bağımlı) | Hard — pozisyon ve yön verisini okur |
| **Entity Health System** | Downstream (bağımlı) | Hard — OnDeath event'i ile Dead state tetiklenir |
| **Camera System** | Downstream (bağımlı) | Hard — transform pozisyonunu takip eder |
| **Düşman AI** | Downstream (bağımlı) | Hard — oyuncu pozisyonunu hedef olarak kullanır |
| **Mutasyon Sistemi** | Upstream (sağlayıcı) | Soft — speed_modifier ve dash_cd_modifier sağlar. Mutasyon yoksa varsayılan değerler kullanılır |
| **Gameplay HUD** | Downstream (bağımlı) | Soft — dash cooldown göstergesi okur |
| **VFX / Juice** | Downstream (bağımlı) | Soft — dash event'leri dinler |
| **Sprite Compositing** | Downstream (bağımlı) | Soft — sprite yönü ve animasyon state'i okur |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `base_move_speed` | 5.0 | 3.0-8.0 | Daha hızlı hareket, kaçış kolaylaşır | Yavaş hareket, pozisyonlama zorlaşır |
| `dash_distance` | 3.0 | 2.0-5.0 | Daha uzun dash, güvenli kaçış | Kısa dash, riskli kararlar |
| `dash_duration` | 0.15 | 0.1-0.25 | Daha yavaş ama uzun dash animasyonu | Daha hızlı snap dash |
| `base_dash_cooldown` | 3.0 | 2.0-5.0 | Daha az dash kullanımı, hareket odaklı | Sık dash, daha agresif oyun |
| `player_collider_radius` | 0.3 | 0.2-0.5 | Daha büyük hitbox, hasar almak kolaylaşır | Küçük hitbox, daha affedici |
| `speed_modifier_min_clamp` | -0.5 | -0.7 to -0.3 | Yavaşlama etkisi artırılır | Minimum hız yükselir |
| `speed_modifier_max_clamp` | 2.0 | 1.5-3.0 | Maksimum hız artar (kontrol zorlaşabilir) | Hız tavanı düşer |
| `dash_cd_min_clamp` | 0.5 | 0.3-1.0 | Minimum cooldown yükselir | Dash spam'ına yaklaşır |
| `enemy_knockback_force` | 0.5 | 0.2-1.0 | Düşman teması daha fazla iter | Hafif itme, daha sticky |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Hareket | Yürüme animasyonu (4/8 yön), toz partikülleri | Hafif ayak sesi (opsiyonel) | High / Low |
| Dash başlangıç | Afterimage trail, hız çizgileri | "whoosh" SFX | High / High |
| Dash bitiş | Trail solması | — | Medium / — |
| Dash hazır (cooldown bitti) | HUD göstergesi parlar | Kısa "ding" SFX | Medium / Low |
| Düşman teması (knockback) | Kısa flash (beyaz), hafif screen shake | "hit" SFX | High / High |
| Ölüm | Patlama/dağılma animasyonu, screen freeze (0.1sn) | Ölüm SFX | High / High |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Dash cooldown indicator | HUD — karakter altında veya ekran köşesinde | Her frame (cooldown bar) | Dash cooldown aktifken |
| Dash hazır göstergesi | Aynı yer | Cooldown bittiğinde | Dash kullanılabilir |
| Hareket hızı buff/debuff | HUD köşesinde ikon | Modifier değiştiğinde | speed_modifier ≠ 0 |

## Acceptance Criteria

- [ ] Oyuncu WASD ve gamepad ile 360° hareket edebilir
- [ ] Çapraz hareket normal hareketle aynı hızda (normalize)
- [ ] Dash butonu oyuncuyu hareket yönünde 3.0 birim ilerletir
- [ ] Dash cooldown sırasında tekrar dash atılamaz
- [ ] Dash düşmanların içinden geçer (collision ignore)
- [ ] Dash arena sınırında durur (geçmez)
- [ ] speed_modifier -0.5 ile 2.0 arasında clamp edilir
- [ ] Ölüm anında tüm input devre dışı kalır
- [ ] Pause sırasında hareket ve fizik durur
- [ ] **Performance:** Player Controller update'i < 0.1ms (60fps budget'ından < %1)
- [ ] Tüm hareket ve dash parametreleri ScriptableObject/data file üzerinden konfigüre edilebilir (hardcoded değil)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Mutasyon bazlı dash varyasyonları (teleport dash, damage dash) nasıl handle edilir? | Mutasyon Sistemi GDD | Mutasyon GDD tasarımında | Provizönel: Player Controller'a dash_type enum eklenir, mutasyonlar set eder |
| Arena sınırları nasıl tanımlanır? (Collider? Tilemap bounds?) | Level/Arena GDD | Alpha | Provizönel: BoxCollider2D kenarlık |
