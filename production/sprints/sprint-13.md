# Sprint 13 — 2026-07-24 to 2026-08-07

## Sprint Goal
Pasif Skill Tree, boss'lardan item drop, stat point dağıtım UI, loot sistemi. Sprint sonunda: oyuncular skill tree'den kalıcı bonuslar açıyor, boss'lar loot düşürüyor, stat point'leri dağıtabiliyor.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Roguelite ARPG Evolution** (Sprint 11-15)
- Sprint 11: Level/Stage + Gold ✅
- Sprint 12: WorldMap + Character + Save/Load + Items + Inventory ✅
- Sprint 13: Skill Tree + Loot + Stat Points ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S13-01 | **SkillTreeData SO** — 4 dal (Might/Vitality/Agility/Fortune), her dal 10 node. Node: id, isim, açıklama, stat bonus, maliyet (skill point), prerequisite node. ScriptableObject yapısı | 1.0 | — | 40 node tanımlı, Inspector'da düzenlenebilir |
| S13-02 | **SkillTreeManager** — Static class: node unlock, point harcama, SaveData entegrasyonu. `CharacterSaveData.unlockedSkillNodes` okuma/yazma. Tüm açılmış node stat'larını CombatStatBlock'a uygulama | 0.5 | S13-01 | Skill point harcanır, node unlock'lanır, stat uygulanır |
| S13-03 | **Skill Tree UI** — WorldMap'ten erişim. 4 dal görsel olarak ayrılmış, her node bir buton. Açık: renkli, kilitli: gri, prerequisite karşılanmamış: koyu. Tıklayınca unlock. Kalan point göstergesi | 2.0 | S13-01, S13-02 | Skill tree açılır, node'lar tıklanabilir, point harcanır |
| S13-04 | **Boss loot drop** — Boss öldürülünce rastgele item drop. Rarity: level'a göre scaling (Level 1-3: Rare garantili, Level 4-7: Epic %30, Level 8+: Legendary %10). Item otomatik envantere eklenir | 1.0 | — | Boss öldürünce item kazanılır, envanterde görünür |
| S13-05 | **Elite loot drop** — Elite düşman %30 şansla Uncommon/Rare item drop | 0.5 | S13-04 | Elite öldürünce şansla item düşer |
| S13-06 | **Stat Point dağıtım UI** — Karakter ekranında "+" butonları: STR/VIT/AGI/LCK/WIS. Unspent point varsa aktif, tıklayınca stat artar, save'e yazılır | 1.0 | — | Stat point dağıtılabilir, efektler run'da uygulanır |
| S13-07 | **Skill tree stat'ları run başında uygula** — GameBootstrap'ta tüm açılmış skill node stat bonuslarını CombatStatBlock'a ekle | 0.5 | S13-02 | Run başında skill tree bonusları aktif |

**Toplam Must Have: 6.5 gün** (kapasite: 8 gün — 1.5 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S13-08 | **Loot bildirim pop-up** — Item kazanıldığında gameplay HUD'da "Item Found: [name]" banner (2sn) | 0.5 | S13-04 | Loot kazanılınca bildirim görünür |
| S13-09 | **Level complete ekranında loot listesi** — Kazanılan itemlar listelenir | 0.25 | S13-04 | Level complete ekranında loot gösterilir |
| S13-10 | **Skill tree tooltip** — Node üzerine hover'da detaylı açıklama + stat bonus | 0.25 | S13-03 | Hover'da tooltip görünür |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S13-11 | **Skill tree unlock animasyonu** — Node açılınca glow + scale pulse efekti | 0.25 | S13-03 | Unlock tatmin edici |
| S13-12 | **Skill reset butonu** — Tüm skill'leri sıfırla (gold maliyeti), point'leri geri al | 0.5 | S13-02 | Reset çalışır, point'ler geri döner |
| S13-13 | **Normal düşman loot** — Normal düşmanlar %5 şansla Common item drop | 0.25 | S13-04 | Nadir ama düşebilir |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Skill tree UI karmaşık olabilir | Orta | 1 gün | Basit grid layout, node bağlantıları çizgi yerine renk ile |
| Loot çok fazla item doldurabilir | Düşük | 0.25 gün | Envanter cap (30 item), fazla satılabilir |
| Skill tree dengesi bozuk olabilir | Orta | 0.5 gün | Küçük artışlar, playtest ile ayarla |

## Dependencies on External Factors
- Mevcut ItemData/InventoryManager altyapısı (✅)
- CharacterSaveData.unlockedSkillNodes alanı (✅ zaten mevcut)
- CharacterSaveData.unspentStatPoints alanı (✅ zaten mevcut)

## Definition of Done for this Sprint
- [ ] 40 skill node tanımlı (4 dal x 10 node)
- [ ] Skill tree UI'dan node açılabilir
- [ ] Boss'lar rarity-scaled item drop eder
- [ ] Stat point'ler karakter ekranından dağıtılabilir
- [ ] Tüm stat'lar run başında uygulanır (class + upgrades + equipment + skills + stat points)
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
