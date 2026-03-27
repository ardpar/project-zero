# Gameplay HUD

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Kolay Giriş/Derin Derinlik

## Overview

Gameplay HUD, run sırasında oyuncuya kritik bilgiyi gösteren arayüz katmanıdır. HP/Armor bar, XP bar, dalga bilgisi, aktif mutasyon ikonları, dash cooldown ve boss HP bar'ı içerir. Pillar 4 gereği minimal ve anında okunabilir — bakış 0.5 saniyede tüm durumu kavramalı. UI Toolkit (Unity 6 production-ready) ile implement edilir.

## Player Fantasy

HUD "görünmez" hissetmeli — oyuncu bilgiye ihtiyacı olduğunda orada, yoksa fark etmiyor. "HP'im düşük!" fark etmesi <0.1 sn, "XP'im ne kadar?" bakışı <0.3 sn. Kaotik savaş sırasında bile net okunabilir.

## Detailed Design

### Core Rules

**HUD Elemanları:**

1. **HP Bar** — Ekranın sol üst köşesinde veya karakter üstünde
   - Yeşilden kırmızıya renk geçişi (HP oranıyla)
   - Armor varsa: HP bar'ın üstünde mavi armor göstergesi
   - HP < %25: kırmızı yanıp sönme + vignette
   - Sayısal değer göstergesi: `current_hp / max_hp`

2. **XP Bar** — Ekranın altında (tam genişlik, ince bar)
   - Smooth dolum animasyonu
   - Level numarası bar'ın yanında
   - Level-up anında flash + "Level Up!" text

3. **Dalga Bilgisi** — Ekranın üst ortasında
   - "WAVE X" text + kalan süre timer
   - Boss fight'ta: boss ismi + büyük HP bar

4. **Aktif Mutasyonlar** — Ekranın sağ kenarında dikey ikon listesi
   - Slot mutasyonları: slot ikonu (kol/bacak/sırt/baş şekli) + mutasyon ikonu
   - Pasif mutasyonlar: küçük ikon grid'i (altında)
   - Aktif synergy: özel synergy ikonu (parıltılı)

5. **Dash Cooldown** — Karakter altında veya HP bar yanında
   - Circular cooldown göstergesi
   - Hazır olduğunda kısa flash

6. **Boss HP Bar** — Ekranın üstünde (büyük, belirgin)
   - Boss adı üstte
   - Boss fight sırasında dalga bilgisinin yerini alır

7. **Damage Numbers** — Düşman/oyuncu üzerinde yüzen rakamlar
   - Normal hasar: beyaz, küçük
   - Crit hasar: sarı/turuncu, büyük, bounce animasyonu
   - Heal: yeşil, yukarı kayma

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Gameplay** | Run aktif | Pause / Level-up / Run sonu | Tüm HUD elemanları görünür |
| **Boss Fight** | Boss phase | Boss öldü | Boss HP bar görünür, dalga timer gizlenir |
| **Hidden** | Pause menü / Level-up UI | Resume | HUD gizlenir veya karartılır |
| **Victory** | Boss yenildi | Run Manager devralır | "VICTORY" banner, HUD fade out |
| **Death** | Oyuncu öldü | Run Manager devralır | HUD fade out, ölüm ekranı |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Entity Health** | Upstream | HP, max HP, armor → HP bar güncellemesi |
| **XP & Level-Up** | Upstream | current_xp, xp_to_next, level → XP bar |
| **Dalga/Spawning** | Upstream | wave_number, wave_timer → dalga bilgisi |
| **Player Controller** | Upstream | dash_cooldown_remaining → dash göstergesi |
| **Mutasyon Sistemi** | Upstream | aktif mutasyon listesi → ikon grid |
| **Synergy Matrisi** | Upstream | aktif synergy listesi → synergy ikonları |
| **Projectile/Damage** | Upstream | damage events → damage numbers |

## Formulas

### HP Bar Color
```
hp_ratio = current_hp / max_hp
bar_color = Lerp(RED, GREEN, hp_ratio)
// <0.25: yanıp sönme aktif
```

### Damage Number Scale
```
font_size = base_size * (1 + damage / reference_damage * 0.5)
// Crit: font_size * 1.5 + bounce animation
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Çok fazla mutasyon ikonu (15+) | İkonlar küçülür veya scrollable olur. İlk 8 görünür, "..." ile genişletilebilir | Ekran kalabalıklaştırma |
| Damage number spam (çok düşman) | Eski damage number'lar hızlıca fade out. Max 20 aynı anda. | Okunabilirlik |
| HUD + kaotik gameplay = bilgi overload | HUD minimal tutulur, renk kodlaması ile hızlı okuma | Pillar 4: kolay giriş |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Entity Health** | Upstream | Hard |
| **XP & Level-Up** | Upstream | Hard |
| **Dalga/Spawning** | Upstream | Hard |
| **Player Controller** | Upstream | Soft (dash CD) |
| **Mutasyon Sistemi** | Upstream | Soft (ikon listesi) |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect |
|-----------|---------|------------|--------|
| `low_hp_threshold` | 0.25 | 0.15-0.35 | Kırmızı uyarı eşiği |
| `damage_number_duration` | 0.8 | 0.5-1.5 | Rakam ekranda kalma süresi |
| `max_damage_numbers` | 20 | 10-30 | Aynı anda ekrandaki rakam limiti |
| `hud_opacity` | 0.9 | 0.6-1.0 | HUD şeffaflığı |

## Visual/Audio Requirements

| Event | Visual | Audio | Priority |
|-------|--------|-------|----------|
| HP düşük | Bar yanıp sönme + vignette | Kalp atışı SFX | High / Medium |
| Level-up | XP bar flash + "Level Up!" text | Level-up jingle (XP System'de) | High / — |
| Dalga geçişi | "WAVE X" banner | Wave SFX (Wave System'de) | High / — |
| Dash hazır | Cooldown göstergesi flash | Ding SFX (Player Controller'da) | Medium / — |

## Acceptance Criteria

- [ ] HP bar doğru current/max HP gösterir, renk geçişi çalışır
- [ ] XP bar smooth dolum animasyonu ile güncellenir
- [ ] Dalga numarası ve timer doğru gösterilir
- [ ] Boss HP bar boss fight'ta görünür
- [ ] Aktif mutasyonlar ikon olarak listelenir
- [ ] Dash cooldown göstergesi çalışır
- [ ] Damage numbers spawn olur, crit büyük gösterilir
- [ ] Pause/level-up'ta HUD gizlenir
- [ ] **Performance:** HUD update < 0.3ms
- [ ] UI Toolkit ile implement edilir

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| HUD pozisyonları: sabit mi, özelleştirilebilir mi? | UX Designer | Vertical Slice | MVP: sabit. VS: drag-drop özelleştirme |
| Minimap / radar gerekli mi? | Game Designer | Alpha | MVP: yok. Alpha: değerlendirilir |
