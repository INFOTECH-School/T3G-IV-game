# T3G-IV-game

*(English version below | Polska wersja poniżej)*

---

# 🇬🇧 English Version

## 🎮 Project Overview
This is a Unity game project. Please ensure you have the correct Unity version installed before opening the project to avoid serialization issues.

* **Unity Version:** `6000.X.XXXX`
* **Render Pipeline:** URP

### 🚀 Getting Started
1.  Clone the repository.
2.  Open **Unity Hub**.
3.  Click **Add** and select the folder containing this repository.
4.  Open the project.

> **Important:** Always commit `.meta` files! Every asset in Unity has a corresponding `.meta` file. If you add a texture or script, you must commit the meta file along with it, or the reference will break for other developers.

---

## workflow Git & Contribution Guidelines

To maintain code quality and stability, we enforce strict branching and push policies.

### ⛔ Push Policies (Main Branch Protected)
* The `main` branch is **protected**.
* **Direct pushes to `main` are disabled.**
* All changes must be reviewed via a Pull Request (PR) before merging.

### 🌿 Branch Naming Convention
You must create a new branch for every task or bug fix. Do not work directly on the main branch.

**Format:** `category/task-name`
* `feature/player-movement`
* `fix/ui-scaling-bug`
* `art/forest-level-design`

### 🔄 Development Workflow
1.  **Update your local main:**
    ```bash
    git checkout main
    git pull origin main
    ```
2.  **Create a new branch:**
    ```bash
    git checkout -b feature/your-task-name
    ```
3.  **Work and Commit:**
    Make small, frequent commits. Describe what you changed clearly.
4.  **Push your branch:**
    ```bash
    git push origin feature/your-task-name
    ```
5.  **Create a Pull Request (PR):**
    Go to the repository page (GitHub) and open a Pull Request targeting `main`.
6.  **Code Review & Approval:**
    * **Approval is required** from at least one other team member.
    * Resolve any conflicts or requested changes.
7.  **Merge:**
    Once approved, the branch can be merged into `main`.

---
---

# 🇵🇱 Polska Wersja

## 🎮 Przegląd Projektu
To jest projekt gry w Unity. Upewnij się, że posiadasz zainstalowaną odpowiednią wersję Unity przed otwarciem projektu, aby uniknąć problemów z serializacją.

* **Wersja Unity:** `6000.X.XXXX`
* **Render Pipeline:** URP

### 🚀 Jak zacząć
1.  Sklonuj repozytorium.
2.  Otwórz **Unity Hub**.
3.  Kliknij **Add** (Dodaj) i wybierz folder zawierający to repozytorium.
4.  Otwórz projekt.

> **Ważne:** Zawsze commituj pliki `.meta`! Każdy asset w Unity posiada odpowiadający mu plik `.meta`. Jeśli dodajesz teksturę lub skrypt, musisz wysłać plik meta razem z nim, w przeciwnym razie referencje zostaną zerwane u innych programistów.

---

## 🛡️ Git Workflow i Zasady Współpracy

Aby utrzymać wysoką jakość kodu i stabilność projektu, stosujemy ścisłe zasady dotyczące branchy i wypychania zmian (push).

### ⛔ Polityka Push (Zabezpieczony Main)
* Gałąź `main` jest **chroniona (protected)**.
* **Bezpośredni push do `main` jest zablokowany.**
* Wszystkie zmiany muszą zostać sprawdzone poprzez Pull Request (PR) przed scaleniem.

### 🌿 Nazewnictwo Branchy (Gałęzi)
Musisz utworzyć nowy branch dla każdego zadania lub poprawki błędu. Nie pracuj bezpośrednio na branchu main.

**Format:** `kategoria/nazwa-zadania`
* `feature/ruch-gracza`
* `fix/blad-skalowania-ui`
* `art/poziom-las`

### 🔄 Cykl Pracy (Workflow)
1.  **Zaktualizuj lokalny main:**
    ```bash
    git checkout main
    git pull origin main
    ```
2.  **Stwórz nowy branch:**
    ```bash
    git checkout -b feature/twoja-nazwa-zadania
    ```
3.  **Praca i Commit:**
    Rób małe, częste commity. Opisuj jasno, co zostało zmienione.
4.  **Wypchnij (Push) branch:**
    ```bash
    git push origin feature/twoja-nazwa-zadania
    ```
5.  **Stwórz Pull Request (PR):**
    Przejdź na stronę repozytorium (GitHub/GitLab/Bitbucket) i otwórz Pull Request celujący w `main`.
6.  **Code Review i Zatwierdzenie:**
    * **Wymagane jest zatwierdzenie (Approve)** przez co najmniej jednego członka zespołu.
    * Rozwiąż ewentualne konflikty lub wprowadź wymagane poprawki.
7.  **Merge (Scalenie):**
    Po zatwierdzeniu branch może zostać scalony z `main`.
