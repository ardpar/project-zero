# Dalga / Spawning Sistemi

> **Status**: Designed
> **Author**: User + Claude Code Game Studios
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Hızlı ve Kaotik Run'lar

## Overview

Dalga/Spawning Sistemi, run boyunca düşman oluşturma zamanlamasını, kompozisyonunu ve zorluk eğrisini yöneten sistemdir. Zaman bazlı dalgalar kullanır: her dalga belirli bir süre boyunca düşman spawn eder, dalga sonunda elite veya boss düşman gelir. Düşman çeşitliliği ve yoğunluğu her dalgada artar. 10-15 dakikalık run hedefine uygun 5-7 dalga yapısı, son dalga boss fight ile biter.

## Player Fantasy

Oyuncu "durdurulamaz bir sel"e karşı koyduğunu hissetmeli. İlk dakikalarda rahat, 5. dakikadan sonra baskı artıyor, son dakikalarda "hayatta kalabilir miyim?" gerilimi. Dalga geçişleri nefes alma anı — kısa mola + "sonraki dalga ne getirecek?" beklentisi. Boss fight run'ın doruk noktası.

## Detailed Rules

### Core Rules

**Dalga Yapısı:**
1. Run, `wave_count` (varsayılan: 6) dalgadan oluşur + 1 boss fight
2. Her dalga `wave_duration` sürer (dalga bazlı, data-driven)
3. Dalga sırasında düşmanlar `spawn_interval` aralıklarla spawn olur
4. Dalga bitişinde kısa `wave_break` (varsayılan: 3 sn) — düşman spawn durur, kalan düşmanlar hayatta

**Varsayılan Dalga Tablosu:**

| Dalga | Süre | Düşman Türleri | Spawn Hızı | Elite? | Tema |
|-------|------|---------------|------------|--------|------|
| 1 | 90 sn | Chaser | Yavaş (2sn) | Hayır | Isınma |
| 2 | 90 sn | Chaser + Runner | Orta (1.5sn) | Hayır | Hız tanıtımı |
| 3 | 120 sn | Chaser + Runner + Shooter | Orta (1.2sn) | 1 Elite (dalga sonu) | Menzil tehdidi |
| 4 | 120 sn | Tümü + Exploder | Hızlı (1.0sn) | 2 Elite | Patlayıcı tehlike |
| 5 | 120 sn | Tümü (ağırlıklı Runner+Shooter) | Çok hızlı (0.8sn) | 3 Elite | Baskı doruk |
| 6 | 150 sn | Tümü (karışık, yoğun) | Maksimum (0.6sn) | 4 Elite | Son dalga |
| Boss | ∞ | Boss + Chaser spawn (yavaş) | Yavaş (3sn) | Boss | Final |

5. Bu tablo ScriptableObject olarak saklanır, tamamen düzenlenebilir
6. Her dalga entry'si: süre, spawn havuzu (tür + ağırlık), spawn hızı, elite sayısı

**Düşman Spawn Mekanikleri:**
7. Düşmanlar ekran dışında, oyuncudan `min_spawn_distance` (varsayılan: 12 birim) uzakta spawn olur
8. Spawn pozisyonu: ekranın kenarlarında rastgele dağılım (360°)
9. Aynı noktadan çok fazla spawn olmasını engellemek için spawn pozisyonları sektörlere bölünür (4-8 sektör, round-robin)
10. Spawn edilen düşman türü dalga havuzundan ağırlıklı rastgele seçilir

**Elite Spawn:**
11. Elite, dalga sonuna doğru (`elite_spawn_time` = dalga süresinin %80'ı) spawn olur
12. Elite spawn öncesi kısa uyarı (yer göstergesi, 1 sn warning)
13. Elite düşman türü dalga havuzundan seçilir ama tier = Elite

**Boss Fight:**
14. Son dalga tamamlandıktan sonra boss fight başlar
15. Boss ekranın merkezine doğru yavaşça girer (cinematic giriş, 2 sn)
16. Boss fight sırasında yavaş Chaser spawn devam eder (baskı sürer)
17. Boss yenildiğinde run tamamlanır (Win)
18. Boss yenilmezse run süresiz devam eder — oyuncu ölene kadar

**Zorluk Ölçekleme:**
19. Düşman HP, hız ve hasar dalga numarasına göre ölçeklenir (Entity Health + Düşman AI GDD'lerindeki formüller)
20. Spawn hızı dalga bazlı tablo ile kontrol edilir (yukarıdaki tablo)
21. Düşman çeşitliliği kademeli olarak açılır (erken dalgalar basit, geç dalgalar karmaşık)

**Ekrandaki Düşman Limiti:**
22. Maksimum aynı anda ekranda `max_alive_enemies` (varsayılan: 200) düşman
23. Limit aşıldığında yeni spawn durur, mevcut düşmanlar ölene kadar bekler
24. Bu soft limit — performans kritik eşik

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| **Wave Active** | Run başlangıcı / Önceki wave break bitti | Wave süresi doldu | Düşman spawn, elite spawn (geç) |
| **Wave Break** | Wave süresi doldu | wave_break süresi doldı | Spawn durur, kalan düşmanlar hayatta, nefes alma |
| **Boss Phase** | Son dalga tamamlandı | Boss öldü / Oyuncu öldü | Boss + yavaş chaser spawn |
| **Complete** | Boss öldü | Run Manager devralır | Spawn durur, zafer |
| **Paused** | Pause / Level-up | Resume | Spawn timer dondurulur |

### Interactions with Other Systems

| Sistem | Yön | Arayüz |
|--------|-----|--------|
| **Düşman AI** | Downstream | Spawn edilen düşmana EnemyData atanır (tür, tier). AI davranışı devralır. |
| **Entity Health** | Downstream | Spawn edilen düşmana HP/damage data'sı atanır (wave_number ile ölçeklenmiş) |
| **XP & Level-Up** | Dolaylı | Daha çok düşman = daha çok XP gem = daha hızlı level-up |
| **Gameplay HUD** | Downstream | Dalga numarası, dalga timer, boss HP bar |
| **Run Manager** | Downstream | Boss yenildiğinde "Run Complete" event'i |

## Formulas

### Spawn Rate (per wave)
```
spawn_interval = wave_table[wave_number].spawn_interval
enemies_per_minute = 60 / spawn_interval
```

| Dalga | Interval | Enemies/min |
|-------|----------|-------------|
| 1 | 2.0 sn | 30 |
| 2 | 1.5 sn | 40 |
| 3 | 1.2 sn | 50 |
| 4 | 1.0 sn | 60 |
| 5 | 0.8 sn | 75 |
| 6 | 0.6 sn | 100 |

### Total Enemies per Run (Estimate)
```
total ≈ sum(wave_duration * enemies_per_minute for each wave) + boss_phase_enemies
// ≈ (90*30 + 90*40 + 120*50 + 120*60 + 120*75 + 150*100)/60 + ~30
// ≈ 45 + 60 + 100 + 120 + 150 + 250 + 30
// ≈ 755 düşman per run
```

### Difficulty Curve Shape
```
perceived_difficulty = enemy_count * enemy_hp * enemy_damage / player_power
// Hedef: ilk 2 dk kolay, 2-8 dk kademeli artış, 8-12 dk zirve, boss = final sınav
```

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| Oyuncu çok güçlü — düşmanlar anında ölüyor | Spawn hızı dalga tablosuna göre sabit — oyuncu güçlü hisseder, bu sorun değil | "Güçlenme spirali" pillar ile uyumlu |
| Oyuncu çok zayıf — düşmanlar birikiroir | max_alive_enemies limiti korur. Oyuncu pozisyonlamayla hayatta kalabilir. | Soft skill ceiling |
| 200 düşman ekranda + spawn limit | Spawn bekler, mevcut düşmanlar ölünce devam eder | Performans koruması |
| Level-up dalga ortasında | Oyun duraklar, spawn timer donar. Resume'da dalga kaldığı yerden devam. | Adil pause |
| Boss fight sırasında oyuncu çok uzun hayatta kalır | Boss fight süresiz — boss yenilene kadar veya oyuncu ölene kadar | Run sona ermeli: ya kazan ya kaybet |
| Dalga tablosunda tanımsız dalga (7+) | Son dalga tekrar eder (spawn hızı + düşman çeşitliliği sabit kalır) | Uzun run fallback |
| Spawn pozisyonu duvarın içinde | Spawn pozisyonu arena bounds + offset ile sınırlanır, geçerli pozisyon bulunana kadar retry (max 3) | Geçersiz spawn önleme |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| **Düşman AI** | Downstream | Hard — spawn edilen düşman AI'ya devredilir |
| **Entity Health** | Downstream | Hard — düşman HP ataması |
| **XP & Level-Up** | Dolaylı | Soft — spawn hızı level-up hızını etkiler |
| **Gameplay HUD** | Downstream | Soft — dalga bilgisi gösterimi |
| **Run Manager** | Downstream | Hard — boss yenilme = run complete |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| `wave_count` | 6 | 4-8 | Daha uzun run | Daha kısa run |
| `wave_break_duration` | 3.0 | 1.0-5.0 | Daha uzun mola | Nefes almadan devam |
| `min_spawn_distance` | 12.0 | 8.0-16.0 | Uzaktan spawn (tepki zamanı) | Yakından spawn (baskı) |
| `max_alive_enemies` | 200 | 100-300 | Daha kalabalık ekran | Temiz ekran, performanslı |
| `elite_spawn_time_ratio` | 0.8 | 0.6-0.95 | Elite geç gelir | Elite erken gelir |
| `boss_chaser_spawn_interval` | 3.0 | 2.0-5.0 | Boss fight'ta daha çok chaser | Daha az baskı |
| Wave tablosu (per-wave) | Yukarıdaki tablo | Her entry ayrı | Dalga bazlı fine-tuning | — |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| Dalga başlangıcı | "WAVE X" banner (1 sn) | Dalga başlangıç SFX | High / Medium |
| Dalga sonu | "WAVE CLEAR" banner (kısa) | Rahatlatıcı "clear" jingle | High / High |
| Elite spawn uyarısı | Yerde kırmızı daire (1 sn) | Uyarı SFX (ominous) | High / High |
| Boss giriş | Ekran karartma + boss isim banner + slow-mo | Epik boss giriş müziği | Critical / Critical |
| Boss yenildi | Büyük patlama, confetti, "VICTORY" banner | Zafer fanfare | Critical / Critical |
| Spawn (düşman belirme) | Fade-in / portal efekti (ekran dışında) | Hafif "pop" (toplu spawn'da sessiz) | Low / Low |

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| Dalga numarası | HUD üst köşe | Dalga değişiminde | Her zaman |
| Dalga timer (kalan süre) | Dalga numarasının yanında | Her saniye | Dalga aktifken |
| Boss HP bar | Ekran üstü (büyük) | Her frame | Boss fight |
| "WAVE X" banner | Ekran ortası | Dalga başında (1 sn) | Dalga geçişi |

## Acceptance Criteria

- [ ] Dalga tablosuna göre düşmanlar doğru aralıklarla spawn olur
- [ ] Her dalga doğru düşman türlerini içerir (kademeli açılım)
- [ ] Elite düşmanlar dalga sonuna doğru spawn olur
- [ ] Boss fight son dalga sonrasında başlar
- [ ] Boss yenildiğinde run tamamlanır event'i tetiklenir
- [ ] Spawn pozisyonları ekran dışında ve arena içinde
- [ ] max_alive_enemies limiti çalışır
- [ ] Wave break sırasında spawn durur
- [ ] Pause sırasında spawn timer donar
- [ ] **Performance:** Spawn logic < 0.2ms per frame
- [ ] Dalga tablosu ScriptableObject'ten okunur

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| Dalga arası bonus event olacak mı? (treasure wave, healing wave) | Game Designer | Vertical Slice | MVP'de yok, VS'de değerlendirilir |
| Boss türü per-biome farklı mı? | Game Designer | Alpha | MVP: 1 boss türü. Alpha: biome başına unique boss |
| Adaptive difficulty (oyuncu performansına göre spawn ayarı) | Systems Designer | Full Vision | MVP'de yok, sabit dalga tablosu |
| Arena biome'ları dalga tablosunu değiştirir mi? | Level Designer | Vertical Slice | MVP: tek biome, tek tablo |
