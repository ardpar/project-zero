# SYNTHBORN — World-Building & Narrative Audit Report

> **Status**: Draft
> **Date**: 2026-03-29
> **Authored by**: narrative-director + world-builder
> **Resolution**: Option A adopted — "Arena Remembers, Subject Doesn't"

---

## Narrative Health Assessment: 6.5 / 10

### Güçlü
- Temel dünya konsepti sağlam (Arena, SUBJECT-FINAL, klinik ton)
- Duygusal ark 6 faza haritalanmış
- Ton sütunları net (Clinical Curiosity, Incomplete Information, Body as Language)
- Boss felsefesi mükemmel (aynalar, düşmanlar değil)
- Mutation isimlendirme lore taşıyıcı olarak çalışıyor
- Ludonarrative harmony: mutasyon seçimi = hikaye anlatımı

### Zayıf
- Sadece felsefe var, uygulama eksik
- Expansion-lore çelişkileri vardı (v2'de çözüldü)
- Boss roster'ı yok, sadece 3 örnek log
- 6 biome'dan sadece 1'i detaylı
- Lore delivery pipeline tasarlanmamış

---

## Çözülen Çelişkiler (Option A ile)

| Çelişki | Eski Durum | Çözüm |
|---------|-----------|-------|
| Hafıza silme vs. kalıcılık | XP/ekipman ölümden sağ çıkar | Arena'nın dosyası kalıcı, Subject sıfırlanır |
| İsimsiz kimlik vs. karakter yaratma | İsim + Warrior/Rogue/Mage/Sentinel | SUBJECT-FINAL-[SUFFIX] + Substrate Konfigürasyonları |
| Ölüm = reset vs. ilerleme | %20 gold kalıcı | Arena kalibrasyon kaydına veri işlenir |
| Fantasy biome vs. Arena çağları | Caverns/Jungle/Temple/Hell | 6 Arena çağ biome'u korundu |
| 100 level tamamlama vs. sonsuz döngü | Level 100 = bitiş | Arena'nın kalibrasyon döngüsü tamamlanır, Subject "kazanmaz" |

---

## Hala Eksik Dökümanlar (Öncelik Sırasıyla)

### P1 — Architect Codex (İç Referans)
- Oyuncunun asla görmeyeceği "gerçek cevaplar" dökümanı
- Mimar'ların soyu tükenme nedeni (canonical)
- CONTINGENCY-ZERO'nun asıl amacı
- SUBJECT-FINAL ne olmak üzere tasarlandı
- **Kural gereği zorunlu:** "Mysteries must have documented true answers"
- Tahmini boyut: 400-600 kelime

### P2 — Boss Registry
- 8-10 boss roster'ı tanımlanmalı
- Her boss: designasyon, biome, temsil ettiği mutation yolu, duygusal ark fazı, açığa çıkardığı lore beat
- 3 mevcut örnek (STRATUM-7, LACERATE-3, NULLFRAME) onaylanmalı veya genişletilmeli
- Tahmini boyut: 800-1200 kelime

### P3 — Signal Archive Design
- Lore fragment koleksiyon ve gösterim UI spesifikasyonu
- Fragment format kısıtlamaları (karakter limiti, gösterim süresi)
- Unlock trigger mekanizması
- Koleksiyon ilerleme göstergesi
- Fragment yazılmadan ÖNCE tasarlanmalı (format kısıtlamaları yazıyı belirler)

### P4 — Biome 2-6 Environmental Narrative
- The Atrium şablonunu 5 biome daha için tekrarla
- Her biome: çevresel hikaye anlatımı öğeleri, pozisyonlar, iletilen mesaj, görsel yön

### P5 — Enemy Type Lore
- Normal düşman tipleri (Melee, Ranged, Exploder) için narrative kimlik
- Elite varyantları için lore

---

## Hala Eksik Dünya Kuralları

| Kural | Durum | Neden Önemli |
|-------|-------|-------------|
| Mutation teklif mantığı | Tanımsız | Arena nasıl seçiyor? Düşmanca mı, destekleyici mi? |
| Biome geçiş tetikleyicileri | Tanımsız | Ne tetikliyor? Performans, mutasyon eşiği, zamanlama? |
| Stabilized arası ilişkiler | Tanımsız | Birbirlerinin farkındalar mı? Hiyerarşi var mı? |
| NULLFRAME'in durumu | Çelişkili | "Asla Stabilize olmadı" ama Stabilized kategorisinde mi? |
| Arena gözlem mekanizmaları | Tanımsız | Kameralar, terminaller, sensörler nedir? |
| Pre-Architect varlığı | İpucu var, cevap yok | "Designation predating Arena file system" — kim? |
| Diğer Arena'lar var mı? | Tanımsız | "the Arena" tekil ama kural olarak belirtilmemiş |
| SUBJECT-FINAL tek aktif Subject mi? | Tanımsız | "FINAL" bu mu ima ediyor? |

---

## Lore Delivery Mekanizması Durumu

| Mekanizma | Durum | Eksik |
|-----------|-------|-------|
| Boss Fragment'ları (birincil) | Yazım kuralları var, 3 örnek var | Gösterim zamanlaması kararı, Signal Archive entegrasyonu |
| Arena Ortamı (ikincil) | Biome 1 tam | Biome 2-6 tasarlanmamış |
| Mutasyon İsimleri (üçüncül) | Draft, kısmen dolu | Tam mutation pool için açıklamalar eksik |
| Signal Archive (meta) | Sadece isim var | Tamamen tasarlanmamış |
| Yükleme/Ambient Text (önerildi) | 3 örnek var | Resmi mekanizma olarak tanınmamış |

---

## Mekansal Model Önerisi

**Konsantrik halkalar:** Biome'lar Arena'nın merkezinden dışa doğru katmanlar.
- The Atrium = en dış katman (giriş, temiz, yeni)
- The Null Chamber = merkez (en eski, tanımsız)
- Oyuncu dışarıdan içeriye ilerler
- Temporal ark (yeniden eskiye) + mekansal ark (dıştan içe) = aynı yön

Bu model Arena Haritası'nın grid düzeni için de kullanılabilir (dış halkadan merkeze).

---

## Sonraki Adımlar

1. **Architect Codex** yazılmalı (tüm narrative tutarlılığın sigortası)
2. **Boss Registry** tanımlanmalı (lore delivery pipeline'ın temeli)
3. **Signal Archive** tasarlanmalı (fragment yazılmadan önce format belirlensin)
4. **Duygusal ark yeniden kalibre edilmesi** — run sayısından boss defeat sayısına/oda progression'a rebase
5. **Loading/ambient text** resmi 5. mekanizma olarak narrative-brief'e eklenmeli
