# Mutasyon Seçim UI

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Kolay Giriş/Derin Derinlik, Synergy Keşfi

## Overview

Mutasyon Seçim UI, level-up'ta oyuncuya 3 mutasyon kartı sunan ve seçim yapmasını sağlayan tam ekran overlay arayüzüdür. Oyun duraklar, 3 kart animasyonlu olarak ortaya çıkar, oyuncu birini seçer. Her kart: mutasyon adı, açıklama, rarity çerçevesi, slot/pasif göstergesi ve synergy ipucu içerir. Bu, run'ın en kritik karar anı — hızlı ama bilgili seçim yapılabilmeli.

## Player Fantasy

"3 kart, hangisi?" heyecanı. Kartlar açılırken "ne geldi?" merakı, Legendary kartı gördüğünde "EVET!" çığlığı. Synergy ipucu gördüğünde "bu ikisini birleştirmeliyim" stratejik düşünce. Seçim yapınca anında geri dönen aksiyon. Seçim ekranı oyunu kesmemeli — aksine "bir sonraki mutasyon ne olacak?" beklentisi yaratmalı.

## Detailed Rules

### Core Rules

**Ekran Yapısı:**
1. Level-up'ta time scale = 0 (oyun duraklar)
2. Arka plan karartılır (dim overlay, %60 opacity)
3. 3 kart ekranın ortasında yan yana gösterilir
4. Kartlar sırayla flip animasyonuyla açılır (sol→orta→sağ, her biri 0.15 sn)

**Kart İçeriği:**
5. Mutasyon ikonu (büyük, kartın üst yarısı)
6. Mutasyon adı (kalın text)
7. Kısa açıklama (efekt özeti, 1-2 satır)
8. Rarity çerçevesi: Common=beyaz, Uncommon=yeşil, Rare=mavi, Legendary=altın
9. Slot göstergesi: slot mutasyonuysa ilgili slot ikonu (kol/bacak/sırt/baş). Pasif ise "Pasif" etiketi.
10. Stat değişiklikleri: +%20 Damage, +5 Armor gibi kısa stat listesi
11. Synergy ipucu: mevcut mutasyonlarla synergy potansiyeli varsa, kart altında küçük synergy ikonu + "Synergy!" etiketi

**Seçim:**
12. Oyuncu karta tıklar/basar → seçim onaylanır
13. Seçilen kart büyüme animasyonu + parıltı, diğer 2 kart fade-out
14. 0.3 sn sonra overlay kapanır, oyun devam eder (time scale = 1)
15. Oyuncu seçim yapmadan ekranı kapatamaz (zorunlu seçim)

**Input:**
16. Mouse click veya gamepad A butonu ile seçim
17. Gamepad: sol/sağ ile kartlar arası geçiş, A ile onay
18. Klavye: 1/2/3 tuşları ile hızlı seçim (speedrun friendly)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Opening** | Level-up event | 3 kart açıldı | Overlay fade-in, kartlar sırayla flip |
| **Selecting** | Kartlar açık | Oyuncu kart seçti | Hover efektleri, kart detayları |
| **Closing** | Seçim yapıldı | Overlay kapandı | Seçilen kart animasyonu, fade-out |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **XP & Level-Up** | Upstream | Level-up event → UI açılır |
| **Mutasyon Havuzu** | Upstream | 3 MutationData + synergy flag → kart içeriği |
| **Mutasyon Sistemi** | Downstream | Seçilen mutasyon ID → Mutasyon Sistemi'ne bildirilir |
| **Gameplay HUD** | — | Seçim ekranı açıkken HUD gizlenir |

## Formulas

Yok — bu sistem hesaplama yapmaz, sadece veri gösterir ve seçim alır.

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Zincir level-up (2-3 level aynı anda) | Her biri için ayrı seçim ekranı. İlk seçim sonrası ikinci açılır. | Her mutasyon kararı bilinçli olmalı |
| Oyuncu AFK (seçim yapmıyor) | Ekran süresiz açık kalır (time scale 0). Timeout yok. | Pillar 4: baskı yok |
| 3 kartın hepsi "kötü" | Oyuncu yine de seçmeli. Mutasyon Havuzu bunu minimize eder. | Zorunlu seçim = stratejik karar |
| Gamepad bağlantısı kesilir | Mouse/klavye fallback her zaman aktif | Input robustluğu |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **XP & Level-Up** | Upstream | Hard — level-up tetikleme |
| **Mutasyon Havuzu** | Upstream | Hard — 3 kart verisi |
| **Mutasyon Sistemi** | Downstream | Hard — seçim bildirimi |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect |
|-----------|---------|------------|--------|
| `card_flip_duration` | 0.15 | 0.1-0.3 | Kart açılma hızı |
| `card_flip_stagger` | 0.1 | 0.05-0.2 | Kartlar arası gecikme |
| `selection_close_delay` | 0.3 | 0.2-0.5 | Seçim sonrası kapanma gecikmesi |
| `overlay_dim_opacity` | 0.6 | 0.4-0.8 | Arka plan karartma |

## Visual/Audio Requirements

| Event | Visual | Audio | Priority |
|-------|--------|-------|----------|
| UI açılma | Overlay fade-in + kart flip | "whoosh" + kart çevirme SFX | High / High |
| Rare+ kart | Kart çerçevesi parlar | "rare!" pling | High / High |
| Legendary kart | Yıldız efekti + ekstra parlak | Epik jingle | Critical / Critical |
| Kart hover | Kart hafif büyür + glow | Hafif hover SFX | Medium / Low |
| Seçim yapıldı | Seçilen kart büyür + parıltı, diğerleri fade | "select" confirmation SFX | High / High |
| Synergy ipucu | Kart altında pulsating ikon | — | Medium / — |

## Acceptance Criteria

- [ ] Level-up'ta 3 kart doğru gösterilir
- [ ] Kart içeriği doğru: ad, açıklama, rarity, slot, stat, synergy
- [ ] Seçim yapılabilir (mouse, gamepad, klavye)
- [ ] Seçim sonrası oyun devam eder
- [ ] Seçim ekranı açıkken oyun duraklar
- [ ] Zincir level-up sıralı çalışır
- [ ] Rarity çerçevesi doğru renkte
- [ ] Synergy ipucu doğru gösterilir
- [ ] **Performance:** UI açılma < 100ms
- [ ] UI Toolkit ile implement edilir

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Reroll butonu eklenecek mi? | Game Designer | Vertical Slice | MVP: yok |
| Kart karşılaştırma (mevcut vs yeni) gösterilsin mi? | UX Designer | Vertical Slice | MVP: sadece yeni kart. VS: side-by-side comparison |
