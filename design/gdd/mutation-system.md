# Modüler Mutasyon Sistemi

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Görsel Evrim Tatmini, Synergy Keşfi, Kolay Giriş/Derin Derinlik

## Overview

Modüler Mutasyon Sistemi, SYNTHBORN'un ayırt edici çekirdek mekaniğidir. Oyuncu level-up'ta mutasyonlar seçer. İki mutasyon kategorisi vardır: **Slot Mutasyonları** (4 vücut slotu — kol, bacak, sırt, baş — her biri görsel + mekanik değişiklik) ve **Pasif Mutasyonlar** (slotsuz, sınırsız, stat boost ve özel efektler). Slot mutasyonları kalıcıdır (run içi değiştirilemez), stratejik kararlar gerektirir. Belirli mutasyon kombinasyonları synergy'ler tetikler (Synergy Matrisi — ayrı GDD). Bu sistem Auto-Attack, Projectile/Damage, Player Controller ve Entity Health sistemlerinin davranışını modifiye eder.

## Player Fantasy

Her level-up'ta "ne olacağım?" sorusu. Kol slotuna bıçak mutasyonu → kollar bıçağa dönüşür, yakın saldırı kazanılır. Sırt slotuna kanat mutasyonu → kanatlar çıkar, hareket hızı artar. Baş slotuna göz mutasyonu → gözler lazer fırlatır. Pasif olarak "vampirism" → her kill HP verir. Run sonunda başlangıç formundan tamamen farklı, benzersiz bir yaratık. "Bu run'da ne oldum, bakın!" — screenshot paylaşılabilir bir kimlik.

## Detailed Rules

### Core Rules

**Mutasyon Kategorileri:**
1. **Slot Mutasyonları** — 4 vücut slotundan birine takılır. Görsel değişiklik + yetenek/saldırı ekler.
2. **Pasif Mutasyonlar** — Slot gerektirmez, sınırsız taşınabilir. Stat boost veya özel efekt verir. Görsel değişiklik minimal (ikon ile gösterilir).

**Vücut Slotları:**
3. 4 slot: **Kol** (Arms), **Bacak** (Legs), **Sırt** (Back), **Baş** (Head)
4. Her slotta en fazla 1 slot mutasyonu bulunabilir
5. Slot mutasyonu seçimi kalıcıdır — run içinde değiştirilemez, kaldırılamaz
6. Slot doluyken o slot için yeni mutasyon sunulmaz (sadece boş slotlara)

**Slot Mutasyon Etkileri (slot bazlı):**
- **Kol:** Saldırı türünü değiştirir veya yeni saldırı slotu ekler (Auto-Attack'a AttackModifier)
- **Bacak:** Hareket modifiye eder (Player Controller'a speed_modifier, dash_cd_modifier)
- **Sırt:** Defansif veya utility etki (Entity Health'e armor, area efektler)
- **Baş:** Özel yetenek veya aura (otomatik targeting, XP magnet, vision)

**Pasif Mutasyonlar:**
7. Slot gerektirmez, stacking sınırı yok (aynı pasifi birden fazla alamazsın)
8. Pasifler modifiye eder: damage_modifier, crit_chance, crit_multiplier, attack_speed_modifier, speed_modifier, max_hp_modifier, armor, xp_gain_modifier, magnet_radius_modifier, vb.
9. Bazı pasifler özel efekt verir: "her kill'de %3 HP", "crit'te AoE patlama", "düşük HP'de hasar artışı"
10. Pasif mutasyonlar synergy'leri tetikleyebilir (slot + pasif veya pasif + pasif)

**Mutasyon Seçimi (Level-Up Sırasında):**
11. Level-up'ta 3 mutasyon kartı sunulur
12. Sunulan mutasyonlar Mutasyon Havuzu / Loot Table tarafından belirlenir (ayrı GDD)
13. 4 slot dolmadan: havuz slot + pasif karışık sunar
14. 4 slot dolduktan sonra: havuz sadece pasif sunar
15. Oyuncu 3'ten birini seçmek zorundadır (atlama yok)
16. Seçim yapıldığında: mutasyon aktif edilir, efektleri anında uygulanır

**Mutasyon Data Yapısı:**
17. Her mutasyon bir `MutationData` ScriptableObject'tir:
    - `id`: benzersiz tanımlayıcı
    - `display_name`: oyuncuya gösterilen isim
    - `description`: efekt açıklaması
    - `category`: Slot veya Passive
    - `slot_type`: (slot ise) Arms / Legs / Back / Head
    - `rarity`: Common / Uncommon / Rare / Legendary
    - `stat_modifiers`: Dictionary<StatType, float> — uygulanacak stat değişiklikleri
    - `special_effect`: (varsa) özel efekt ID'si
    - `visual_prefab`: (slot ise) sprite katmanı referansı
    - `synergy_tags`: string[] — synergy tespiti için etiketler
    - `icon`: UI ikonu

**Stat Modifier Sistemi:**
18. Tüm modifier'lar toplanarak ilgili sisteme uygulanır
19. Modifier tipleri: `Additive` (toplam), `Multiplicative` (çarpım)
20. Uygulama sırası: Base × (1 + Additive Sum) × Multiplicative Product
21. Her stat'ın min/max clamp'ı var (ilgili sistem GDD'lerinde tanımlandı)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Inactive** | Run başlamadı | Run başlar | Mutasyon listesi boş |
| **Active** | Run başlangıcı | Run sonu | Mutasyonlar toplanıyor, efektler aktif |
| **Selecting** | Level-up tetiklendi | Oyuncu seçim yaptı | 3 kart sunulur, seçim beklenir |
| **Applying** | Mutasyon seçildi | Efektler uygulandı | Stat modifier eklenir, görsel güncellenir, synergy check |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Auto-Attack System** | Downstream | Kol mutasyonları → AttackModifier eklenir (yeni saldırı türleri, hız değişiklikleri) |
| **Player Controller** | Downstream | Bacak mutasyonları → speed_modifier, dash_cd_modifier sağlar |
| **Entity Health** | Downstream | Sırt/pasif mutasyonları → max_hp_modifier, armor sağlar |
| **Projectile/Damage** | Downstream | Pasif mutasyonlar → damage_modifier, crit_chance, crit_multiplier |
| **XP & Level-Up** | Upstream | Level-up → mutasyon seçimi tetiklenir. Pasifler → xp_gain_modifier |
| **Sprite Compositing** | Downstream | Slot mutasyonları → görsel sprite katmanı eklenir/değişir |
| **Synergy Matrisi** | Downstream | Her mutasyon eklendiğinde synergy check tetiklenir |
| **Mutasyon Havuzu** | Upstream | 3 kart seçimi bu sistemden gelir |
| **Mutasyon Seçim UI** | İki yönlü | 3 kart gösterimi, seçim geri bildirimi |

## Formulas

### Stat Modifier Application

**"Higher is better" stats** (speed, damage, HP, crit):
```
effective_stat = base_stat * (1 + sum(additive_modifiers)) * product(multiplicative_modifiers)
effective_stat = clamp(effective_stat, stat_min, stat_max)
```

**"Lower is better" stats** (attack interval, dash cooldown):
```
effective_stat = base_stat * (1 - sum(additive_modifiers)) * product(multiplicative_modifiers)
// modifier represents reduction fraction, clamped to [0.0, max_clamp]
effective_stat = clamp(effective_stat, stat_min, stat_max)
```

**Convention per stat:**
| Stat | Convention | Example modifier +0.2 meaning |
|------|-----------|-------------------------------|
| move_speed | Higher is better | +20% faster |
| damage | Higher is better | +20% more damage |
| max_hp | Higher is better | +20% more HP |
| crit_chance | Higher is better | +20% more crit |
| crit_multiplier | Higher is better | +20% more crit damage |
| attack_interval | Lower is better | 20% shorter interval (faster attacks) |
| dash_cooldown | Lower is better | 20% shorter cooldown |

**Örnek: Attack Speed (lower is better)**
- Base interval: 1.0 sn
- Kol mutasyonu: +0.2 (additive)
- Pasif "Haste": +0.15 (additive)
- Toplam additive: 0.35
- `effective_interval = 1.0 * (1 - 0.35) = 0.65 sn`
- Clamp: max(0.65, 0.1) = 0.65 sn ✓

**Örnek: Move Speed (higher is better)**
- Base speed: 5.0 birim/sn
- Bacak mutasyonu: +0.3 (additive)
- Pasif "Swift": +0.2 (additive)
- Toplam additive: 0.5
- `effective_speed = 5.0 * (1 + 0.5) = 7.5 birim/sn`

### Mutasyon Dağılım Hedefi (12 dk run)
```
Hedef ~15 mutasyon/run:
- Slot mutasyonları: 4 (tüm slotlar dolu)
- Pasif mutasyonlar: ~11
- Toplam: ~15
```

### Rarity Distribution (Mutasyon Havuzu GDD'sinde detaylandırılacak)
| Rarity | Oran | Güç Seviyesi |
|--------|------|-------------|
| Common | %50 | Temel stat boost (+10-15%) |
| Uncommon | %30 | Orta stat boost (+20-30%) veya basit özel efekt |
| Rare | %15 | Güçlü stat boost (+40%+) veya güçlü özel efekt |
| Legendary | %5 | Oyun değiştirici etki veya çok güçlü synergy enabler |

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 4 slot dolu, 3 kart hepsi slot mutasyonu | Olmaz — Havuz sistemi slot doluyken slot mutasyonu sunmaz | Tasarım garantisi |
| Aynı pasif ikinci kez sunulur | Olmaz — alınmış mutasyonlar havuzdan çıkar | Tekrarlama = ilgisizlik |
| Tüm pasifler alınmış (çok uzun run) | Havuzda mutasyon kalmadığında: "stat boost" kartları sunulur (generic +%10 damage/HP/speed) | Fallback — asla boş seçim yok |
| Mutasyon synergy beklenmedik güç yaratır | Kabul edilebilir — "broken combo bulma" oyunun eğlencesi. Ancak her synergy playtest ile doğrulanmalı | Pillar 3: Synergy Keşfi. Dengeli denge yerine "keşif heyecanı" |
| Mutasyon stat'ı negatife iter (örn: hız -0.5 altına) | İlgili sistem kendi clamp'ini uygular. Mutasyon sistemi uyarı vermez. | Sorumluluk her sistemin kendi GDD'sinde |
| Level-up sırasında oyuncu AFK (seçim yapmıyor) | Oyun duraklar (time scale 0), süresiz bekler. Timeout yok. | Seçim baskısı olmamalı — Pillar 4 |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Auto-Attack** | Downstream | Hard — kol mutasyonları saldırıyı değiştirir |
| **Player Controller** | Downstream | Hard — bacak mutasyonları hareketi değiştirir |
| **Entity Health** | Downstream | Soft — HP/armor modifier (yoksa 0) |
| **Projectile/Damage** | Downstream | Soft — damage/crit modifier (yoksa 0) |
| **XP & Level-Up** | Upstream | Hard — level-up tetikleme |
| **Sprite Compositing** | Downstream | Hard — slot mutasyonları görsel değiştirir |
| **Synergy Matrisi** | Downstream | Soft — synergy check (yoksa synergy yok) |
| **Mutasyon Havuzu** | Upstream | Hard — 3 kart sağlar |
| **Mutasyon Seçim UI** | İki yönlü | Hard — kart gösterimi ve seçim |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `slot_count` | 4 | 3-6 | Daha fazla görsel mutasyon | Daha az, her biri daha önemli |
| `cards_per_levelup` | 3 | 2-4 | Daha fazla seçenek, daha yavaş seçim | Az seçenek, hızlı karar |

> **Note:** Rarity weights (`common_weight`, `uncommon_weight`, `rare_weight`, `legendary_weight`) owned by mutation-pool.md.

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Slot mutasyonu seçimi | Vücut parçası morph animasyonu, parıltı efekti | "transform" SFX (satisfying, meaty) | Critical / Critical |
| Pasif mutasyon seçimi | İkon HUD'a eklenir, kısa flash | "pickup" pling | High / Medium |
| Synergy tetiklenme | Özel synergy efekti, banner "SYNERGY!" | Özel synergy jingle | Critical / Critical |
| Slot mutasyonu aktif (gameplay sırasında) | Sprite katmanı karakter üzerinde görünür | — | Critical / — |
| Rarity gösterimi (kart) | Kart çerçevesi rarity renginde (beyaz/yeşil/mavi/altın) | — | High / — |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Mutasyon Seçim Ekranı | Ekran ortası (tam ekran overlay) | Level-up'ta | Level-up |
| 3 mutasyon kartı | Yan yana, animasyonlu giriş | Seçim başında | Selecting state |
| Kart detayları | Kart üzerinde: isim, açıklama, rarity, slot/pasif, stat değişiklikleri | Hover/focus | Kart görüntülenirken |
| Synergy ipucu | Kartın altında "X ile synergy!" uyarısı | Eğer mevcut mutasyonlarla synergy varsa | Synergy mümkün olduğunda |
| Aktif mutasyonlar listesi | HUD kenarında küçük ikonlar (slot + pasif ayrı) | Mutasyon eklendiğinde | Her zaman |
| Slot durumu | 4 slot göstergesi (dolu/boş) | Mutasyon seçiminde | Selecting state |

## Acceptance Criteria

- [ ] 4 vücut slotuna mutasyon eklenebilir (birer tane)
- [ ] Slot mutasyonları kalıcı (değiştirilemez, kaldırılamaz)
- [ ] Slot doluyken o slot için mutasyon sunulmaz
- [ ] Pasif mutasyonlar sınırsız toplanabilir (tekrar yok)
- [ ] Stat modifier'ları doğru hesaplanır (additive + multiplicative + clamp)
- [ ] Kol mutasyonları Auto-Attack'ı modifiye eder
- [ ] Bacak mutasyonları Player Controller'ı modifiye eder
- [ ] Sırt/Baş mutasyonları ilgili sistemleri modifiye eder
- [ ] Her mutasyon seçiminde Synergy Matrisi check tetiklenir
- [ ] Slot mutasyonları Sprite Compositing'e görsel katman ekler
- [ ] Tüm pasif havuz tükendiyse fallback kartlar sunulur
- [ ] **Performance:** Mutasyon seçimi ve stat recalculation < 1ms
- [ ] Tüm mutasyon verileri ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| MVP mutasyon sayısı: Slot başına kaç mutasyon? | Game Designer | MVP | Hedef: slot başına 3 (12 slot mutasyonu) + 10-15 pasif = ~25 mutasyon |
| Mutasyon "special_effect" sistemi nasıl implement edilir? (Strategy pattern? ScriptableObject callback?) | Lead Programmer | MVP prototype | ADR gerekli — /architecture-decision ile belgelenecek |
| Legendary mutasyonlar ne kadar "broken" olabilir? | Balans testi | Vertical Slice | Playtest ile — "fun > balance" ama run'ı bitirmemeli |
| Pasif mutasyonlar görsel olarak nasıl yansısın? (Hiç mi, aura mı, renk mi?) | Art Director | Vertical Slice | MVP'de sadece ikon, VS'de aura/parıltı değerlendirilir |
