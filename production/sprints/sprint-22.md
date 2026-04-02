# Sprint 22 — 2026-06-10 to 2026-06-24

## Sprint Goal
Alpha Polish milestone kapatma: runtime playtest + build profiling (kesin tamamlanacak), bug fix, final polish, Alpha gate check. Sprint sonunda: tüm Alpha Polish deliverable'ları tamamlanmış, performans ölçülmüş, milestone PASS ile kapatılmış.

## Milestone
- **Alpha Polish** (Sprint 19-22)
- Sprint 19: Runtime Playtest + Audio + Lore v1 + Build Profiling ✅
- Sprint 20: Tutorial/Onboarding + UI Polish + Balance Tuning ✅
- Sprint 21: Content Expansion (yeni düşmanlar, mutasyonlar, synergy'ler) ✅
- Sprint 22: Alpha Gate Check + Bug Fix + Final Polish ← **Buradayız**

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün
- Available: 8 gün

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S22-01 | **Runtime playtest (10 run)** — Development Build, 10 tam run (oda 1→ölüm). Yeni content dahil (3 düşman, synergy'ler, lore drop). Playtest raporu: ölüm odaları, zorluk hissi, ekonomi, synergy keşif oranı, tutorial etkinliği, lore pacing. `tests/balance/playtest-report-s22.md` | 2.0 | — | 10 run tamamlanmış, rapor yazılmış, tespit edilen sorunlar listelenmiş |
| S22-02 | **Standalone build profiling** — Development Build + Unity Profiler. 4 senaryo: wave 1 (az düşman), wave 5 (çok düşman), boss fight, 200 düşman stress test. Frame-time budget tablosu doldur. Memory ceiling belirle. Draw call analizi (Frame Debugger). `technical-preferences.md` güncelle | 1.5 | S22-01 | Frame budget gerçek verilerle dolu, memory ceiling belirlenmiş, draw call sayısı belgelenmiş |
| S22-03 | **Bug fix pass** — Playtest'ten çıkan bug'ları triyajla ve düzelt. S1 (crash/data loss) ve S2 (broken feature) öncelikli. S3+ bug'lar belgelenip deferlanabilir | 1.5 | S22-01 | 0 S1/S2 bug, S3+ bug'lar belgelenmiş |
| S22-04 | **Final polish** — Playtest'ten çıkan UX sorunları: tutarsız UI elemanları, eksik feedback, ölü butonlar, scaling sorunları. Synergy banner görsel iyileştirmesi. Lore popup zamanlama ayarı. Boss odası müzik geçişi polish | 1.5 | S22-01 | Playtest notlarındaki UX sorunları çözülmüş |
| S22-05 | **Alpha gate check** — Tüm Sprint 19-22 deliverable'ları gözden geçirme. Feature checklist. Quality metrics (test sayısı, perf budget, bug count). Architecture health. Next milestone adayları. `production/milestones/milestone-3-gate-check.md` | 1.0 | S22-01, S22-02, S22-03 | Gate check raporu yazılmış, PASS/FAIL verdikti verilmiş |

**Toplam Must Have: 7.5 gün** (kapasite: 8 gün — 0.5 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S22-06 | **Unit test genişletme** — Synergy detection, tutorial step progression, lore drop logic, yeni düşman brain testleri. Hedef: 60+ toplam test | 0.5 | — | 60+ test geçiyor |
| S22-07 | **Gerçek müzik asset'leri** — Placeholder procedural stem'ler yerine gerçek .wav/.ogg biome müzikleri (ya da yüksek kaliteli procedural). 6 biome calm + combat stem | 1.0 | — | BiomeConfig'lerde gerçek AudioClip'ler atanmış |
| S22-08 | **Node isimleri Arena terminolojisine** — Skill tree 40 node hala İngilizce isimli (Sharp Edge, Heavy Strike...). Arena terminolojisine çevir (Substrate Resonance, Impact Calibration...) | 0.5 | — | 40 node Arena terminolojisiyle |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S22-09 | **Run özet ekranı** — Run bittiğinde (ölüm): toplam kill, en yüksek oda, toplanan lore, aktif synergy'ler, adaptasyon dağılımı. Motivasyon: "bir dahaki sefere daha ileri" | 0.5 | — | Ölüm sonrası run özeti gösteriliyor |
| S22-10 | **Accessibility: renk körlüğü modu** — Rarity renkleri + biome renkleri alternatif paletler (protanopia, deuteranopia). Settings'ten toggle | 0.5 | — | Renk körlüğü modu çalışıyor |
| S22-11 | **Perf report güncelleme** — Sprint 17 perf report'u güncellenmiş haliyle, tüm fix'lerin doğrulanmış statüsü | 0.25 | S22-02 | Güncel perf report |

## Carryover from Sprint 21

| Task | Reason | New Estimate |
|------|--------|-------------|
| S21-05 Runtime playtest | Development Build gerekli — son kez carryover | S22-01 olarak 2.0 gün |
| S21-06 Standalone build profiling | Development Build gerekli — son kez carryover | S22-02 olarak 1.5 gün |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Playtest'te çıkan bug sayısı beklenenden fazla olabilir | Orta | 1 gün | S1/S2 öncelikli, S3+ defer. Buffer 0.5 gün mevcut |
| Standalone build oluşturma süresi uzun olabilir | Düşük | 0.5 gün | İlk build sırasında profiling planını hazırla |
| Gate check'te beklenmeyen blocker | Düşük | 0.5 gün | Gate check esnek — "PASS with follow-ups" kabul edilebilir |
| Yeni content (S21) balance sorunları playtest'te ortaya çıkabilir | Orta | 0.5 gün | SO değerleri hızlıca ayarlanabilir, code change gerekmez |

## Dependencies on External Factors
- Sprint 21 tamamlanmış (yeni düşmanlar, mutasyonlar, synergy'ler)
- Unity Development Build capability
- Unity Profiler bağlantısı
- Tüm önceki sprint'lerin commit'leri (S17-S21)

## Definition of Done for this Sprint
- [ ] 10 tam run playtest tamamlanmış ve raporlanmış
- [ ] Frame budget tablosu gerçek verilerle doldurulmuş
- [ ] Memory ceiling belirlenmiş ve technical-preferences.md güncel
- [ ] 0 S1/S2 bug
- [ ] Alpha gate check raporu yazılmış
- [ ] Milestone 3 (Alpha Polish) PASS verdikti
- [ ] Git'e commit

## Milestone 3 Kapatma Kontrol Listesi
- [ ] Tutorial çalışıyor ve yeni oyuncu rehberli
- [ ] Audio: biome müzikleri + boss geçişi + SFX
- [ ] Lore: 20 fragment toplanabiliyor, Signal Archive çalışıyor
- [ ] Content: 11 düşman tipi, 67 mutasyon, 8 synergy
- [ ] Balance: item stat'lar GDD'ye uygun, skill tree logaritmik, ekonomi sürdürülebilir
- [ ] UI: TMP, popup animasyonları, rarity renkleri tutarlı
- [ ] Performans: 60fps hedef ölçülmüş
- [ ] 41+ unit test geçiyor
- [ ] ADR'ler güncel (003, 004)
- [ ] Sprint planları tamamlanmış (S19-S22)
