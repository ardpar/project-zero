# Arena Balance Report — 100 Rooms Data Analysis

**Date**: 2026-04-02
**Build**: main @ be63b404 (Sprint 18)
**Method**: TrialChamberData SO extraction via MCP script-execute

---

## Summary

100 TrialChamberData SO'su analiz edildi. Biome dağılımı tasarıma uygun, pressure eğrisi genel olarak düzgün ama **oda 85-100 arasında pressure 5'te düzleşme** ve **erken biome'larda çeşitlilik eksikliği** tespit edildi.

**Verdict**: Genel yapı sağlam, 3 spesifik ayarlama önerisi var.

---

## Biome Dağılımı

| Biome | Oda Aralığı | Sayı | Tasarım Hedefi | Status |
|-------|-------------|------|----------------|--------|
| The Atrium | 1-16 | 16 | 16 | PASS |
| The Assay Chambers | 17-33 | 17 | 17 | PASS |
| The Deep Archive | 34-50 | 17 | 17 | PASS |
| The Collapse Stratum | 51-67 | 17 | 17 | PASS |
| The Corruption Layer | 68-84 | 17 | 17 | PASS |
| The Null Chamber | 85-100 | 16 | 16 | PASS |

Biome dağılımı expansion-vision ile tam uyumlu.

## Pressure Dağılımı

| Rating | Sayı | Oran | Biome Aralığı |
|--------|------|------|---------------|
| P1 | 9 | 9% | Atrium (oda 1-9, 13) |
| P2 | 16 | 16% | Atrium son + Assay başı |
| P3 | 17 | 17% | Assay son + Deep Archive |
| P4 | 22 | 22% | Deep Archive son + Collapse |
| P5 | 36 | 36% | Corruption + Null Chamber |

## Pressure Eğrisi Analizi

```
P5 ██████████████████████████████████████ 36
P4 ██████████████████████ 22
P3 █████████████████ 17
P2 ████████████████ 16
P1 █████████ 9
```

### Bulgular

**1. Null Chamber (85-100) düzleşmesi — UYARI**
Oda 85-100 arası tüm odalar P5. 16 ardışık P5 oda oyuncuyu monotonlaştırabilir.
- **Öneri**: Oda 85-86'yı P4'e düşür (giriş odaları), oda 92-93'ü P4'e düşür (nefes alma alanı). Son 3 oda (98-100) P5 kalsın.

**2. Corruption Layer (68-84) pressure çeşitliliği iyi**
P4-P5 arası salınım var (68:P4, 71:P5, 72:P4, 73:P5...). Bu "nefes alıp verme" ritmi doğru tasarlanmış.

**3. Atrium (1-16) pressure eğrisi düzgün**
P1→P2 geçişi oda 8'de, tutarlı ve yeni oyuncuya uygun.

**4. Deep Archive → Collapse geçişi (50-51) yumuşak**
P4→P4, biome değişse de zorluk sıçramıyor. Doğru tasarım.

## Pressure Multiplier Eğrisi

Formül: `1 + chamberNumber * 0.03`

| Oda | Multiplier | Etki |
|-----|-----------|------|
| 1 | 1.03x | Baseline (neredeyse normal) |
| 25 | 1.75x | %75 stat artışı |
| 50 | 2.50x | 2.5x daha güçlü düşmanlar |
| 75 | 3.25x | Zor ama yönetilebilir |
| 100 | 4.00x | Endgame zorluk |

Eğri lineer ve öngörülebilir. **Sorun yok** — oyuncu her oda arttığında ne beklediğini biliyor.

## Öneriler

### 1. Null Chamber Pressure Çeşitlendirmesi (Düşük Effort)
```
Mevcut:  85:5 86:5 87:5 88:5 89:5 90:5 91:5 92:5 93:5 94:5 95:5 96:5 97:5 98:5 99:5 100:5
Önerilen: 85:4 86:4 87:5 88:5 89:5 90:5 91:5 92:4 93:4 94:5 95:5 96:5 97:5 98:5 99:5 100:5
```
4 oda P4'e düşürülür → ritim oluşur.

### 2. Boss Odaları Vurgusu
Her biome'un son odası (16, 33, 50, 67, 84, 100) boss odası olarak etiketlenebilir.
Şu an pressure ile ayrışıyorlar ama `isBossRoom` flag'i ile özel ödül/müzik tetiklenebilir.

### 3. Runtime Playtest Doğrulama (Manuel)
Bu analiz statik veri üzerinden yapıldı. Gerçek zorluk deneyimi için:
- 10 tam run (oda 1→ölüm)
- Her biome geçişinde zorluk hissi notu
- Fragment ekonomisi yeterli mi (tedarik vs. kazanç)
- Ölüm odası dağılımı (hangi odalarda en çok ölünüyor)

---

*Rapor: Statik veri analizi. Runtime playtest ile doğrulama gerekli.*
