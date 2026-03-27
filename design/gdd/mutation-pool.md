# Mutasyon Havuzu / Loot Table

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Synergy Keşfi, Kolay Giriş/Derin Derinlik

## Overview

Mutasyon Havuzu, her level-up'ta oyuncuya sunulacak 3 mutasyon kartının seçim algoritmasını yöneten sistemdir. Tüm mutasyonları rarity, slot türü ve oyuncunun mevcut durumuna göre filtreler ve ağırlıklandırır. Amacı: her seçim anlamlı hissettirmek, "dead card" sunmamak, synergy potansiyelini teşvik etmek ve nadir mutasyonların heyecan yaratmasını sağlamak.

## Player Fantasy

Oyuncu 3 kart gördüğünde "hepsi ilginç, hangisini seçsem?" hissetmeli. Asla "hiçbiri işe yaramaz" olmamalı. Nadir bir mutasyon gördüğünde "şanslıyım!" heyecanı. Synergy ile uyumlu bir kart gördüğünde "evet, beklediğim buydu!" tatmini.

## Detailed Design

### Core Rules

**Havuz Yapısı:**
1. Tüm mutasyonlar bir master havuzda tanımlanır (ScriptableObject listesi)
2. Her mutasyonun: id, category (slot/pasif), slot_type, rarity, synergy_tags
3. Level-up'ta havuz filtre ve ağırlıklama ile 3 kart üretir

**Filtreleme (Sunulmayacaklar):**
4. Zaten alınmış mutasyonlar havuzdan çıkar (tekrar yok)
5. Dolu slotlar için slot mutasyonları havuzdan çıkar
6. 4 slot doluysa TÜM slot mutasyonları çıkar (sadece pasif sunulur)
7. Unlock edilmemiş mutasyonlar havuzdan çıkar (meta-ilerleme ile açılanlar)

**Ağırlıklama (Rarity):**
8. Filtrelenen havuzdan rarity bazlı ağırlıklı rastgele seçim:

| Rarity | Base Weight | Açıklama |
|--------|-------------|----------|
| Common | 50 | Temel stat boost'lar |
| Uncommon | 30 | Orta güçlü efektler |
| Rare | 15 | Güçlü efektler |
| Legendary | 5 | Oyun değiştirici |

9. Ağırlıklar level'a göre kaydırılabilir (geç level'larda nadir mutasyon şansı artar)
```
effective_weight = base_weight * (1 + level * rarity_scaling)
// Legendary rarity_scaling: 0.05 (level 10'da: 5 * 1.5 = 7.5 ağırlık)
```

**Synergy Bias (Akıllı Sunma):**
10. Eğer oyuncunun mevcut mutasyonlarından biri ile synergy potansiyeli olan bir kart varsa, o kartın ağırlığı `synergy_bias_multiplier` (varsayılan: 1.5x) ile artırılır
11. Synergy bias garanti DEĞİL — sadece şansı artırır
12. Bu mekanik oyuncuya gösterilmez (gizli — keşif hissi korunmalı)

**Pity Timer:**
13. Oyuncu `pity_threshold` (varsayılan: 5) level-up boyunca Rare veya üzeri görmezse, bir sonraki seçimde en az 1 kart Rare+ garanti edilir
14. Legendary için ayrı pity: `legendary_pity` (varsayılan: 10 level-up)
15. Pity tetiklendiğinde ağırlıklar override edilir (1 kart zorunlu Rare/Legendary, diğer 2 normal)

**Kart Çeşitliliği Kuralları:**
16. 3 kart birbirinden farklı olmalı (aynı mutasyon 2 kez sunulmaz)
17. Mümkünse 3 kart farklı rarity'de olur (ama zorunlu değil — havuz küçükse tekrar olabilir)
18. Slot mutasyonu sunulacaksa, boş olan farklı slotlar öncelikli (çeşitlilik)

**Fallback (Havuz Tükenmesi):**
19. Havuzda 3'ten az mutasyon kaldıysa: eksik kartlar generic stat boost kartlarıyla doldurulur
20. Generic kartlar: "+%10 Damage", "+%10 Max HP", "+%10 Speed" (sınırsız, rarity yok)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Ready** | Run başlangıcı / Seçim tamamlandı | Level-up tetiklendi | Havuz güncel, seçim bekleniyor |
| **Generating** | Level-up tetiklendi | 3 kart üretildi | Filtre → ağırlıkla → seç |
| **Presenting** | 3 kart hazır | Oyuncu seçim yaptı | Kartlar Mutasyon Seçim UI'a gönderilir |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **XP & Level-Up** | Upstream | Level-up event → 3 kart üretme tetiklenir |
| **Mutasyon Sistemi** | İki yönlü | Mevcut mutasyon listesini okur (filtre + synergy bias). Seçilen mutasyonu Mutasyon Sistemine bildirir. |
| **Synergy Matrisi** | Upstream | Synergy tag eşleşme sorgusu (mevcut mutasyonlarla potansiyel synergy var mı?) |
| **Mutasyon Seçim UI** | Downstream | 3 kart verisini (MutationData + synergy ipucu flag) UI'a gönderir |
| **Meta-İlerleme** | Upstream | Unlock durumu — hangi mutasyonlar havuzda erişilebilir |

## Formulas

### Effective Weight
```
effective_weight = base_weight * (1 + level * rarity_scaling) * synergy_bonus
synergy_bonus = has_synergy_potential ? synergy_bias_multiplier : 1.0
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_weight | int | 5-50 | Rarity tablosu | Rarity bazlı temel ağırlık |
| level | int | 1-∞ | XP System | Oyuncu level'ı |
| rarity_scaling | float | 0.0-0.1 | Tuning knob, rarity bazlı | Level başına ağırlık artışı |
| synergy_bias_multiplier | float | 1.0-2.0 | Tuning knob | Synergy potansiyeli bonus çarpanı |

**Örnek (Level 10, Rare kart, synergy var):**
`15 * (1 + 10 * 0.03) * 1.5 = 15 * 1.3 * 1.5 = 29.25`

### Selection Probability
```
probability = effective_weight / sum(all_effective_weights)
```

### Pity Counter
```
if (levels_since_rare >= pity_threshold):
    force_one_rare_plus = true
    levels_since_rare = 0
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Havuzda 3'ten az mutasyon | Generic stat boost kartlarıyla doldur | Asla boş seçim yok |
| Havuzda 0 mutasyon (tümü alınmış) | 3 generic kart | Ultra-uzun run fallback |
| Tüm boş slotlar için slot mutasyonu sunulur ama oyuncu pasif istiyor | Rastgele — havuz karışık sunar. Slot/pasif oranı havuz kompozisyonuna bağlı | Zorlama yok, RNG |
| Synergy bias tüm kartları synergy kartı yapar | Bias sadece çarpan — düşük base weight'li kart yine düşük kalır | Bias ≠ garanti |
| Pity timer legendary için tetiklenir | 1 kart zorunlu Legendary, diğer 2 normal | Heyecan anı garantisi |
| Aynı level-up'ta pity + synergy bias | Her ikisi de uygulanır — pity kartı synergy bonus'u da alabilir | Stack eden mekanikler |
| Reroll mekanığı (gelecek feature?) | MVP'de yok. Varsa: reroll havuzu yeniden generate eder, alınan kartlar hariç | Provizönel: MVP'de yok |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **XP & Level-Up** | Upstream | Hard — level-up tetikleme |
| **Mutasyon Sistemi** | İki yönlü | Hard — mevcut mutasyon listesi + seçim bildirimi |
| **Synergy Matrisi** | Upstream | Soft — synergy tag sorgusu (yoksa bias çalışmaz, sorun değil) |
| **Mutasyon Seçim UI** | Downstream | Hard — 3 kart verisi |
| **Meta-İlerleme** | Upstream | Soft — unlock durumu (yoksa tüm mutasyonlar erişilebilir) |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `common_weight` | 50 | 40-60 | Daha çok sıradan | Nadirlere kayma |
| `uncommon_weight` | 30 | 20-40 | Daha çok uncommon | — |
| `rare_weight` | 15 | 10-25 | Daha sık rare | — |
| `legendary_weight` | 5 | 2-10 | Daha sık legendary | Çok nadir |
| `rarity_scaling` (per rarity) | C:0, U:0.01, R:0.03, L:0.05 | 0-0.1 | Geç level'da nadir artışı | Sabit rarity dağılımı |
| `synergy_bias_multiplier` | 1.5 | 1.0-2.5 | Synergy kartları daha sık | Synergy bias yok |
| `pity_threshold` (rare) | 5 | 3-8 | Daha sık pity | Daha nadir pity |
| `legendary_pity` | 10 | 7-15 | Daha sık legendary garanti | Daha nadir |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Kart sunumu | Kartlar sırayla ortaya çıkar (animasyonlu) | Kart çevirme SFX | High / Medium |
| Rare+ kart sunuldu | Kart çerçevesi parlar (mavi/altın) | Özel "rare!" pling | High / High |
| Legendary kart | Ekstra parlak, yıldız efekti | Epik jingle | High / High |
| Pity tetiklendi | Oyuncuya gösterilmez (gizli mekanik) | — | — |

## UI Requirements

Bu sistem Mutasyon Seçim UI'a veri sağlar — UI implementasyonu Mutasyon Seçim UI GDD'sinde.

| Information | Hedef | Condition |
|-------------|-------|-----------|
| 3 MutationData | Seçim UI'a gönderilir | Her level-up |
| Synergy ipucu flag | Kart altında "synergy mümkün" göstergesi | Synergy potansiyeli varsa |
| Rarity bilgisi | Kart çerçeve rengi | Her zaman |

## Acceptance Criteria

- [ ] Her level-up'ta 3 farklı mutasyon kartı sunulur
- [ ] Alınmış mutasyonlar tekrar sunulmaz
- [ ] Dolu slotlar için slot mutasyonu sunulmaz
- [ ] Rarity ağırlıklama doğru çalışır (1000 roll simülasyonu ile doğrula)
- [ ] Synergy bias uygulanır (synergy potansiyeli olan kartlar daha sık)
- [ ] Pity timer çalışır (N level-up sonra Rare+ garanti)
- [ ] Havuz tükendiğinde generic kartlar sunulur
- [ ] 4 slot doluyken sadece pasif sunulur
- [ ] **Performance:** Kart üretme < 0.5ms (filtre + ağırlıklama + random)
- [ ] Tüm ağırlıklar ve eşikler ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Reroll mekanığı eklenecek mi? (run başına 1-3 hak) | Game Designer | Vertical Slice | MVP'de yok, VS'de değerlendirilir |
| Level bazlı havuz genişlemesi (level 1'de sadece Common, level 5'ten sonra Rare açılır)? | Balans testi | MVP | Prototipte test — başlangıçta tüm rarity'ler açık |
| Slot mutasyonu vs pasif sunma oranı kontrol edilmeli mi? | Game Designer | MVP | Provizönel: Doğal havuz dağılımı, zorlama yok |
| Synergy ipucu ne kadar açık olsun? (tam synergy adı mı, sadece "synergy mümkün" işareti mi?) | UX Designer | MVP | Başlangıçta sadece ikon — keşif hissi Pillar 3 |
