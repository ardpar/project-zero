# Projectile / Damage System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar, Synergy Keşfi

## Overview

Projectile/Damage System, Auto-Attack System'in çıktısı olan mermileri oluşturan, hareket ettiren, çarpışma tespiti yapan ve hasar hesaplayan sistemdir. Her projectile'ın türü, hızı, hasarı, pierce sayısı ve ömrü vardır. Mutasyonlar yeni projectile türleri ekler ve mevcut olanları modifiye eder. Bu sistem ekrandaki "görsel kaos"un birincil kaynağıdır — yüzlerce mermi aynı anda uçabilir.

## Player Fantasy

Oyuncu ekranı dolduran mermileri gördükçe "güçlüyüm" hissetmeli. İlk run'da tek projectile, son dakikalarda ekranı kaplayan lazer + girdap + mermi yağmuru — güç spirali burada somutlaşır. Her yeni mutasyon saldırıyı görsel olarak daha büyük ve kaotik yapmalı.

## Detailed Rules

### Core Rules

**Projectile Oluşturma:**
1. Auto-Attack System `FireProjectile(origin, direction, projectileData)` çağrısı yapar
2. Her projectile bir `ProjectileData` ScriptableObject'ten türer: hız, hasar, menzil, pierce, AoE, lifetime
3. Projectile object pool'dan alınır (new GameObject oluşturulmaz — performans kritik)

**Projectile Hareket:**
4. Projectile `projectile_speed` (birim/sn) ile `direction` yönünde doğrusal hareket eder
5. Homing projectile: hedef düşmana doğru `homing_strength` ile döner (bazı mutasyonlar)
6. Projectile `max_lifetime` (varsayılan: 3.0 sn) veya `max_range` (varsayılan: 12.0 birim) aştığında yok olur (hangisi önce gelirse)

**Çarpışma ve Hasar:**
7. Projectile düşmanla çarpıştığında `TakeDamage(damage, source)` çağrısı yapar (Entity Health System)
8. Çarpışma sonrası davranış `on_hit_behavior` ile belirlenir:
   - **Destroy**: Çarpışmada yok olur (varsayılan)
   - **Pierce**: `pierce_count` kadar düşmandan geçer, her geçişte hasar verir
   - **AoE**: Çarpışma noktasında `aoe_radius` alanında patlama, alandaki herkese hasar
   - **Chain**: Vurulan düşmandan en yakın başka düşmana sıçrar, `chain_count` kez
9. Aynı düşmana aynı projectile birden fazla hasar veremez (pierce hariç — pierce'da bile 0.1sn cooldown)

**Hasar Hesaplama:**
10. `final_damage = base_damage * (1 + damage_modifier) * crit_multiplier`
11. Critical hit şansı: `crit_chance` (varsayılan: %5)
12. Critical çarpan: `crit_multiplier` (varsayılan: 2.0x)
13. Hasar sonucu integer'a yuvarlanır (minimum 1)

**Düşman Projectile'ları:**
14. Bu sistem düşman mermilerini de yönetir (aynı altyapı, farklı data)
15. Düşman projectile'ları oyuncuya hasar verir (Entity Health)
16. Oyuncu düşman projectile'larını yok edemez (başlangıçta — mutasyonla değişebilir)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Flying** | FireProjectile çağrısı | Çarpışma / Lifetime / Range aşıldı | Direction'da hareket, collision check |
| **Hit** | Düşmanla çarpışma | On-hit behavior tamamlandı | Hasar verme, pierce/AoE/chain tetikleme |
| **Expired** | Lifetime/range aşıldı | Pool'a geri döndü | Görsel fade-out, pool return |
| **Paused** | Game pause | Resume | Tüm projectile'lar dondurulur |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Auto-Attack System** | Upstream | `FireProjectile(origin, direction, projectileData)` tetikleme |
| **Entity Health** | Downstream | `TakeDamage(damage, source)` çağrısı (hasar verme) |
| **Mutasyon Sistemi** | Upstream sağlayıcı | `damage_modifier`, `crit_chance`, `crit_multiplier`, projectile türü değişiklikleri, on_hit_behavior override |
| **VFX / Juice** | Downstream | Projectile trail, impact efekti, AoE patlama, crit efekti |
| **Audio Manager** | Downstream | Impact SFX, crit SFX, AoE patlama SFX |

## Formulas

### Final Damage
```
is_crit = random(0,1) < crit_chance
crit_mult = is_crit ? crit_multiplier : 1.0
final_damage = max(round(base_damage * (1 + damage_modifier) * crit_mult), 1)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_damage | int | 1-100 | ProjectileData | Temel mermi hasarı |
| damage_modifier | float | 0.0-3.0 | Mutasyon Sistemi | Toplam hasar artışı |
| crit_chance | float | 0.0-0.5 | Mutasyon Sistemi + base | Kritik vuruş şansı |
| crit_multiplier | float | 1.5-4.0 | Mutasyon Sistemi + base | Kritik çarpanı |

**Başlangıç örneği:** base 10, modifier 0, crit 5%, mult 2.0 → Normal: 10, Crit: 20
**Geç oyun örneği:** base 10, modifier 1.5, crit 25%, mult 3.0 → Normal: 25, Crit: 75

### Pierce Damage Decay
```
pierce_damage = base_damage * (1 - pierce_index * pierce_decay)
// pierce_index: 0 = ilk hedef, 1 = ikinci, ...
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| pierce_decay | float | 0.0-0.3 | ProjectileData | Her geçişte hasar azalma oranı |
| pierce_index | int | 0-pierce_count | Runtime | Kaçıncı hedef |

**Örnek:** base 20, decay 0.15 → 1. hedef: 20, 2. hedef: 17, 3. hedef: 14

### AoE Damage Falloff
```
aoe_damage = base_damage * (1 - (distance_from_center / aoe_radius) * aoe_falloff)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| aoe_radius | float | 1.0-5.0 | ProjectileData | Patlama yarıçapı |
| aoe_falloff | float | 0.0-1.0 | ProjectileData | Mesafeyle hasar azalma (0 = tam hasar, 1 = kenarda 0) |

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 500+ projectile ekranda | Object pool + deactivation. Max aktif projectile limiti: 300 | Performans koruması |
| Pierce projectile son düşmanda | Pierce biter, projectile destroy | Normal davranış |
| AoE friendly fire (oyuncuya) | Oyuncu kendi AoE'sinden hasar almaz | Frustration önleme |
| Chain hedefi bulamaz | Chain durur, projectile destroy | Sıçrayacak düşman yoksa normal bitiş |
| Projectile duvardan geçer mi? | Evet — arena sınırları projectile'ı durdurmaz, lifetime/range sınırlar | Duvar arkası stratejisi yok — açık arena |
| Crit + Pierce + AoE aynı anda | Her mekanik sırayla uygulanır: crit hasarı hesapla → pierce geç → AoE tetikle | Synergy stack — kasıtlı |
| Düşman ölür, projectile havadayken | Projectile devam eder (pierce ise sonraki hedefe), destroy ise expire | Ghost-hit yok — dead entity'ye hasar verilmez (Entity Health garantisi) |
| Pause sırasında projectile'lar | Tümü dondurulur (pozisyon + velocity) | Adil pause |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Auto-Attack System** | Upstream | Hard — projectile oluşturma çağrısı |
| **Entity Health** | Downstream | Hard — hasar verme |
| **Mutasyon Sistemi** | Upstream sağlayıcı | Soft — damage/crit modifier, projectile türü (yoksa varsayılan) |
| **VFX / Juice** | Downstream | Soft — trail, impact, patlama efektleri |
| **Gameplay HUD** | Downstream | Soft — damage number popup events |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `default_projectile_speed` | 10.0 | 5.0-20.0 | Hızlı mermi, kolay isabet | Yavaş mermi, kaçırılabilir |
| `default_base_damage` | 10 | 5-25 | Güçlü başlangıç | Zayıf başlangıç |
| `default_crit_chance` | 0.05 | 0.0-0.15 | Daha sık crit | Nadir crit |
| `default_crit_multiplier` | 2.0 | 1.5-4.0 | Güçlü crit spike | Hafif crit bonus |
| `max_active_projectiles` | 300 | 150-500 | Daha fazla ekran kaosu | Temiz ekran |
| `default_max_lifetime` | 3.0 | 1.0-5.0 | Uzun menzil (dolaylı) | Kısa menzil |
| `default_pierce_count` | 0 | 0-5 | Pierce başlangıçtan (genellikle mutasyonla) | Pierce yok |
| `pierce_decay` | 0.15 | 0.0-0.3 | Hasar daha hızlı düşer | Hasar sabit kalır |
| `aoe_falloff` | 0.5 | 0.0-1.0 | Kenarlarda zayıf hasar | Her yerde tam hasar |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Projectile spawn | Mermi sprite + trail | Hafif "fire" SFX | High / Low |
| Düşmana isabet | Impact flash, hasar rakam popup | "hit" SFX | High / High |
| Critical hit | Büyük impact, renkli flash (sarı), büyük rakam | Güçlü "crit" SFX | High / High |
| AoE patlama | Daire şeklinde patlama animasyonu | "boom" SFX | High / High |
| Pierce geçiş | Düşmandan geçen mermi trail'i parlar | Hafif "swoosh" | Medium / Low |
| Chain sıçrama | Hedefler arası yıldırım/çizgi efekti | "zap" SFX | High / Medium |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Hasar rakamları (popup) | Düşman üzerinde yüzen rakam | Her hasar anında | Her zaman |
| Crit göstergesi | Büyük, renkli (sarı/turuncu) hasar rakamı | Crit anında | Crit olduğunda |

## Acceptance Criteria

- [ ] Projectile oluşturulur ve doğrusal hareket eder
- [ ] Düşmanla çarpışmada TakeDamage doğru çağrılır
- [ ] Pierce, AoE, Chain mekanikleri ayrı ayrı ve birlikte çalışır
- [ ] Crit hesabı doğru (şans + çarpan)
- [ ] Object pool kullanılır (GC allocation yok)
- [ ] 300 aktif projectile sınırı aşıldığında en eski expire edilir
- [ ] Aynı projectile aynı düşmana tekrar hasar vermez (pierce cooldown hariç)
- [ ] AoE oyuncuya hasar vermez
- [ ] Pause sırasında tüm projectile'lar durur
- [ ] **Performance:** 300 projectile update + collision < 2ms (60fps budget'ın %12'si)
- [ ] Tüm değerler ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Projectile collision: Physics2D trigger mi, manual raycast mı? | Gameplay Programmer | MVP prototype | Benchmark — trigger daha basit, raycast daha performanslı |
| Düşman projectile'ları oyuncunun projectile'larıyla çarpışabilir mi? (bullet cancel) | Game Designer | Vertical Slice | Provizönel: Hayır. Potansiyel mutasyon olarak değerlendirilebilir. |
| Homing projectile dönüş hızı ne olmalı? | Playtest | MVP | Prototipte test — başlangıçta 180°/sn |
