# Entity Health System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Kolay Giriş/Derin Derinlik

## Overview

Entity Health System, hem oyuncu hem düşmanlar için paylaşılan bir can (HP) ve zırh (Armor) yönetim sistemidir. Her entity'nin HP ve opsiyonel Armor değeri vardır. Hasar önce Armor'dan düşürülür, kalan HP'den kesilir. HP sıfıra düştüğünde entity ölür. Oyuncu HP'si düşman drop'ları (HP orb) ile iyileşebilir, pasif rejenerasyon yoktur. Düşman HP'leri tür, tier (Normal/Elite/Boss) ve dalga numarasına göre ölçeklenir.

## Player Fantasy

Oyuncu her hasar aldığında tehlikeyi hissetmeli — HP değerli ve kıt bir kaynak. Zırh "tampon" hissi vermeli: "Zırhım var, biraz daha agresif oynayabilirim." HP orb'u toplamak rahatlama anı yaratmalı. Ölüm ani ve net olmalı — uzun "can çekişme" yok, "HP bitti = run bitti."

## Detailed Design

### Core Rules

**HP (Hit Points):**
1. Her entity bir `max_hp` ve `current_hp` değerine sahiptir
2. `current_hp` her zaman 0 ile `max_hp` arasında clamp edilir
3. `current_hp ≤ 0` → entity ölür (`OnDeath` event yayınlanır)
4. Oyuncu başlangıç `max_hp`: 100 (tuning knob)

**Armor:**
5. Armor, flat hasar azaltma sağlar: `actual_damage = max(raw_damage - armor, 1)`
6. Hasar asla 1'in altına düşmez (minimum 1 hasar garantisi)
7. Oyuncu başlangıç armor: 0 (mutasyonlarla kazanılır)
8. Düşmanların armor'ı yok (sadece oyuncuda, basitlik için)

**Hasar Alma:**
9. Hasar kaynakları: düşman temas hasarı, düşman saldırıları, arena tehlikeleri
10. Hasar aldıktan sonra kısa `invulnerability_window` (varsayılan: 0.5 sn) — temas hasarı spam'ı engellenir
11. Invulnerability tüm hasar kaynaklarından korur (prototipte test edilecek, gerekirse sadece aynı kaynak türü)

**İyileşme:**
12. Düşmanlar öldüğünde `hp_drop_chance` (varsayılan: %5) ile HP orb düşürür
13. HP orb toplandığında `hp_orb_value` (varsayılan: 10 HP) kadar iyileşme
14. İyileşme `max_hp`'yi aşamaz
15. Pasif HP rejenerasyonu YOK (pickup-only)

**Düşman HP Ölçekleme:**
16. Her düşman türünün `base_hp` değeri var (data-driven)
17. Düşman tier çarpanı: Normal = 1x, Elite = 3x, Boss = 10x
18. Dalga ölçekleme: `enemy_hp = base_hp * tier_multiplier * (1 + wave_number * wave_hp_scale)`
19. `wave_hp_scale` varsayılan: 0.15 (her dalga %15 daha fazla HP)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Alive** | Entity spawn | HP ≤ 0 | Normal — hasar alabilir, hareket edebilir |
| **Invulnerable** | Hasar alındı | `invulnerability_window` süresi dolar | Hasar almaz, görsel flash/flicker |
| **Dead** | HP ≤ 0 | Entity despawn / run reset | Ölüm event yayınlanır, entity etkileşilemez |

**Not:** Invulnerable, Alive'ın alt-state'i. Entity hala hareket edebilir. Sadece oyuncu invulnerability kullanır — düşmanlar her zaman hasar alır.

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Player Controller** | Downstream | OnDeath event → Player Controller Dead state |
| **Auto-Attack System** | Upstream hasar kaynağı | Saldırı isabeti → `TakeDamage(amount, source)` çağrısı |
| **Projectile/Damage** | Upstream hasar kaynağı | Mermi isabeti → `TakeDamage(amount, source)` çağrısı |
| **XP & Level-Up** | Downstream | Düşman OnDeath → XP gem spawn tetikler |
| **Düşman AI** | İki yönlü | Düşman temas → oyuncuya `TakeDamage`; düşman HP data'sı AI'dan bağımsız |
| **Mutasyon Sistemi** | Upstream sağlayıcı | Mutasyonlar `max_hp_modifier`, `armor` değerlerini sağlar |
| **Gameplay HUD** | Downstream | Oyuncu HP/Armor bar'ı; düşman HP bar'ı (elite/boss) |
| **VFX / Juice** | Downstream | Hasar flash, ölüm efekti, HP orb toplama efekti |
| **Dalga/Spawning** | Upstream sağlayıcı | `wave_number` değerini sağlar (HP ölçekleme için) |

**Arayüz prensibi:** `TakeDamage(int rawDamage, DamageSource source)` tek giriş noktası. Tüm hasar bu method üzerinden geçer. Entity Health iç hesaplamasını yapar (armor, clamp, invulnerability), sonucu event olarak yayınlar.

## Formulas

### Actual Damage (Oyuncu)
```
actual_damage = max(raw_damage - armor, 1)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| raw_damage | int | 1-999 | Hasar kaynağı | İşlenmemiş hasar |
| armor | int | 0-50 | Mutasyon Sistemi | Toplam zırh değeri |

**Expected output range:** 1 - 999
**Edge case:** Armor > raw_damage → minimum 1 hasar. Armor asla tam bağışıklık vermez.

### Effective Max HP (Oyuncu)
```
effective_max_hp = base_max_hp * (1 + hp_modifier)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_max_hp | int | 50-200 | Tuning knob | Temel max HP |
| hp_modifier | float | -0.3 to 2.0 | Mutasyon Sistemi | HP çarpanı |

**Expected output range:** 35 - 300 HP (sonuç integer'a yuvarlanır)
**Edge case:** `hp_modifier < -0.3` → clamp to -0.3. Max HP hiçbir zaman 35'in altına düşmez.

### Enemy HP Scaling
```
enemy_hp = base_hp * tier_multiplier * (1 + wave_number * wave_hp_scale)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_hp | int | 5-500 | Düşman data file | Tür bazlı HP |
| tier_multiplier | float | 1 / 3 / 10 | Düşman tier | Normal / Elite / Boss |
| wave_number | int | 1-∞ | Dalga Sistemi | Mevcut dalga numarası |
| wave_hp_scale | float | 0.1-0.3 | Tuning knob | Dalga başına HP artış oranı |

**Örnek hesap (Wave 5):**
- Bat (base 10), Normal: `10 * 1 * (1 + 5 * 0.15) = 17.5 → 18 HP`
- Bat, Elite: `10 * 3 * (1 + 5 * 0.15) = 52.5 → 53 HP`
- Bat, Boss: `10 * 10 * (1 + 5 * 0.15) = 175 HP`

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| One-shot (hasar > max_hp) | Oyuncu ölür, HP 0'a gider | One-shot mümkün — boss saldırıları güçlü olmalı |
| Armor > raw_damage | 1 hasar verilir (min damage) | Armor asla tam koruma sağlamaz |
| Healing max_hp'yi aşar | max_hp'ye clamp, fazlası kaybolur | Overheal yok (basitlik) |
| Aynı frame'de çoklu hasar | İlk hasar alınır, invulnerability başlar, geri kalanı engellenir | Burst hasar frustration'ı önlenir |
| HP orb ekranda kalır | 30 sn timeout sonra kaybolur; manyetik çekim radius'unda otomatik toplanır | Ekran temizliği |
| Dead düşmana hasar | İşlenmez — Dead state'te TakeDamage çağrısı ignore edilir | Ghost-hit önleme |
| wave_hp_scale çok yüksek | Clamp: 0.3 maksimum | Sponge düşmanlar "Hızlı Run" pillarını ihlal eder |
| max_hp modifier ile HP artışı | current_hp aynı kalır, max_hp artar. HP orb ile doldurulmalı. | Max HP artışı = bedava heal olmamalı |
| max_hp modifier ile HP düşüşü | current_hp yeni max_hp'ye clamp edilir (fazlası kesilir) | Tutarlılık — current > max olamaz |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Player Controller** | Downstream | Hard — OnDeath → Dead state tetikler |
| **Auto-Attack System** | Upstream hasar kaynağı | Hard — hasar gönderir |
| **Projectile/Damage** | Upstream hasar kaynağı | Hard — hasar gönderir |
| **XP & Level-Up** | Downstream | Hard — düşman ölümünde XP tetikler |
| **Düşman AI** | İki yönlü | Hard — düşman temas hasarı + düşman HP verisi |
| **Mutasyon Sistemi** | Upstream sağlayıcı | Soft — armor ve hp_modifier (yoksa 0) |
| **Gameplay HUD** | Downstream | Soft — HP/Armor bar görüntüleme |
| **Dalga/Spawning** | Upstream sağlayıcı | Soft — wave_number (yoksa 1) |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `base_max_hp` | 100 | 50-200 | Daha dayanıklı oyuncu | Kırılgan oyuncu |
| `invulnerability_window` | 0.5 | 0.2-1.0 | Daha az hasar spam'ı | Hızlı hasar birikimi |
| `hp_drop_chance` | 0.05 | 0.01-0.15 | Daha çok iyileşme | Az iyileşme, riskli |
| `hp_orb_value` | 10 | 5-25 | Güçlü iyileşme | Zayıf iyileşme |
| `hp_orb_magnet_radius` | 1.5 | 0.5-3.0 | Uzaktan çekim | Üzerine yürümek gerekir |
| `hp_orb_timeout` | 30 | 15-60 | Orb'lar daha uzun kalır | Hızlı kaybolur |
| `wave_hp_scale` | 0.15 | 0.1-0.3 | Düşmanlar daha hızlı güçlenir | Yavaş güçlenme |
| `elite_hp_multiplier` | 3.0 | 2.0-5.0 | Dayanıklı elite | Kolay elite |
| `boss_hp_multiplier` | 10.0 | 5.0-20.0 | Uzun boss savaşı | Kısa boss savaşı |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Hasar alma | Kırmızı flash (0.1sn), hafif screen shake | "hit" SFX | High / High |
| Düşük HP (< %25) | HP bar kırmızı yanıp söner, vignette efekti | Kalp atışı SFX (loop) | High / Medium |
| HP orb toplama | Yeşil parıltı, +HP sayı popup | "heal" pling SFX | High / High |
| Düşman ölümü | Patlama/dağılma, XP gem + (şans) HP orb spawn | "death" pop SFX | High / Medium |
| Elite ölümü | Büyük patlama, ekran flash | Güçlü "boom" SFX | High / High |
| Boss ölümü | Büyük patlama, slow-mo (0.3sn), confetti | Epik ölüm SFX + jingle | High / High |
| Invulnerability aktif | Sprite yanıp söner (flicker) | — | Medium / — |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Oyuncu HP bar | HUD üst/alt köşe veya karakter üstü | Her frame | Her zaman |
| Oyuncu Armor göstergesi | HP bar'ın yanında veya üstünde | Armor değiştiğinde | Armor > 0 |
| Düşman HP bar | Düşman üzerinde (küçük bar) | Hasar alınca | Sadece Elite ve Boss |
| HP değişim popup | Karakter/düşman üzerinde yüzen rakam | Hasar/heal anında | Her hasar/heal |
| Düşük HP uyarısı | Ekran kenarı kırmızı vignette | HP < %25 | Oyuncu düşük HP'de |

## Acceptance Criteria

- [ ] Oyuncu konfigüre edilmiş max_hp ile başlar
- [ ] Hasar aldığında HP düşer, armor varsa önce armor'dan düşülür
- [ ] Minimum hasar her zaman 1 (armor > damage durumunda bile)
- [ ] HP ≤ 0'da OnDeath event tetiklenir
- [ ] Hasar sonrası invulnerability_window süresi boyunca ek hasar alınmaz
- [ ] HP orb toplanınca HP artar, max_hp'yi aşmaz
- [ ] Düşman HP'leri tür/tier/dalga bazında doğru ölçeklenir (örnek hesapla doğrula)
- [ ] Elite/Boss HP barları görünür, normal düşmanlar HP barı göstermez
- [ ] Dead state'teki entity'lere hasar verilemez
- [ ] **Performance:** 500 entity'de TakeDamage çağrısı < 0.05ms
- [ ] Tüm HP/Armor/scaling değerleri ScriptableObject'ten okunur (hardcoded değil)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Elemental hasar türleri olacak mı? (fire, ice, vb.) | Mutasyon Sistemi GDD | Mutasyon tasarımında | Provizönel: Tek hasar türü, elemental sonra eklenebilir |
| Armor kırılabilir mi (durability)? | Balans testi | Vertical Slice | Provizönel: Armor sabit, kırılmaz |
| Invulnerability tüm hasar türlerinden mi? | Playtest | MVP | Başlangıçta tüm kaynaklardan, playtest ile ayarla |
| HP orb manyetik çekim hızı ne olmalı? | Playtest | MVP | Prototipte test — sabit hız veya ivmeli yaklaşma |
