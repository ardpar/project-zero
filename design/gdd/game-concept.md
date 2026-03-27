# Game Concept: SYNTHBORN

*Created: 2026-03-27*
*Status: Draft*

---

## Elevator Pitch

> Dijital bir arenada sonsuz yaratık sürülerine karşı hayatta kal — her level-up'ta vücuduna modüler mutasyonlar ekle, görsel olarak evrimleş ve çılgın synergy'ler keşfet. 10 dakikalık run'larda "bir tane daha" bağımlılığı yaratan bir roguelite survivor.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | Roguelite Survivor (top-down auto-battler) |
| **Platform** | PC (Steam) |
| **Target Audience** | Survivor ve roguelite seven mid-core oyuncular (18-35) |
| **Player Count** | Single-player |
| **Session Length** | 10-15 dakika (run), 30-60 dakika (oturum) |
| **Monetization** | Premium (Steam) |
| **Estimated Scope** | Medium (3-6 ay, solo geliştirici) |
| **Comparable Titles** | Vampire Survivors, Brotato, The Binding of Isaac |

---

## Core Fantasy

Bir dijital arenada doğan sentetik bir varlıksın. Her düşmanı yok ettiğinde güçleniyorsun — ama sadece stat olarak değil, fiziksel olarak. Kolların bıçağa dönüşüyor, sırtından kanatlar çıkıyor, gözlerin lazer fırlatıyor. Run'ın sonunda başladığın yaratıkla hiçbir benzerliğin kalmıyor.

Oyuncunun vaadi: **"Her run'da farklı bir canavara dönüşeceğim ve bu dönüşümü kendi ellerimle şekillendireceğim."**

---

## Unique Hook

Vampire Survivors tarzı survivor, **AND ALSO** her mutasyon karakterinin vücudunda görsel olarak yansıyor — kol, bacak, sırt, baş slotlarına eklenen parçalar hem yetenek hem görünüm değiştiriyor. Build'ini sadece hissetmiyorsun, **görüyorsun**. İki mutasyonu birleştirince beklenmedik evrimler ortaya çıkıyor.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Sensation** (sensory pleasure) | 2 | Ekran dolduran düşmanlar, patlama efektleri, screen shake, satisfying pixel art |
| **Fantasy** (make-believe, role-playing) | 3 | "Evrimleşen sentetik yaratık" fantezisi, her run farklı bir form |
| **Narrative** (drama, story arc) | 6 | Run'lar arası damla damla akan lore, arena boss'larının hikayeleri |
| **Challenge** (obstacle course, mastery) | 1 | Dalga zorluk eğrisi, boss mekanikleri, pozisyonlama becerisi |
| **Fellowship** (social connection) | N/A | Single-player, sosyal etkileşim yok |
| **Discovery** (exploration, secrets) | 4 | Gizli synergy'ler, nadir mutasyonlar, gizli boss'lar |
| **Expression** (self-expression, creativity) | 2 | Modüler build çeşitliliği, görsel karakter kimliği |
| **Submission** (relaxation, comfort zone) | 5 | Otomatik saldırı sistemi, kolay giriş |

### Key Dynamics (Emergent player behaviors)

- Oyuncular mutasyon kombinasyonlarını deneyerek gizli synergy'leri keşfedecek
- "Bu sefer tamamen bacak build'i deneyeceğim" gibi tematik run'lar planlayacaklar
- Screenshot paylaşımı: "Bakın bu run'da ne oldum!" — organik viral potansiyel
- Leaderboard rekabeti: en hızlı boss kill, en yüksek dalga

### Core Mechanics (Systems we build)

1. **Survivor Hareket + Otomatik Saldırı** — Top-down hareket, saldırılar otomatik tetiklenir
2. **Modüler Mutasyon Sistemi** — Vücut slotlarına (kol, bacak, sırt, baş) mutasyonlar eklenir, her biri görsel + mekanik etki
3. **Synergy Matrisi** — Belirli mutasyon kombinasyonları evrimleşmiş formlar üretir (A + B = C)
4. **Dalga Sistemi** — Zaman bazlı düşman dalgaları, artan zorluk, dalga sonu elite/boss
5. **Meta-İlerleme** — Kalıcı unlock'lar, stat yükseltme, hikaye açılımı

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** (freedom, meaningful choice) | Her level-up'ta mutasyon seçimi, build yönlendirme özgürlüğü | Core |
| **Competence** (mastery, skill growth) | Pozisyonlama becerisi, synergy bilgisi, artan zorluk | Core |
| **Relatedness** (connection, belonging) | Leaderboard rekabeti, screenshot paylaşımı, topluluk synergy keşfi | Supporting |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** (goal completion, collection, progression) — Mutasyon koleksiyonu, tüm synergy'leri açma, meta-ilerleme tamamlama
- [x] **Explorers** (discovery, understanding systems, finding secrets) — Gizli synergy keşfi, nadir mutasyonlar, gizli boss'lar
- [ ] **Socializers** (relationships, cooperation, community) — Minimal (leaderboard ve screenshot paylaşımı)
- [x] **Killers/Competitors** (domination, PvP, leaderboards) — Dalga rekoru, hız run'ları, leaderboard sıralaması

### Flow State Design

- **Onboarding curve**: İlk run'da 3 temel mutasyon sunulur, 30 saniyede hareket öğrenilir, ilk level-up'ta sistem anlaşılır
- **Difficulty scaling**: Dalga bazlı zorluk artışı + düşman çeşitliliği. Her run'ın ilk 2 dakikası güvenli, sonra rampa başlar
- **Feedback clarity**: XP barı, düşman sayacı, dalga numarası, karakter üzerinde görsel büyüme — her şey açıkça gösterilir
- **Recovery from failure**: Ölüm = run sonu, ama meta-ilerleme korunur. 5 saniyede yeni run başlar. Başarısızlık eğitici: "Bu sefer o synergy'yi denemeyeceğim"

---

## Core Loop

### Moment-to-Moment (30 seconds)
Oyuncu ekranda hareket eder, saldırılar otomatik tetiklenir. Düşman sürüleri her yönden gelir. Pozisyonlama kritik — düşman yoğunluğunu yönet, XP gem'lerini topla, tehlikeli bölgelerden kaç. Mutasyonların otomatik ateşi ekranı doldurur — görsel kaos ama kontrollü.

### Short-Term (5-15 minutes)
XP toplayarak level-up ol, 3 mutasyondan birini seç. Her seçim vücudunu değiştirir — hem görsel hem mekanik. Synergy'leri keşfet: "Bıçak kolu + hız bacağı = girdap saldırısı!" Her 3-4 dakikada bir elite/mini-boss dalgası. Run 10-12 dakikada arena boss'una ulaşır.

### Session-Level (30-120 minutes)
2-4 run bir oturumda. Her run farklı mutasyon kombinasyonları dener. Ölümde: meta-para kazanılır, yeni mutasyonlar unlock'lanır, hikaye parçası açılır. "Bu sefer tank build deneyeceğim" → yeni run.

### Long-Term Progression
- **Unlock'lar:** Yeni mutasyon türleri (başlangıçta 15, tam oyunda 40+), yeni başlangıç formları, yeni arena biome'ları
- **Stat yükseltme:** Meta-para ile kalıcı küçük güçlenmeler (max HP, XP çarpanı, başlangıç hızı)
- **Hikaye:** Her boss ilk kez yenildiğinde lore açılır. Arena'nın ne olduğu, sentetik varlıkların kökeni yavaşça ortaya çıkar.

### Retention Hooks
- **Curiosity**: "O kilitli mutasyonu açınca ne oluyor? İki nadir mutasyonun synergy'si ne?"
- **Investment**: Unlock ilerlemesi, koleksiyon tamamlama yüzdesi
- **Social**: Leaderboard rekabeti, "bakın ne oldum" screenshot'ları
- **Mastery**: Daha yüksek dalga, daha hızlı boss kill, zor modlar

---

## Game Pillars

### Pillar 1: Görsel Evrim Tatmini
Her mutasyon karakterde görünür bir değişiklik yaratır. Oyuncunun "ne olduğunu" görmesi, build'inin kimliğidir.

*Design test*: "Yeni bir mutasyon eklerken görsel etkisi yoksa, o mutasyon eklenmez."

### Pillar 2: Hızlı ve Kaotik Run'lar
10-15 dakikalık run'lar. Hızlı başla, hızlı güçlen, hızlı bitir. "Bir tane daha" hissi her şeyin önünde.

*Design test*: "Bir özellik run süresini 20 dakikanın üzerine çıkarıyorsa, kesilir veya yeniden tasarlanır."

### Pillar 3: Synergy Keşfi
Mutasyon kombinasyonları sürpriz etkileşimler yaratır. Oyuncu her run'da yeni bir şey keşfedebilir.

*Design test*: "Yeni bir mutasyon eklenirken en az 2 mevcut mutasyonla synergy'si tanımlanmalı."

### Pillar 4: Kolay Giriş, Derin Derinlik
Kontroller basit (sadece hareket), ama build kararları derin. Herkes oynayabilir, ustalar optimize eder.

*Design test*: "İlk run'da oyuncuya 30 saniyeden fazla tutorial gösterilmez."

### Anti-Pillars (What This Game Is NOT)

- **Hikaye odaklı DEĞİL**: Lore var ama cutscene'ler veya diyalog ağaçları yok — hikaye run'ların arasında damla damla akar. Derin hikaye, run hızını bozar.
- **Hassas skill-based DEĞİL**: Bu bullet-hell değil. Pozisyon önemli ama pixel-perfect kaçınma beklenmez. Frustration, "bir tane daha" hissini öldürür.
- **Sandbox/open-world DEĞİL**: Arena bazlı, kapalı alanlar. Açık dünya keşfi yok. Odak dağınıklığı, run'ların keskinliğini bozar.

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| Vampire Survivors | Otomatik saldırı, survivor core loop, ekran dolduran kaos | Modüler görsel evrim, synergy derinliği | Türün pazar başarısını kanıtladı |
| The Binding of Isaac | Çılgın item synergy'leri, her run farklı, görsel item yansıması | Survivor formatında (auto-attack), daha kısa run'lar | Synergy sisteminin bağımlılık yarattığını kanıtladı |
| Brotato | Kısa run'lar, çoklu silah slotları, basit kontroller | Silah yerine vücut mutasyonları, görsel karakter dönüşümü | Solo dev survivor'ın yapılabilirliğini kanıtladı |

**Non-game inspirations**: Akira (biyolojik dönüşüm estetiği), The Thing (mutasyon kabusu), Tron (dijital arena atmosferi), Pokemon evrim animasyonları (dönüşüm tatmini)

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | 16-35 |
| **Gaming experience** | Mid-core (survivor/roguelite deneyimi olan) |
| **Time availability** | 15-60 dakikalık oturumlar, günlük veya gün aşırı |
| **Platform preference** | PC (Steam), gamepad veya klavye+mouse |
| **Current games they play** | Vampire Survivors, Brotato, Isaac, Hades, 20 Minutes Till Dawn |
| **What they're looking for** | Survivor türünde taze bir mekanik, build çeşitliliği, "bir tane daha" hissi |
| **What would turn them away** | Aşırı zorluk, yavaş başlangıç, görsel monotonluk, pay-to-win |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | Unity (C# — projede zaten konfigüre, geniş 2D desteği, sprite compositing araçları) |
| **Key Technical Challenges** | Modüler sprite compositing sistemi (çok parçalı karakter rendering), çok sayıda düşman performansı (object pooling kritik), synergy matrisi yönetimi |
| **Art Style** | Pixel art (16x16 veya 32x32 base, modüler parçalar) |
| **Art Pipeline Complexity** | Orta — base sprite + modüler parça katmanları. Parça başına ~4 yön animasyonu |
| **Audio Needs** | Orta — chiptune/synthwave müzik, satisfying hit/kill SFX, mutasyon ekleme jingle |
| **Networking** | Yok (single-player) |
| **Content Volume** | ~5-8 arena biome, ~30-40 mutasyon, ~8-10 boss, ~15-20 düşman tipi, ~20-30 saat tam içerik |
| **Procedural Systems** | Dalga oluşturma (düşman kompozisyonu), mutasyon havuz seçimi, arena layout varyasyonları |

---

## Risks and Open Questions

### Design Risks
- Modüler mutasyon sistemi görsel olarak "frankenstein" hissi verebilir — dikkatli art direction ve renk paleti tutarlılığı gerekli
- 40+ mutasyon arasında synergy dengesi çok zor olabilir — bazı kombinasyonlar oyunu kırabilir (bu kısmen istenen bir özellik ama sınırları olmalı)

### Technical Risks
- Çok sayıda sprite katmanı + çok sayıda düşman = potansiyel performans sorunu. Object pooling ve sprite batching kritik
- Modüler sprite sistemi animasyon uyumluluğu — her parça her animasyon frame'inde doğru pozisyonda olmalı

### Market Risks
- Survivor türü hızla doygunlaşıyor — "görsel evrim" hook'u yeterince farklılaştırıcı mı?
- Steam'de survivor oyunları fiyat yarışında — premium fiyatlandırma baskısı

### Scope Risks
- 40 mutasyon x 4 yön animasyonu = büyük asset yükü. MVP'de 10-15 mutasyonla başlamak kritik
- Meta-ilerleme + hikaye + unlock sistemi scope'u şişirebilir — MVP'de minimal tutulmalı

### Open Questions
- Modüler sprite compositing Unity'de en iyi nasıl yapılır? — Prototipte test edilmeli (SpriteRenderer layering vs. custom shader)
- Synergy matrisi nasıl tanımlanır? — Data-driven JSON/ScriptableObject yapısı prototiplenmeli
- "Evrimleşmiş form" (A+B=C) kaç tane olmalı? — İlk 5-10 synergy'yi prototipleyip oyuncu tepkisini ölçmeli

---

## MVP Definition

**Core hypothesis**: "Modüler görsel mutasyonlarla evrimleşen bir karakter, survivor core loop'una anlamlı bir derinlik ve tatmin katıyor — oyuncular 'bir tane daha' run yapmak istiyor."

**Required for MVP**:
1. Temel survivor hareket + otomatik saldırı sistemi
2. 10-12 modüler mutasyon (kol, bacak, sırt, baş slotları), her biri görsel yansımalı
3. 3-5 temel synergy (iki mutasyon birleşince evrimleşmiş form)
4. 1 arena biome, 5-7 dalga, 1 boss
5. Level-up mutasyon seçim ekranı (3 seçenek)
6. Temel XP ve düşman spawning sistemi

**Explicitly NOT in MVP** (defer to later):
- Meta-ilerleme (kalıcı unlock, stat yükseltme)
- Hikaye/lore sistemi
- Birden fazla arena biome
- Leaderboard
- Ses ve müzik (placeholder SFX yeterli)
- Başlangıç formu seçimi

### Scope Tiers (if budget/time shrinks)

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **MVP** | 1 arena, 10 mutasyon, 1 boss | Core loop + mutasyon sistemi | 4-6 hafta |
| **Vertical Slice** | 2 arena, 20 mutasyon, 3 boss | + Meta-ilerleme + 10 synergy | 8-10 hafta |
| **Alpha** | 4 arena, 30 mutasyon, 6 boss | + Hikaye + leaderboard + ses | 14-18 hafta |
| **Full Vision** | 8 arena, 40+ mutasyon, 10 boss | + Polish + başlangıç formları + tam içerik | 20-26 hafta |

---

## Next Steps

- [ ] Get concept approval from creative-director
- [ ] Fill in CLAUDE.md technology stack based on engine choice (`/setup-engine`)
- [ ] Create game pillars document (`/design-review` to validate)
- [ ] Decompose concept into systems (`/map-systems` — maps dependencies, assigns priorities, guides per-system GDD writing)
- [ ] Create first architecture decision record (`/architecture-decision`)
- [ ] Prototype core loop (`/prototype [core-mechanic]`)
- [ ] Validate core loop with playtest (`/playtest-report`)
- [ ] Plan first milestone (`/sprint-plan new`)
