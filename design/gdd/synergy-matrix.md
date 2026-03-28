# Synergy Matrisi

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Synergy Keşfi (birincil), Görsel Evrim Tatmini

## Overview

Synergy Matrisi, mutasyon kombinasyonlarının özel bonus efektler tetiklemesini yöneten sistemdir. Oyuncu belirli mutasyonları birlikte topladığında (slot + slot, slot + pasif, veya pasif + pasif), tag eşleşmeleri üzerinden synergy tespit edilir ve bonus efekt aktive edilir. Synergy'ler oyuncuya doğrudan gösterilmez — keşfedilmesi gereken gizli kombinasyonlardır. Bu sistem "bir tane daha run" motivasyonunun birincil kaynağıdır.

## Player Fantasy

"Bıçak kolu + hız bacağı aldım ve... GIRDAP SALDIRISI açıldı!" Synergy keşfi run'ın en tatmin edici anı olmalı. Oyuncu synergy'yi keşfettiğinde sürpriz + güç hissetmeli. Toplulukta paylaşılacak: "Bu ikisini birlikte alırsan şunu açıyor, biliyor muydun?" Gizlilik keşif hissini korur, ama ipuçları (Mutasyon Havuzu'ndaki synergy bias) farkında olmadan yönlendirir.

## Detailed Rules

### Core Rules

**Tag Bazlı Synergy Sistemi:**
1. Her mutasyonun `synergy_tags` listesi var (string[])
2. Synergy, bir `SynergyDefinition` ScriptableObject ile tanımlanır:
   - `required_tags`: string[] — bu tag'lerin TÜMÜ oyuncunun aktif mutasyonlarında bulunmalı
   - `bonus_effect`: aktive edilen efekt (stat boost, yeni saldırı, özel davranış)
   - `display_name`: synergy adı
   - `description`: efekt açıklaması
   - `visual_effect`: synergy aktif olduğunda görsel feedback
3. Synergy tetikleme: her mutasyon eklendiğinde tüm SynergyDefinition'lar taranır
4. `required_tags`'in tamamı karşılandığında synergy aktive edilir

**Synergy Türleri:**
5. **İkili Synergy** (2 tag): En yaygın. İki mutasyon birleşince bonus.
6. **Üçlü Synergy** (3 tag): Nadir. Üç mutasyon birlikte daha güçlü bonus.
7. **Set Synergy**: Aynı tematik ailden N mutasyon toplamak (örn: 3 "fire" tag'li mutasyon)

**Synergy Efekt Türleri:**
8. **Stat Boost**: Ekstra modifier eklenir (örn: +%30 damage)
9. **Yeni Saldırı**: Bonus saldırı tipi eklenir (Auto-Attack'a yeni slot)
10. **Özel Davranış**: Unique efekt (örn: "öldürdüğün düşmanlar patlıyor", "crit'te zaman yavaşlar")
11. **Evrimleşmiş Form**: Slot mutasyonu görsel olarak değişir/büyür (Pillar 1 bonus)

**Synergy Aktivasyonu:**
12. Synergy anında aktive edilir (mutasyon seçildikten sonra)
13. "SYNERGY!" banner'ı gösterilir + özel jingle + parıltı efekti
14. Synergy efekti run boyunca kalıcı (geri alınamaz)
15. Bir synergy birden fazla kez aktive edilemez

**Synergy Keşif Durumu:**
16. İlk kez tetiklenen synergy "keşfedilmiş" olarak işaretlenir (Unlock Tracker)
17. Keşfedilmiş synergy'ler bir koleksiyon ekranında görüntülenebilir (meta)
18. Keşfedilmemiş synergy'ler "???" olarak gösterilir (merak yaratır)

### Synergy Örnekleri (MVP — 5-10 synergy)

| Synergy Adı | Required Tags | Bonus Efekt | Tür |
|-------------|---------------|-------------|-----|
| Girdap Saldırısı | blade + speed | Etrafta dönen bıçak AoE saldırısı | Yeni Saldırı |
| Lazer Baraji | eye_laser + multi_shot | Lazerler 3 yöne aynı anda ateşlenir | Saldırı Upgrade |
| Vampir Kanatları | wings + lifesteal | Kill'lerde %5 HP iyileşme | Stat Boost |
| Zırh Kabuk | shell_back + armor_boost | Armor 2x, hareket %15 yavaş | Stat Boost (trade-off) |
| Patlayıcı Çarpma | charge_legs + aoe | Dash bitiminde AoE patlama | Özel Davranış |
| Ateş Ustası | fire + fire + fire | Tüm saldırılara fire DoT eklenir | Set Synergy (3x fire) |
| Mutant Lord | 4 farklı slot dolu | Tüm stat'lar +%15 | Set Synergy (full build) |

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Checking** | Mutasyon eklendi | Kontrol tamamlandı | Tüm SynergyDefinition'lar taranır |
| **Inactive** | Required tags karşılanmadı | — | Synergy bekleniyor |
| **Active** | Tüm required tags mevcut | Run sonu | Bonus efekt uygulanır, banner gösterilir |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Mutasyon Sistemi** | Upstream | Her mutasyon eklenmesinde `OnMutationEquipped` event'i dinlenir → synergy check tetiklenir |
| **Mutasyon Havuzu** | Upstream sorgu | `HasSynergyPotential(mutation, current_tags)` sorgusu — synergy bias için |
| **Auto-Attack** | Downstream | Synergy bonus saldırısı → yeni AttackModifier eklenir |
| **Player Controller** | Downstream | Synergy efekti hareket modifier'ı değiştirirse |
| **Entity Health** | Downstream | Synergy efekti HP/armor modifier'ı değiştirirse |
| **Sprite Compositing** | Downstream | Evrimleşmiş form synergy'si → slot sprite değişir |
| **Unlock Tracker** | Downstream | İlk keşif → unlock kaydı |
| **VFX / Juice** | Downstream | Synergy aktivasyon efekti |

## Formulas

### Synergy Check (Pseudocode)
```
function CheckSynergies(active_mutation_tags: Set<string>):
    for each synergy in synergy_definitions:
        if synergy.is_active: continue  // Zaten aktif
        if synergy.required_tags.isSubsetOf(active_mutation_tags):
            ActivateSynergy(synergy)
```

### Synergy Power Budget
```
synergy_power = sum(component_mutation_powers) * synergy_multiplier
// synergy_multiplier: ikili = 0.5, üçlü = 0.75, set = 1.0
// Synergy, parçalarının toplamından biraz daha zayıf (ama benzersiz efekt)
```

| Synergy Türü | Typical Power | Karşılaştırma |
|--------------|---------------|---------------|
| İkili stat boost | +%20-30 single stat | ~1 Uncommon mutasyon gücünde |
| İkili yeni saldırı | Orta güçlü bonus saldırı | ~1 Rare mutasyon gücünde |
| Üçlü | Güçlü stat veya unique efekt | ~1 Legendary mutasyon gücünde |
| Set (4 slot) | Broad buff | Tüm stat'lara +%15 |

### Synergy Probability Estimate (per run)
```
// 15 mutasyon / run, ~25 mutasyon havuzu, 10 synergy tanımlı
// Ortalama ikili synergy şansı (belirli bir synergy):
// P(A ve B) ≈ (15/25) * (14/24) ≈ 35%
// Synergy bias ile: ~45-50%

// Herhangi bir synergy'yi tetikleme: ~70-80% per run
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Aynı anda 2 synergy tetiklenir (tek mutasyonla) | Her ikisi de ayrı ayrı aktive edilir, 2 banner gösterilir (sırayla) | Çoklu synergy = mega-tatmin anı |
| Synergy tag'i pasif + slot'tan gelir | Geçerli — tag kaynağı önemsiz, sadece mevcut olması yeterli | Esneklik |
| Synergy definition'da bug: required_tag havuzda hiçbir mutasyonda yok | Synergy asla tetiklenemez — development-time validation ile yakala | ScriptableObject validate |
| 3-tag synergy'nin 2 tag'i var, 3. gelmeden run biter | Synergy aktive olmaz — partial synergy yok | Net kural: tümü veya hiçbiri |
| Synergy efekti çok güçlü (oyunu kırıyor) | Kabul edilebilir aralıkta — synergy'ler "güçlü hissettirmeli" ama run'ı bitirmemeli | Power budget limitleri |
| Synergy koleksiyonu: tümü keşfedilmiş | Tebrik mesajı + cosmetic ödül (meta) | Achiever tatmini |
| Synergy havuzu genişliyor (yeni mutasyonlar) | Yeni SynergyDefinition eklemek = yeni ScriptableObject. Mevcut synergy'ler bozulmaz | Data-driven genişleme |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Mutasyon Sistemi** | Upstream | Hard — mutasyon ekleme event'i + tag listesi |
| **Mutasyon Havuzu** | Downstream sorgu | Soft — synergy bias sorgusu |
| **Auto-Attack** | Downstream | Soft — bonus saldırı (yoksa sadece stat synergy'ler) |
| **Unlock Tracker** | Downstream | Soft — keşif kaydı |
| **VFX / Juice** | Downstream | Soft — synergy aktivasyon efekti |
| **Gameplay HUD** | Downstream | Soft — aktif synergy ikon listesi |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `synergy_count` (total defined) | 10 (MVP) | 5-30 | Daha fazla keşfedilecek | Az synergy, seyrek tetikleme |
| `synergy_multiplier_dual` | 0.5 | 0.3-0.8 | Güçlü ikili synergy | Zayıf ikili synergy |
| `synergy_multiplier_triple` | 0.75 | 0.5-1.0 | Güçlü üçlü | Zayıf üçlü |

> **Note:** `synergy_bias_multiplier` owned by mutation-pool.md.

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Synergy aktive | "SYNERGY!" banner (ekran ortası, 1.5sn), parlak flash | Synergy jingle (unique, memorable) | Critical / Critical |
| Synergy bonus saldırı | Yeni saldırı efekti (synergy'ye özel renk/şekil) | Yeni saldırı SFX | High / High |
| Evrimleşmiş form | Slot sprite morph (büyüme/değişim animasyonu) | "evolution" SFX | Critical / High |
| Synergy ipucu (seçim ekranı) | Kart altında küçük synergy ikonu | — | Medium / — |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Synergy banner | Ekran ortası (overlay) | Synergy tetiklendiğinde | Synergy aktive |
| Aktif synergy listesi | HUD kenarında (genişletilebilir) | Synergy eklendiğinde | En az 1 aktif synergy |
| Synergy koleksiyonu | Meta menü (ayrı ekran) | Session arası | Her zaman erişilebilir |
| Synergy ipucu (seçim kartında) | Kart altı küçük ikon | Kart sunumunda | Potansiyel synergy var |

## Acceptance Criteria

- [ ] Tag eşleşmesi ile synergy doğru tespit edilir
- [ ] İkili ve üçlü synergy'ler çalışır
- [ ] Synergy bonus efekti (stat, saldırı, davranış) doğru uygulanır
- [ ] Synergy banner gösterilir (görsel + ses)
- [ ] Aynı synergy ikinci kez tetiklenemez
- [ ] Tek mutasyonla birden fazla synergy tetiklenebilir
- [ ] Synergy keşfi Unlock Tracker'a kaydedilir
- [ ] Mutasyon Havuzu synergy bias sorgusu doğru çalışır
- [ ] **Performance:** Synergy check (tüm definition tarama) < 0.1ms
- [ ] Tüm synergy tanımları ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| MVP synergy sayısı: 5 mi 10 mu? | Game Designer | MVP | Hedef: 7-10 synergy (çeşitlilik + keşif motivasyonu) |
| Synergy keşif ipuçları ne kadar açık? | UX Designer | Vertical Slice | MVP: sadece seçim kartında küçük ikon. VS: codex/journal sistemi |
| Negatif synergy olacak mı? (trade-off: güçlü bonus + ceza) | Game Designer | Vertical Slice | MVP'de yok. VS'de 1-2 "risky synergy" denenebilir |
| Synergy görsel efektleri her biri unique mi, template mi? | Art Director | MVP | MVP: 2-3 template (stat boost = parıltı, saldırı = yeni sprite, form = morph) |
