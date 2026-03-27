# Sprite Compositing System

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Görsel Evrim Tatmini (birincil)

## Overview

Sprite Compositing System, oyuncunun sentetik varlığını modüler sprite katmanlarından oluşturan görsel render sistemidir. Base sprite (gövde) üzerine 4 slot (Kol, Bacak, Sırt, Baş) için ayrı sprite katmanları eklenir. Her katman bağımsız bir child SpriteRenderer olarak render edilir ve animasyon sırasında senkronize kalır. Oyuncu bu sistemi doğrudan kontrol etmez — Mutasyon Sistemi slot mutasyonu eklediğinde otomatik olarak görsel katman güncellenir. Bu sistem Pillar 1'in ("Görsel Evrim Tatmini") tek uygulama noktasıdır.

## Player Fantasy

Oyuncu her mutasyonda karakterinin fiziksel olarak değiştiğini görmeli. İlk run'da sade bir varlık, 4 slot dolduktan sonra tamamen farklı bir yaratık. "Bakın bu run'da ne oldum!" hissi — screenshot'a değer bir görsel kimlik. Dönüşüm yumuşak ve "büyüme" hissi vermeli, frankenstein etkisi yaratmamalı (renk paleti tutarlılığı kritik).

## Detailed Design

### Core Rules

**Sprite Mimarisi:**
1. Oyuncu karakteri bir parent GameObject altında 5+ child SpriteRenderer'dan oluşur:
   - `Base` — gövde sprite'ı (her zaman görünür)
   - `Slot_Arms` — kol mutasyonu sprite'ı (boşsa görünmez)
   - `Slot_Legs` — bacak mutasyonu sprite'ı (boşsa görünmez)
   - `Slot_Back` — sırt mutasyonu sprite'ı (boşsa görünmez)
   - `Slot_Head` — baş mutasyonu sprite'ı (boşsa görünmez)
2. Her child belirli bir sorting order'a sahip:
   - Back: -1 (gövdenin arkasında)
   - Base: 0
   - Legs: 1
   - Arms: 2
   - Head: 3

**Sprite Boyutu ve Çözünürlük:**
3. Base sprite: 16x16 piksel
4. Slot sprite'ları: 16x16 piksel (aynı boyut, pivot noktası slot'a göre ayarlanır)
5. Pixel-per-unit: 16 (1 birim = 16 piksel)
6. Filtering: Point (no filtering) — pixel art keskinliği

**Animasyon:**
7. Base sprite 4 yön animasyonuna sahip: Aşağı, Yukarı, Sol, Sağ
8. Her yön için 2-4 frame yürüme animasyonu
9. Slot sprite'ları aynı animasyon controller'ı paylaşır — base ile senkronize
10. Animasyon sync: tüm child SpriteRenderer'lar aynı Animator component'inden kontrol edilir (single Animator, multiple SpriteRenderer)
11. Her slot mutasyonunun 4 yön x N frame sprite sheet'i olmalı

**Pivot Noktaları (Anchor Points):**
12. Her slot'un base sprite üzerinde sabit bir pivot noktası var:
    - Arms: gövdenin yanları (x: ±4px, y: 0)
    - Legs: gövdenin altı (x: 0, y: -6px)
    - Back: gövdenin arkası (z-order ile, x: 0, y: +2px)
    - Head: gövdenin üstü (x: 0, y: +6px)
13. Pivot noktaları animasyon frame'lerine göre hafif kayabilir (animation curve ile)

**Mutasyon Ekleme (Runtime):**
14. Mutasyon seçildiğinde: Mutasyon Sistemi `OnMutationEquipped(SlotType, MutationData)` event'i yayınlar
15. Sprite Compositing event'i dinler ve ilgili slot child'ının sprite'ını günceller
16. Geçiş animasyonu: eski sprite fade-out (0.2sn) + yeni sprite fade-in (0.2sn) + parıltı efekti
17. İlk takma (boş slot): sadece fade-in + parıltı

**Renk Paleti Tutarlılığı:**
18. Tüm mutasyon sprite'ları aynı renk paletini kullanmalı (asset pipeline kuralı)
19. Palette: 8-16 renk limiti (retro estetik + tutarlılık)
20. Base sprite'ın ana rengi mutasyonlarla çakışmamalı — nötr ton (gri/beyaz/açık mavi)

**Düşman Sprite'ları:**
21. Düşmanlar modüler DEĞİL — tek sprite. Bu sistem sadece oyuncu karakteri için
22. Düşman sprite'ları ayrı, basit SpriteRenderer (compositing overhead yok)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Base Only** | Run başlangıcı | İlk slot mutasyonu seçildi | Sadece base sprite görünür |
| **Compositing** | En az 1 slot dolu | Tüm slotlar boşalır (olmaz — kalıcı) | Base + aktif slot sprite'ları render |
| **Transitioning** | Mutasyon eklendi | Fade animasyonu bitti (0.4sn) | Eski→yeni sprite geçişi |
| **Paused** | Pause/level-up | Resume | Animasyon dondurulur |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Mutasyon Sistemi** | Upstream | `OnMutationEquipped(SlotType, MutationData)` event'i dinler. MutationData içindeki `visual_prefab` referansını kullanarak sprite günceller |
| **Player Controller** | Upstream | Hareket yönünü okur → animasyon yönü belirlenir (4 yön) |
| **Camera System** | — | Doğrudan etkileşim yok, ama kamera zoom ile sprite boyutu algılanır |
| **VFX / Juice** | Downstream | Mutasyon ekleme anında parıltı efekti tetikler |

## Formulas

### Sorting Order
```
sorting_order = base_layer + slot_offset
```

| Slot | slot_offset | Açıklama |
|------|-------------|----------|
| Back | -1 | Gövdenin arkasında |
| Base | 0 | Referans katman |
| Legs | 1 | Gövdenin önünde (alt kısım) |
| Arms | 2 | Gövdenin önünde (yan kısım) |
| Head | 3 | En üst katman |

### Sprite Sheet Boyutu (per mutasyon)
```
total_frames = directions * frames_per_direction
sheet_size = total_frames * sprite_size

// Varsayılan:
// 4 yön * 4 frame = 16 frame
// 16 frame * 16x16 = 256x64 piksel sprite sheet (veya 64x64 4x4 grid)
```

### Memory Estimate
```
per_mutation_memory = total_frames * sprite_size^2 * 4bytes (RGBA)
// 16 frame * 16 * 16 * 4 = 16,384 bytes = 16 KB per mutasyon
// 12 slot mutasyonu * 16 KB = 192 KB toplam (ihmal edilebilir)

// Sprite Atlas ile daha da az (atlas packing)
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Tüm 4 slot dolu — çok karmaşık görünüm | Renk paleti tutarlılığı + sorting order doğru katman garantisi. Art direction sorumluluğu. | Pillar 1 gereği her mutasyon görünmeli |
| Animasyon sync kayması (slot sprite 1 frame geride) | Tek Animator → tüm sprite'lar aynı frame. Kayma teknik olarak imkansız. | Single Animator mimarisi bunu garanti eder |
| Slot sprite base sprite'tan taşar (büyük mutasyon) | Kabul edilebilir — bazı mutasyonlar base'den büyük olabilir (kanatlar, dev kol). Pivot noktası doğru olmalı. | Görsel çeşitlilik + güçlenme hissi |
| 16x16 çözünürlükte yeterli detay yok | Renk kullanımı ve silhouette ile ayrım. Her mutasyonun silhouette'i benzersiz olmalı. | Pixel art = silhouette + renk, detay değil |
| Düşmanlar oyuncuyla aynı sorting layer'da | Düşmanlar ayrı sorting layer'da — çakışma yok | Layer ayrımı |
| Pause sırasında animasyon | Animator time scale 0 → dondurulur | Tutarlı pause davranışı |
| Sprite atlas fragmentation (çok küçük sprite) | Tüm oyuncu sprite'ları tek atlas'ta toplanır | Sprite Atlas Analyzer (Unity 6.3) ile kontrol |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Mutasyon Sistemi** | Upstream | Hard — slot mutasyonu ekleme event'i sağlar |
| **Player Controller** | Upstream | Hard — hareket yönü (animasyon yönü belirleme) |
| **VFX / Juice** | Downstream | Soft — mutasyon ekleme efekti |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `sprite_size` | 16 | 16-32 | Daha detaylı sprite ama daha çok asset iş yükü | — (16 minimum) |
| `frames_per_direction` | 4 | 2-6 | Daha yumuşak animasyon | Daha kaba animasyon |
| `directions` | 4 | 4-8 | Daha çok yön (8-yön), 2x asset | 4 yön yeterli |
| `transition_fade_duration` | 0.2 | 0.1-0.5 | Yavaş geçiş (dramatic) | Hızlı geçiş (snappy) |
| `color_palette_size` | 12 | 8-16 | Daha zengin renk | Daha tutarlı, retro |
| `pixels_per_unit` | 16 | 16-32 | Dünya birimi başına daha çok piksel | — |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Slot mutasyonu ekleme | Fade-in + parıltı efekti (slot pozisyonunda) | "transform" SFX (Mutasyon Sistemi GDD'de tanımlı) | Critical / — (SFX başka sistemde) |
| Base animasyon (yürüme) | 4 yön x 4 frame = 16 frame yürüme | — (ayak sesi Player Controller'da) | High / — |
| Idle animasyon | 1-2 frame idle (nefes alma / hafif hareket) | — | Medium / — |

**Art Pipeline Kuralları:**
- Tüm sprite'lar aynı renk paletini kullanmalı
- Her mutasyon sprite'ının silhouette'i benzersiz olmalı (renksiz görünümde bile ayırt edilebilir)
- Pivot noktaları standart grid'e snap olmalı (piksel-perfect yerleştirme)
- Sprite sheet'ler 4x4 grid formatında (64x64 piksel per sheet)
- Alpha kenarları temiz olmalı (no anti-aliasing — pixel art)

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| — | — | — | Sprite Compositing'in UI çıktısı yok. Görsel feedback doğrudan game world'de. |

## Acceptance Criteria

- [ ] Base sprite 4 yönde animate olur
- [ ] Her slot için ayrı child SpriteRenderer doğru sorting order'da render olur
- [ ] Mutasyon ekleme event'i sprite katmanını günceller
- [ ] Tüm slot sprite'ları base ile senkronize animate olur (tek Animator)
- [ ] Geçiş animasyonu (fade-in/out) çalışır
- [ ] Sorting order: Back < Base < Legs < Arms < Head
- [ ] Boş slotlar görünmez (SpriteRenderer disabled)
- [ ] 4 slot dolu durumda karakter görsel olarak tutarlı (art direction test)
- [ ] Sprite Atlas kullanılıyor (draw call optimizasyonu)
- [ ] **Performance:** Compositing overhead < 0.1ms (sadece SpriteRenderer, hesaplama yok)
- [ ] **Memory:** Tüm oyuncu sprite'ları < 1MB
- [ ] Tüm sprite referansları data-driven (ScriptableObject'ten MutationData.visual_prefab)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| 4 yön mü 8 yön mü? | Art Director | MVP prototype | Başlangıçta 4 yön, playtest ile 8 yön değerlendirilir. 8 yön = 2x asset. |
| Sprite katmanlarında gölge/outline olacak mı? | Art Director | Vertical Slice | Provizönel: MVP'de yok. VS'de shader ile outline eklenebilir. |
| Base sprite formu başlangıç seçimine göre değişecek mi? | Game Designer | Vertical Slice | Provizönel: MVP'de tek base form. VS'de 2-3 alternatif base (meta-unlock). |
| Pasif mutasyonlar görsel olarak nasıl yansır? | Art Director | Vertical Slice | MVP'de yansımaz (sadece ikon). VS'de aura/parıltı rengi değerlendirilebilir. |
| Animasyon sprite vs bone-based animation? | Technical Artist | MVP prototype | Sprite sheet — bone animation 16x16'da gereksiz karmaşıklık. |
