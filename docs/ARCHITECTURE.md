# Deep Digger — Arquitetura & Decisões Técnicas

Documento vivo. Registra as decisões arquiteturais (ADRs), a convenção de código e o
checklist de setup no Editor. Atualizado a cada fase do roadmap.

---

## 1. Visão geral

O código é organizado em **assemblies** (`.asmdef`) para reduzir tempo de compilação,
forçar dependências unidirecionais e permitir testes isolados.

```
DeepDigger.Core       ← sem dependências de gameplay (infra pura)
      ▲
      │
DeepDigger.Gameplay   ← depende de Core e do Input System
```

Assemblies planejados para as próximas fases: `DeepDigger.UI`, `DeepDigger.Managers`,
`DeepDigger.Data` (ScriptableObjects), `DeepDigger.Save`. Serão adicionados quando o
primeiro código de cada domínio existir (evitar assemblies vazios — YAGNI).

### Convenção de namespaces

`DeepDigger.<Assembly>.<Domínio>` — ex.: `DeepDigger.Gameplay.Player`.

> ⚠️ **Regra:** nunca nomear um namespace igual a um tipo comum da engine.
> A pasta de câmera usa o namespace `DeepDigger.Gameplay.Cameras` (plural) de propósito:
> `...Camera` colidiria com `UnityEngine.Camera` e quebraria a resolução de `Camera` em
> namespaces irmãos (erro `CS0118`). Pelo mesmo motivo, **usamos exclusivamente o novo
> Input System** e nunca a classe legada `UnityEngine.Input`.

---

## 2. ADRs (Architecture Decision Records)

### ADR-001 — Event Bus estático e tipado
**Decisão:** comunicação desacoplada via `EventBus` (Core), com eventos tipados por
`IEvent` implementado em `readonly struct` (zero alocação/boxing).
**Alternativas:** `UnityEvent` (acopla no inspector, difícil de rastrear) · C# `event`
direto (acopla publisher↔subscriber) · bus por instância + Service Locator.
**Por quê:** notificações globais (energia, shake, loot, morte) precisam de N→N sem
referências diretas. O reset via `[RuntimeInitializeOnLoadMethod]` mantém o estado
correto mesmo com *domain reload* desligado.
**Regra de uso:** **comandos** (fazer algo) = chamada direta de método; **notificações**
(algo aconteceu) = `EventBus`.

### ADR-002 — Input via `InputReader` (ScriptableObject, ações em código)
**Decisão:** um `ScriptableObject` expõe a intenção do jogador (MoveInput, eventos de
Dash/Attack/Sprint/Interact). As `InputAction` são criadas em código.
**Alternativas:** asset `.inputactions` + wrapper gerado (melhor para rebind visual, mas
depende da geração do Editor e de um asset autorado) · `PlayerInput` component (menos
controle, mais "mágica").
**Por quê:** desacopla todo o gameplay do Input System (só o `InputReader` o conhece),
compila e roda sem asset autorado, e a migração futura para `.inputactions` fica isolada
nesta única classe.

### ADR-003 — Câmera própria (`CameraFollow`) em vez de Cinemachine
**Decisão:** follow com `SmoothDamp` + `CameraShake` (offset aditivo) neste início.
**Alternativas:** Cinemachine (poderoso, mas adiciona dependência/pacote e setup de
virtual cameras no Editor logo de cara).
**Por quê:** manter o começo *dependency-free* e 100% dirigido por código. `CameraFollow`
pode ser trocado por uma virtual camera do Cinemachine depois **sem tocar** no gameplay.

### ADR-004 — `Active Input Handling = Both`
Habilitado no `ProjectSettings` (`activeInputHandler: 2`) para o novo Input System
funcionar; "Both" evita quebrar qualquer código/pacote que ainda espere o backend legado.

---

## 3. Setup no Editor (necessário uma vez para rodar a Fase 2)

O código foi entregue completo, mas o Unity precisa de alguns assets/cena montados no
Editor. Passos (poucos cliques):

1. **Abrir o projeto** no Unity Hub (Unity `6000.3.10f1`). Na primeira abertura o Package
   Manager resolve o **Input System** (`com.unity.inputsystem`). Se aparecer aviso para
   reiniciar habilitando o novo backend, aceite (já deixamos `Active Input Handling = Both`).
2. **Criar o asset de Input:** `Assets/ScriptableObjects/` → botão direito → *Create →
   Deep Digger → Input Reader*. Nomeie `InputReader`.
3. **Cena:** abra/crie uma cena em `Assets/Scenes/` (ex.: `Game.unity`).
4. **Player (GameObject):**
   - Adicione `Rigidbody2D` → *Gravity Scale* = `0`, *Freeze Rotation Z* = ✔, *Collision
     Detection* = Continuous, *Interpolate* = Interpolate.
   - Adicione um `Collider2D` (ex.: `CapsuleCollider2D`).
   - Adicione os componentes `EnergySystem`, `PlayerController`, `PlayerAim`.
   - No `PlayerController`, arraste o asset **InputReader** e (opcional) o `EnergySystem`.
   - No `PlayerAim`, arraste o **InputReader** (a câmera é pega de `Camera.main` se vazia).
5. **Câmera:** na *Main Camera* adicione `CameraShake` e `CameraFollow`; no `CameraFollow`
   defina *Target* = Player. Deixe a *Main Camera* com a tag `MainCamera`.
6. **Play.** WASD anda, `Shift` corre (gasta energia), `Espaço` dá dash, mouse mira.

> Assim que forem criados prefabs/cenas, **comite também os arquivos `.meta`** gerados
> pelo Unity (eles guardam os GUIDs que ligam os assets). Os `.meta` já entram no git.

---

## 4. Progresso do roadmap

| Fase | Descrição | Status |
|-----:|-----------|:------:|
| 1 | Projeto, git, packages, estrutura, asmdefs | ✅ |
| 2 | Movimento, Input, Câmera, Energia, Dash | ✅ |
| 3 | Sistema de blocos, mineração, tilemap | ⏳ próximo |
| 4 | Geração procedural | ⬜ |
| 5 | Inventário | ⬜ |
| 6–20 | Recursos → Steam | ⬜ |
