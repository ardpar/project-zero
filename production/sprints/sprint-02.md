# Sprint 2 — 2026-04-11 to 2026-04-25

## Sprint Goal
Mutasyon sistemi, mutasyon seçim UI'ı, tam dalga tablosu ve synergy matrix'i implement et. Sprint sonunda: düşman öldürünce XP → level-up → 3 kart seçimi → mutasyon uygulanır → sprite değişir → synergy tetiklenir.

## Capacity
- Total days: 10 (2 hafta, solo, full-time)
- Buffer (20%): 2 gün (unplanned work, bug fixing)
- Available: 8 gün

## Milestone
- **MVP** (4-6 hafta, Sprint 1-3)
- Sprint 1: Foundation + Core ✅
- Sprint 2: Feature Layer (mutasyonlar, synergy, dalgalar) ← **Buradayız**
- Sprint 3: Presentation + Polish (VFX, ses, juice, balans)

## Tasks

### Must Have (Critical Path)

| ID | Task | Est. Days | Dependencies | GDD | Acceptance Criteria |
|----|------|-----------|-------------|-----|-------------------|
| S2-01 | **Projectile collision fix** — Sprint 1 carryover: projectile trigger detection intermittent, Rigidbody2D/layer sorunları çöz | 0.5 | — | projectile-damage-system.md | Projectile düşmana isabet eder, hasar verir, pool'a döner, 60fps korunur |
| S2-02 | **MutationData + MutationDatabase** — MutationData SO (slot/passive, rarity, tags, stat modifiers), MutationDatabase SO (master list), 12-15 mutasyon tanımı | 1.0 | S2-01 | mutation-system.md | SO'lar oluşturulur, Inspector'dan düzenlenebilir, 12+ mutasyon tanımlı |
| S2-03 | **MutationManager** — Aktif mutasyonları yönet, ApplyMutation/RemoveMutation, CombatStatBlock güncelle, slot sistemi (Arms/Legs/Back/Head + passives) | 1.0 | S2-02 | mutation-system.md | Mutasyon uygulanır → stat değişir, slot dolu ise yeni slot mutasyonu teklif edilmez |
| S2-04 | **MutationPool** — Level-up'da 3 kart seçimi için weighted random pool, rarity scaling, synergy bias (1.5x), pity timer, duplicate filter | 1.0 | S2-02, S2-03 | mutation-pool.md | 3 farklı kart sunulur, duplicate yok, rarity dağılımı GDD'ye uygun, pity timer çalışır |
| S2-05 | **MutationSelectionUI** — Level-up'da oyun durur, 3 kart gösterilir, oyuncu seçer, mutasyon uygulanır, oyun devam eder | 1.0 | S2-03, S2-04 | mutation-selection-ui.md | Time.timeScale=0, 3 kart görünür, seçim sonrası mutasyon uygulanır, oyun devam eder |
| S2-06 | **Tam Wave Tablosu (6 dalga + boss)** — Sprint 1'deki 3 dalga → 6 dalga, elite spawn, boss phase, WaveTableData güncelle | 1.0 | S2-01 | wave-spawning-system.md | 6 dalga doğru sırayla spawn, elite'ler %80'de çıkar, boss Wave 6 sonrası gelir |
| S2-07 | **Synergy Matrix** — Tag-based synergy detection, 7-10 synergy tanımı, otomatik aktivasyon, stat/davranış bonusları | 1.5 | S2-03 | synergy-matrix.md | Uygun mutasyonlar edinilince synergy otomatik aktive olur, bonus uygulanır |

**Toplam Must Have: 7.0 gün** (kapasite: 8 gün — 1 gün buffer)

### Should Have

| ID | Task | Est. Days | Dependencies | GDD | Acceptance Criteria |
|----|------|-----------|-------------|-----|-------------------|
| S2-08 | **Sprite Compositing (temel)** — Parent+child SpriteRenderer yapısı, slot equip'te sprite değişimi, sorting order | 1.0 | S2-03 | sprite-compositing.md | Mutasyon alınca oyuncu sprite'ı değişir (placeholder sprite), katmanlı render |
| S2-09 | **Boss Brain** — BossBrain'i gerçek boss davranışına yükselt (yavaş chase, yüksek HP, phase transition altyapısı) | 0.5 | S2-06 | wave-spawning-system.md | Boss spawn olur, yüksek HP ile chase yapar, ölünce "run won" event tetiklenir |
| S2-10 | **Enemy çeşitliliği** — Runner (hızlı, düşük HP) + Exploder (yaklaşıp patlama) enemy data + brain'leri aktifleştir | 0.5 | S2-06 | enemy-ai-system.md | Runner ve Exploder WaveTable'da spawn olur, farklı davranış gösterir |

### Nice to Have

| ID | Task | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-----------|-------------|-------------------|
| S2-11 | **Synergy aktivasyon UI** — "SYNERGY!" banner + kısa animasyon | 0.25 | S2-07 | Synergy aktive olunca ekranda banner görünür |
| S2-12 | **Mutasyon kart animasyonu** — Kartlar slide-in + glow efekti | 0.25 | S2-05 | Kartlar animasyonlu açılır |
| S2-13 | **Dash trail efekti** — Dash sırasında basit afterimage | 0.25 | — | Dash sırasında iz bırakır |

## Carryover from Previous Sprint

| Task | Reason | New Estimate |
|------|--------|-------------|
| Projectile collision fix | Projectile trigger detection intermittent — Rigidbody2D eklendi ama tam çözülmedi | 0.5 gün (S2-01) |

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Mutasyon sistemi karmaşıklığı beklenenden fazla | Orta | 1 gün kayıp | GDD çok detaylı, edge case'ler tanımlı — doğrudan implement et |
| Synergy matrix kombinatorik patlama | Düşük | 0.5 gün | MVP'de 7-10 synergy, tag-based basit matching yeterli |
| Sprite compositing pixel alignment sorunları | Orta | 1 gün | Placeholder renkli kareler kullan, gerçek sprite'lar Sprint 3'te |
| MutationSelectionUI Time.timeScale=0 ile input sorunları | Düşük | 0.5 gün | UI action map ayrı, Player map disable edilir |

## Dependencies on External Factors
- Sprint 1 projectile collision fix'i Sprint 2 başında çözülmeli (S2-01)
- Placeholder sprite'lar (renkli kareler) yeterli — gerçek pixel art gerekmez
- Mutasyon ikonları/kartları placeholder olarak renkli kutular + text

## Collision Layer Matrix (Güncelleme)
Sprint 1'deki matrix geçerli, ek layer gerekmez.

## Sprint 2 Sonunda Beklenen Durum
- Oyuncu level atlayınca 3 mutasyon kartından birini seçer
- Mutasyon stat'ları etkiler (hız, hasar, crit, vb.)
- Slot mutasyonları sprite'ı değiştirir (placeholder)
- Uyumlu mutasyonlar synergy tetikler → ekstra bonus
- 6 dalga + boss fight tamamlanabilir
- Runner ve Exploder düşmanlar var
- ~15 dakikalık tam bir run oynanabilir

## Definition of Done for this Sprint
- [ ] Tüm Must Have task'lar tamamlandı
- [ ] Tüm task'lar acceptance criteria'yı karşılıyor
- [ ] Hiçbir S1/S2 bug yok
- [ ] Projectile collision düzgün çalışıyor
- [ ] 12+ mutasyon tanımlı ve seçilebilir
- [ ] 7+ synergy tanımlı ve tetiklenebilir
- [ ] 6 dalga + boss fight oynanabilir
- [ ] 150 düşman ekranda 60fps korunuyor
- [ ] Git'e commit edilmiş

## Next Sprints (Yol Haritası)
- **Sprint 3** (Apr 25-May 9): HUD polish, VFX/Juice (screen shake, particles, damage numbers), SFX, playtest + balans, run manager (win/lose)
