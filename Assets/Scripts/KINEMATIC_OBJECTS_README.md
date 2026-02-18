# Kinematic Push & Pivot System - Implementation Guide

## 🎯 Overview
System kinematyczny dla obiektów puzzlowych, które poruszają się po ścieżce bez użycia fizyki Unity. Wspiera dwa typy ruchu: **Slide** (liniowy) oraz **Pivot** (obrotowy).

## 📋 Komponenty

### 1. **KinematicObject.cs**
Główny komponent do przyczepienia do obiektów puzzlowych (obrotowe ściany, przesuwne platformy).

### 2. **PlayerInteraction.cs** (Zaktualizowany)
Obsługuje interakcje z obiektami kinematycznymi przez naciśnięcie klawisza **X**.

### 3. **PlayerMovement.cs** (Zaktualizowany)
Steruje ruchem obiektu kinematycznego przez przytrzymanie klawisza **W**.

## 🛠️ Jak Skonfigurować

### Dla Obiektu Przesuwnego (Sliding Platform):

1. **Stwórz GameObject** dla platformy
   - Upewnij się, że GameObject ma **Scale (1, 1, 1)**
   - Dodaj mesh jako dziecko i tam ustaw skalę

2. **Dodaj Komponenty:**
   ```
   - KinematicObject
   - Collider (z włączonym Is Trigger)
   ```

3. **Konfiguracja KinematicObject:**
   - **Movement Type:** Slide
   - **Target Transform:** Stwórz pusty GameObject reprezentujący pozycję końcową
   - **Speed:** 2.0 (jednostki/sekundę)
   - **Grab Position:** Stwórz dziecko Transform gdzie gracz będzie stał

4. **Hierarchia:**
   ```
   SlidingPlatform (Scale 1,1,1)
   ├─ Model (Mesh + Scale tu)
   ├─ GrabPosition (Transform)
   └─ Collider (Is Trigger = true)
   
   TargetPosition (Osobny GameObject w scenie)
   ```

### Dla Obrotowego Obiektu (Rotating Wall/Door):

1. **Stwórz GameObject** dla drzwi/ściany
   - Scale musi być **(1, 1, 1)**

2. **Dodaj Komponenty:**
   ```
   - KinematicObject
   - Collider (z włączonym Is Trigger)
   ```

3. **Konfiguracja KinematicObject:**
   - **Movement Type:** Pivot
   - **Target Transform:** Opcjonalne (nie używane w Pivot mode)
   - **Speed:** 45.0 (stopnie/sekundę)
   - **Pivot Anchor:** Transform reprezentujący zawias (hinge)
   - **Max Rotation Angle:** 90 (stopnie)
   - **Grab Position:** Transform gdzie gracz będzie stał

4. **Hierarchia:**
   ```
   RotatingWall (Scale 1,1,1)
   ├─ Model (Mesh + Scale tu)
   ├─ GrabPosition (Transform)
   ├─ PivotAnchor (Transform na krawędzi - zawias)
   └─ Collider (Is Trigger = true)
   ```

## 🎮 Sterowanie

| Klawisz | Akcja |
|---------|-------|
| **X** | Włącz/wyłącz interakcję z obiektem |
| **W** (przytrzymaj) | Przesuń/obróć obiekt w kierunku celu |
| **Puść W** | Zatrzymaj ruch (obiekt pozostaje w miejscu) |

## ⚠️ Ważne Zasady

### ✅ DO:
- Zawsze ustawiaj **Scale (1,1,1)** na obiekcie głównym
- Używaj scale tylko na dziecku (mesh)
- Upewnij się, że **Pivot Anchor** jest pionowy dla Pivot mode
- Testuj ścieżkę ruchu - **brak kolizji**, obiekt będzie "przenikał"

### ❌ DON'T:
- Nie używaj Rigidbody na KinematicObject
- Nie używaj non-uniform scale na głównym GameObject
- Nie zakładaj, że obiekt zatrzyma się przy kolizji (to jest kinematyka!)
- Nie używaj Physics.AddForce ani Rigidbody.velocity

## 🐛 Debug Gizmos

W trybie edytora zobaczysz:
- **Cyan Sphere** - Grab Position (gdzie pojawi się gracz)
- **Green Cube** - Target Position (dla Slide mode)
- **Yellow Line** - Ścieżka ruchu (dla Slide mode)
- **Red Sphere** - Pivot Anchor (dla Pivot mode)
- **Yellow Arc** - Łuk obrotu (dla Pivot mode, tylko gdy wybrany)

## 📊 Definition of Done - Checklist

- [x] **Slide Object:** Naciśnięcie 'X' chwyta. 'W' przesuwa do celu. Puszczenie 'W' zatrzymuje natychmiast.
- [x] **Pivot Object:** 'W' obraca obiekt wokół zdefiniowanej krawędzi (nie środka).
- [x] **Clamping:** Przytrzymanie 'W' nie przesuwa obiektu poza targetTransform.
- [x] **Player Movement:** Gracz porusza się wraz z obiektem podczas chwytania.
- [x] **Visual Check:** Obrót wygląda jak otwieranie drzwi, nie jak bączek.
- [x] **No Physics:** Brak użycia Rigidbody.velocity czy AddForce.
- [x] **One-Way Logic:** [W] przesuwa naprzód. Puszczenie pauzuje.

## 🔧 Metody Publiczne API

### KinematicObject:
```csharp
void StartInteraction(Transform player)  // Rozpocznij interakcję
void StopInteraction()                   // Zakończ interakcję
void AdvanceMovement(float deltaTime)    // Przesuń obiekt (wywoływane przez PlayerMovement)
bool HasReachedTarget()                  // Czy osiągnięto cel?
void ResetToStart()                      // Reset do pozycji startowej
bool IsInteracting { get; }              // Czy obecnie interakcja jest aktywna?
```

## 📝 Przykładowe Wartości

### Drzwi obrotowe:
- Movement Type: **Pivot**
- Speed: **45** (stopni/sek = 90° w 2 sekundy)
- Max Rotation Angle: **90**

### Przesuwna platforma:
- Movement Type: **Slide**
- Speed: **2.0** (jednostki/sek)

### Wolno obracająca się ściana:
- Movement Type: **Pivot**
- Speed: **30** (90° w 3 sekundy)
- Max Rotation Angle: **120** (szerszy kąt)

## 🎓 Level Design Tips

1. **Ścieżka musi być czysta** - Obiekt przejdzie przez wszystko
2. **Testuj w Play Mode** - Gizmos pokażą rzeczywistą ścieżkę
3. **Grab Position** - Umieść tak, żeby animacja gracza wyglądała naturalnie
4. **Pivot Anchor** - Dla drzwi umieść dokładnie na zawiasie
5. **Speed** - Dostosuj do "wagi" obiektu (wolniej = ciężej)

---

## 🚀 Quick Start Example

Szybki setup dla obrotowych drzwi:

1. Stwórz GameObject "RotatingDoor"
2. Dodaj `KinematicObject` komponent
3. Ustaw Movement Type na `Pivot`
4. Stwórz child "Model" (twój mesh)
5. Stwórz child "GrabPosition" (przesuń przed drzwi)
6. Stwórz child "PivotAnchor" (przesuń do krawędzi/zawiasu)
7. Dodaj Box Collider (Is Trigger = true)
8. Ustaw Speed = 45, Max Rotation = 90
9. Przypisz GrabPosition i PivotAnchor w inspektorze
10. Test!

---

**Wersja:** 1.0  
**Data:** 2026-02-18  
**Status:** ✅ Production Ready

