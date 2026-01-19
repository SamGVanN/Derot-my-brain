# Color Palettes - Derot My Brain

This document consolidates all available theme color palettes for the application.

---

## 1. Derot Brain — Cognition, Stimulation, Focus

**Recommended for:** Main project theme, neuroscience focus

### Ambiance
- Neurosciences
- Stimulation intellectuelle
- Léger côté futuriste

### Palette

| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Quasi noir | `#0B0E14` |
| Surface | Dark slate | `#161A22` |
| Primary | Violet cognition | `#7C3AED` |
| Secondary | Light purple | `#A78BFA` |
| Accent | Vert validation / réussite | `#22C55E` |
| Warning | Orange | `#F59E0B` |
| Text Primary | Light gray | `#E5E7EB` |
| Text Secondary | Medium gray | `#9CA3AF` |

### Usage
- **Violet** = réflexion / abstraction
- **Vert** = réponses correctes / score
- **Orange** = erreurs ou info importante

---

## 2. Knowledge Core — Sérieux, Sobre, Intellectuel

**Recommended for:** POC, professional appearance, learning-focused

### Ambiance
- Université / encyclopédie moderne
- Concentration, clarté, calme

### Palette

| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Bleu nuit profond | `#0F172A` |
| Surface | Bleu ardoise | `#1E293B` |
| Primary | Bleu savoir | `#3B82F6` |
| Secondary | Gris bleuté | `#64748B` |
| Accent | Cyan curiosité | `#22D3EE` |
| Text Primary | Blanc cassé | `#F8FAFC` |
| Text Secondary | Light slate | `#CBD5E1` |

### Usage
- **Bleu** = confiance / connaissance
- **Cyan** = actions (quiz, recycler, backlog)

---

## 3. Curiosity Loop — Exploration, Surprise

**Recommended for:** Playful experience, discovery-focused

### Ambiance
- Découverte
- Errance contrôlée
- Apprentissage par curiosité

### Palette

| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Bleu spatial | `#020617` |
| Surface | Bleu spatial | `#020617` |
| Primary | Bleu exploration | `#38BDF8` |
| Secondary | Indigo | `#6366F1` |
| Accent | Rose action | `#F43F5E` |
| Success | Green | `#22C55E` |
| Text Primary | Very light slate | `#F1F5F9` |
| Text Secondary | Medium slate | `#94A3B8` |

### Usage
- **Accent rose** pour :
  - Recycler
  - Passer au quiz
- Très dynamique visuellement

---

## 4. Mind Lab — Science & Expérimentation

**Recommended for:** Lab/test/POC feel, experimental approach

### Palette

| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Dark blue | `#0F172A` |
| Surface | Very dark blue | `#020617` |
| Primary | Teal expérimentation | `#14B8A6` |
| Secondary | Sky blue | `#0EA5E9` |
| Accent | Yellow | `#FACC15` |
| Text Primary | Light slate | `#E2E8F0` |
| Text Secondary | Medium slate | `#94A3B8` |

---

## 5. Neo-Wikipedia — Neutre, Contenu-First

**Recommended for:** Content-focused, long reading sessions, Wikipedia integration

### Ambiance
- Encyclopédie moderne
- Discrète
- Très lisible sur de longs articles

### Palette

| Element | Color | Hex Code |
|---------|-------|----------|
| Background | Off-white | `#FAFAF9` |
| Surface | White | `#FFFFFF` |
| Primary | Blue | `#2563EB` |
| Secondary | Slate | `#475569` |
| Accent | Green | `#16A34A` |
| Border | Light gray | `#E5E7EB` |
| Text Primary | Very dark gray | `#111827` |
| Text Secondary | Dark gray | `#374151` |

### Usage
- Très confortable pour la lecture
- Parfait si Wikipédia est central

---

## Implementation Notes

All themes use semantic color variables defined in the application's CSS. When implementing a new theme:

1. Define all color variables in `src/frontend/src/index.css`
2. Use semantic class names (e.g., `bg-background`, `text-primary`)
3. Never hardcode color values in components
4. Test in both light and dark modes if applicable

See [Frontend-Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Frontend-Architecture.md) for theming guidelines.
