# Sprint 12 — 2026-07-09 to 2026-07-23

## Sprint Goal
MainMenu yeniden tasarımı (New Game / Load Game), karakter yaratma (isim + sınıf seçimi), save sistemi genişletme, WorldMap scene temeli. Sprint sonunda: oyuncu karakter yaratıp kaydedebilir, dünya haritasını görebilir.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S12-01 | **CharacterSaveData** — İsim, sınıf, level, XP, stat points, equipped items, inventory, skill nodes, completed levels, gold. SaveManager genişletme, multi-save slot (3 slot) | 1.0 | — | Karakter verisi kaydedilip yüklenebilir |
| S12-02 | **ClassData SO** — 4 sınıf tanımı: Warrior, Rogue, Mage, Sentinel. Başlangıç stat modifiers, sınıf ismi, açıklama | 0.5 | — | 4 sınıf SO oluşturulmuş, Inspector'da düzenlenebilir |
| S12-03 | **MainMenu yeniden tasarım** — New Game / Load Game / Settings / Quit. Mevcut butonlar kaldırılır, yeni layout | 1.0 | — | MainMenu 4 butonlu temiz tasarım |
| S12-04 | **Character Creation ekranı** — İsim InputField + 4 sınıf kartı (seç) + Create butonu. Yeni scene veya MainMenu panel | 1.5 | S12-01, S12-02 | İsim girilebilir, sınıf seçilebilir, karakter oluşturulur |
| S12-05 | **Load Game ekranı** — 3 save slot gösterimi: slot boşsa "Empty", doluysa isim+sınıf+level. Slot seç → WorldMap'e geç | 1.0 | S12-01 | Save slotları gösterilir, seçim çalışır |
| S12-06 | **WorldMap scene oluştur** — Yeni Unity scene: WorldMap. Basit UI layout: level grid alanı, alt bar (Inventory/Skills/Save butonları), üst bar (karakter info) | 1.5 | S12-01 | WorldMap scene açılır, temel layout var |
| S12-07 | **Level grid (ilk 10 level)** — 10 level butonu grid'de gösterilir. Level 1 açık, diğerleri kilitli. Tıklayınca gameplay scene'e geç | 1.0 | S12-06 | Level butonları gösterilir, açık olana tıklayınca oyun başlar |

**Toplam Must Have: 7.5 gün**

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S12-08 | **Level complete → WorldMap dönüş** — Boss yenilince "Level Complete" → WorldMap scene'e geçiş, sonraki level unlock | 0.5 | S12-07 | Level tamamlayınca haritaya dönülür, sonraki level açılır |
| S12-09 | **Karakter info bar** — WorldMap üstünde: isim, sınıf ikonu, level, XP barı, gold | 0.25 | S12-06 | Karakter bilgisi görünür |
| S12-10 | **Save & Quit butonu** — WorldMap'te kaydet ve MainMenu'ye dön | 0.25 | S12-06 | Save çalışır, MainMenu'ye döner |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S12-11 | **Sınıf seçim animasyonu** — Kart seçilince glow/scale efekti | 0.25 | S12-04 | Sınıf seçimi tatmin edici |
| S12-12 | **Level grid zorluk yıldızları** — Her level'da ★-★★★★★ zorluk göstergesi | 0.25 | S12-07 | Zorluk seviyesi görünür |

## Definition of Done for this Sprint
- [ ] New Game → karakter yaratma → WorldMap akışı çalışır
- [ ] Load Game → save slot seç → WorldMap akışı çalışır
- [ ] 4 sınıf seçilebilir, stat'lar doğru uygulanır
- [ ] WorldMap'te level grid gösterilir, Level 1 tıklanabilir
- [ ] Level tamamlayınca WorldMap'e dönülür
- [ ] Save/Load 3 slot çalışır
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
