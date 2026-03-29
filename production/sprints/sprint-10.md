# Sprint 10 — 2026-06-09 to 2026-06-23

## Sprint Goal
Meta-progression derinliği: kalıcı stat upgrade'ler, başlangıç formu seçimi, achievement sistemi. Run'lar arası kalıcı büyüme hissi yaratarak uzun vadeli motivasyonu artır. Sprint sonunda: oyuncular her run'da biraz daha güçlü başlıyor, farklı başlangıç formları deniyor, achievement'ları kovalıyor.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Content Expansion + Meta Depth** (Sprint 9-10)
- Sprint 1-3: MVP ✅
- Sprint 4-5: Vertical Slice ✅
- Sprint 6-7: Alpha ✅
- Sprint 8: Polish + Onboarding ✅
- Sprint 9: Content Expansion ✅
- Sprint 10: Meta-Progression ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S10-01 | **Kalıcı Stat Upgrade Sistemi** — `StatUpgradeConfig` SO: 5 upgrade tipi (Max HP, Move Speed, XP Gain, Crit Chance, Starting Armor). Her biri 5 kademe, artan cell maliyeti. `UpgradeManager` static class: uygula/kaydet/yükle. SaveData'ya `upgradelevels` dict ekle | 1.5 | — | Cell harcayarak kalıcı stat artışı alınabilir, run başında uygulanır |
| S10-02 | **Upgrade Shop UI** — Main Menu'de yeni "UPGRADES" paneli. 5 upgrade satırı: isim, mevcut kademe (★★★☆☆), maliyet, satın al butonu. Tutarlı panel tasarımı (full-screen, pixel font, BACK butonu) | 1.0 | S10-01 | Upgrade paneli açılır, kademe görünür, satın alma çalışır |
| S10-03 | **Başlangıç Formu Seçimi** — 3 başlangıç formu: Balanced (varsayılan statlar), Berserker (+30% DMG, -20% HP), Speedster (+25% Speed, -15% DMG). `StarterFormData` SO, form seçim UI, run başında stat uygulaması | 1.5 | S10-01 | 3 form arasında seçim yapılabilir, seçilen formun statları run başında uygulanır |
| S10-04 | **Form Seçim UI** — Play butonuna basınca form seçim ekranı: 3 kart (isim, stat bonusları, görsel), seç ve başla. Son seçilen form hatırlanır | 0.5 | S10-03 | Form seçim ekranı gösterilir, kart seçimi çalışır, oyun başlar |
| S10-05 | **Achievement Sistemi** — `AchievementData` SO + `AchievementManager` static tracker. 10 achievement: İlk Win, 100 Kill, 500 Kill, Wave 6 Ulaş, 5 Run Tamamla, 10 Mutasyon Topla (tek run), Boss'u 60s Altında Yen, Tüm Mutasyonları Keşfet, 3 Synergy Aktif (tek run), 1000 XP Topla (tek run) | 1.5 | — | Achievement'lar unlock'lanır, SaveData'da kalıcı |
| S10-06 | **Achievement UI** — Main Menu'de "ACHIEVEMENTS" paneli. Achievement listesi: isim, açıklama, kilit/açık durumu, ilerleme çubuğu (varsa). Tutarlı panel tasarımı | 1.0 | S10-05 | Achievement paneli açılır, unlock durumları doğru gösterilir |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1.0 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S10-07 | **Upgrade stat'ları run başında uygula** — `GameBootstrap` veya `RunManager`'da SaveData'dan upgrade seviyelerini oku, `CombatStatBlock`'a uygula, DailySeed'den sonra | 0.25 | S10-01 | Upgrade seviyesi run istatistiklerinde yansır |
| S10-08 | **Achievement bildirim pop-up** — Oyun içi achievement unlock'landığında kısa banner göster (isim + "Achievement Unlocked!"), 2sn fade | 0.5 | S10-05 | Achievement kazanılınca ekranda bildirim görünür |
| S10-09 | **Cell economy balans** — Upgrade maliyetlerini playtest ile doğrula: ilk upgrade ucuz (5 cell), son kademeler pahalı (100+ cell). Upgrade economy dokümantasyonu | 0.5 | S10-01, S10-02 | Maliyet eğrisi smooth, ne çok kolay ne imkansız |
| S10-10 | **Başlangıç formları unlock sistemi** — Berserker ve Speedster başta kilitli, achievement ile açılır (ör: İlk Win → Berserker, Wave 6 → Speedster) | 0.25 | S10-03, S10-05 | Kilitli formlar achievement'la açılır |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S10-11 | **Stat upgrade görsel feedback** — Upgrade alınca kısa particle/glow efekti, kademe yıldızları animasyonlu | 0.5 | S10-02 | Upgrade satın alma tatmin edici hissettiriyor |
| S10-12 | **Run özeti genişletme** — Ölüm/zafer ekranında unlock edilen achievement'lar gösterilsin | 0.5 | S10-05, S10-08 | Run sonunda kazanılan achievement'lar listelenir |
| S10-13 | **Total stats ekranı** — Main Menu'de toplam istatistikler: total kills, total runs, total playtime, en iyi wave, en iyi survival time | 0.5 | — | İstatistik paneli gösterilir |

## Carryover from Sprint 9

| Task | Reason | New Estimate |
|------|--------|-------------|
| S9-10 Balans testi | Playtest gerekli, sprint 9'da code-only yapıldı | S10-09 ile birleştirildi |
| S9-14 Mini-boss sistemi | Nice to Have, atlandı | Sprint 11'e ertelendi |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Stat upgrade'ler oyunu çok kolaylaştırabilir | Orta | 1 gün | Küçük artışlar (%2-5 per kademe), playtest ile doğrula |
| Achievement tracking her event'te çalışıyor — performans | Düşük | 0.25 gün | Basit counter check, her frame değil sadece event'lerde |
| Form seçim UI akışı karmaşık olabilir | Düşük | 0.5 gün | Basit 3 kart layout, Play butonu forma bağlı |
| SaveData uyumluluk sorunu (yeni field'lar) | Düşük | 0.25 gün | Default değerler ile geriye uyumlu JSON deserialization |

## Dependencies on External Factors
- Mevcut cell economy ve UnlockManager altyapısı (✅ mevcut)
- CombatStatBlock stat uygulama sistemi (✅ mevcut)
- Pixel font ve UI button sprite'ları (✅ mevcut)

## Sprint 10 Sonunda Beklenen Durum
- **5 kalıcı stat upgrade** — Max HP, Speed, XP Gain, Crit Chance, Armor (her biri 5 kademe)
- **3 başlangıç formu** — Balanced, Berserker, Speedster
- **10 achievement** — Çeşitli oyun hedefleri, kalıcı izleme
- **Upgrade Shop + Achievement paneli** — Main Menu'den erişim
- **Uzun vadeli motivasyon** — Her run biraz daha güçlü başlama + hedef kovalama

## Definition of Done for this Sprint
- [ ] 5 stat upgrade cell ile satın alınabilir, run başında uygulanır
- [ ] Upgrade Shop UI tutarlı tasarımda çalışır
- [ ] 3 başlangıç formu seçilebilir, statlar doğru uygulanır
- [ ] 10 achievement tanımlı, unlock mekanizması çalışır
- [ ] Achievement UI paneli gösterilir
- [ ] Yeni SaveData field'ları geriye uyumlu
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
