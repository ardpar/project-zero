# Sprint 9 — 2026-05-25 to 2026-06-08

## Sprint Goal
İçerik genişletme: 4 yeni düşman tipi (4→8), 1 benzersiz fazlı boss, 1 yeni biome. Oyun dünyasını daha zengin ve çeşitli hissettir. Sprint sonunda: her run farklı düşman kombinasyonları sunan, taktik derinliği artan, yeni bir biome ile görsel çeşitlilik kazanmış build.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Milestone
- **Content Expansion** (Sprint 9-10)
- Sprint 1-3: MVP ✅
- Sprint 4-5: Vertical Slice ✅
- Sprint 6-7: Alpha ✅
- Sprint 8: Polish + Onboarding ✅
- Sprint 9: Content Expansion ← **Buradayız**

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S9-01 | **Tank Düşman** — ChaserBrain + yüksek HP (5x normal), yavaş speed (0.5x), büyük hitbox. Rogue Adventure sprite seç, EnemyData SO oluştur, animasyon wiring | 0.5 | — | Tank spawn olur, yavaş hareket eder, çok HP'si var, öldürülünce XP verir |
| S9-02 | **Charger Düşman** — Yeni ChargerBrain: idle durur → warning gösterir → oyuncuya doğru hızlı dash → cooldown → tekrar. Dash sırasında yüksek hasar | 1.0 | — | Charger periyodik dash yapar, warning görünür, dash sırasında hasar verir |
| S9-03 | **Summoner Düşman** — Yeni SummonerBrain: yavaş hareket → menzile girince durur → belirli aralıklarla küçük Chaser spawn eder (max 4). Öldürülünce yavruları da ölür | 1.5 | — | Summoner minion spawn eder, öldürülünce minionlar da ölür |
| S9-04 | **Poisoner Düşman** — Yeni PoisonerBrain: Chaser gibi hareket eder, arkasında zehirli iz bırakır (alan hasarı, 3sn sürer). Oyuncu iz üstünde durunca hasar alır | 1.0 | — | Poisoner yeşil iz bırakır, iz üstündeki oyuncu hasar alır, iz 3sn sonra kaybolur |
| S9-05 | **Wave Table güncelleme** — Yeni düşman tiplerini dalga havuzuna ekle. Dalga 3+: Tank, Dalga 4+: Charger, Dalga 5+: Summoner+Poisoner. Elite versiyonları tanımla | 0.5 | S9-01→04 | Yeni düşmanlar doğru dalgalarda spawn olur, zorluk eğrisi tutarlı |
| S9-06 | **Fazlı Boss: Cavern Guardian** — BossData.Phases kullanarak 3 fazlı boss. Faz 1: yavaş chase, Faz 2 (%50 HP): hızlanır + minion spawn, Faz 3 (%25 HP): dash saldırı + AoE slam. Rogue Adventure boss sprite | 1.5 | S9-02, S9-03 | Boss 3 fazda farklı davranır, faz geçişinde VFX oynar, yenilince victory tetiklenir |
| S9-07 | **Caverns Biome** — Rogue Adventure Caverns tileset'ini kullan, BiomeManager'a ekle, floor+wall tile SO'ları oluştur | 0.5 | — | Caverns biome rastgele seçilebilir, görsel tutarlı |

**Toplam Must Have: 6.5 gün** (kapasite: 8 gün — 1.5 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S9-08 | **Boss müzik stem'i** — Boss fazına özel yüksek intensity müzik, AdaptiveMusicConfig'e boss stem ekle | 0.25 | S9-06 | Boss fight'ta farklı müzik çalar |
| S9-09 | **Yeni düşmanlar için SFX** — Tank impact, Charger dash, Summoner spawn, Poisoner sizzle sesleri (procedural veya asset) | 0.5 | S9-01→04 | Her yeni düşmanın kendine özel sesi var |
| S9-10 | **Düşman çeşitlilik balans testi** — 10+ run playtest, yeni düşman dengesini doğrula, EnemyData değerlerini tune et | 0.5 | S9-05 | Zorluk eğrisi smooth, hiçbir düşman tipi OP veya işe yaramaz değil |
| S9-11 | **Caverns biome özel renk paleti** — Biome'a özel ortam rengi/tonu, kamera post-process tint | 0.25 | S9-07 | Caverns farklı bir atmosfer hissettiriyor |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S9-12 | **Boss intro animasyonu** — Boss spawn olurken kısa zoom + isim gösterimi (1-2sn cinematic) | 0.5 | S9-06 | Boss spawn anı dramatik hissettiriyor |
| S9-13 | **Düşman death VFX çeşitliliği** — Her düşman tipine özel ölüm efekti (Tank: büyük patlama, Poisoner: zehir bulutu) | 0.5 | S9-01→04 | Her düşman ölümü farklı hissettiriyor |
| S9-14 | **Mini-boss sistemi** — Dalga ortasında rastgele mini-boss (Elite'den güçlü, Boss'tan zayıf), özel loot | 1.0 | S9-06 | Mini-boss rastgele ortaya çıkar, öldürülünce bonus XP verir |

## Carryover from Sprint 8

| Task | Reason | New Estimate |
|------|--------|-------------|
| S8-07 Build & Playtest | Sprint 8'de Should/Nice Have'lere zaman harcandı | S9-10 ile birleştirildi |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Summoner'ın minion spawn'ı performans sorunu yaratabilir | Orta | 1 gün | Max 4 minion limiti, ObjectPool kullan |
| Poisoner'ın iz sistemi çok entity oluşturabilir | Düşük | 0.5 gün | Basit trigger collider + sprite, pooled |
| Boss faz geçişleri debug'ı zor olabilir | Orta | 0.5 gün | Her fazı ayrı test et, faz skip cheat ekle |
| Yeni düşman dengeleme çok zaman alabilir | Orta | 1 gün | İlk geçişte yaklaşık değerler, playtest ile iterate |
| Rogue Adventure sprite boyutları uyumsuz olabilir | Düşük | 0.25 gün | Import settings'te PPU ayarla |

## Dependencies on External Factors
- Rogue Adventure asset pack sprite'ları (✅ mevcut)
- Caverns tileset (✅ mevcut, `/Assets/ElvGames/Rogue Adventure/Tilesets/Caverns/`)
- Boss sprite'ları (✅ mevcut, `/Assets/ElvGames/Rogue Adventure/Bosses/`)

## Sprint 9 Sonunda Beklenen Durum
- **8 düşman tipi** — Chaser, Runner, Shooter, Exploder, Tank, Charger, Summoner, Poisoner
- **1 benzersiz fazlı boss** — Cavern Guardian (3 faz: chase → minion+hız → dash+AoE)
- **5 biome** — Base, Jungle, Temple, Hell, Caverns
- **Taktik derinlik** — Oyuncu farklı düşman kombinasyonlarına farklı stratejiler geliştirmeli
- **Dalga çeşitliliği** — Her dalga yeni bir tehdit katmanı ekliyor

## Definition of Done for this Sprint
- [ ] 4 yeni düşman tipi spawn olur ve doğru davranır
- [ ] ChargerBrain, SummonerBrain, PoisonerBrain yeni brain'ler çalışır
- [ ] Cavern Guardian boss 3 fazda farklı davranır
- [ ] Caverns biome görsel olarak farklı ve tutarlı
- [ ] Wave table güncellenmiş, yeni düşmanlar doğru dalgalarda
- [ ] 10+ playtest run sorunsuz tamamlanır
- [ ] 0 bilinen kritik bug
- [ ] Git'e commit
