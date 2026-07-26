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

### ADR-005 — Mundo: dados, geração e renderização separados
**Decisão:** o mundo é dividido em três responsabilidades desacopladas:
- `WorldGrid` — **dados puros** (sem `MonoBehaviour`/Tilemap): estado e HP dos blocos,
  regras de dano. Testável isoladamente.
- `WorldGeneratorSO` — **estratégia de geração** como ScriptableObject. `FlatWorldGenerator`
  é a impl. de referência; a Fase 4 adiciona `ProceduralWorldGenerator` **sem tocar** no resto.
- `IWorldView` / `TilemapWorldRenderer` — **renderização** trocável (Tilemap hoje).

`WorldController` costura os três e é a **única autoridade** que altera terreno, publicando
`BlockDamagedEvent`/`BlockDestroyedEvent` no EventBus.
**Por quê:** permite testar mineração sem engine, trocar algoritmo de geração e backend de
render sem efeito colateral, e mantém `MiningSystem` alheio a Tilemap.

### ADR-006 — Tiles coloridos gerados em runtime
`TilemapWorldRenderer` cria um `Tile` por `BlockDefinition` usando um sprite branco 1×1
tingido pela cor do bloco (flyweight), quando não há tile autorado. Assim a mina é **visível
e colidível sem nenhuma arte**, acelerando a iteração de gameplay antes do pipeline de pixel art.

### ADR-007 — Mineração dirigida pela picareta + custo de energia
`MiningSystem` (no player) faz a ponte input→mundo: mira pelo ponteiro, respeita o **alcance**
da picareta, gasta **energia por golpe** e usa a cadência/dano da `PickaxeDefinition`. Picareta
de nível abaixo da **Dureza** do bloco fica mais lenta (regra "muito lenta"), não impossível.
Drops nascem aqui de forma mínima na Fase 3; a Fase 10 (Loot) move para um `LootSpawner` que
escuta `BlockDestroyedEvent`.

### ADR-008 — Geração procedural: 3 algoritmos + conectividade garantida
**Decisão:** `ProceduralWorldGenerator : WorldGeneratorSO` combina:
- **Cellular Automata** para cavernas orgânicas, **gated por uma máscara Perlin** de baixa
  frequência — assim as cavernas ficam em bolsões e a mina continua majoritariamente sólida
  (fiel à fantasia escavável); a maior parte do mundo é aberta **cavando**, não pré-gerada.
- **Veios de minério por profundidade** via `OreSpawnRule` (Perlin por regra; faixa de
  profundidade normalizada 0→1; limiar = raridade). Regras avaliadas em ordem (raras primeiro).
- **Passo de conectividade**: flood-fill (BFS) rotula regiões abertas e **corredores em L**
  ligam cada região isolada ao spawn — nunca um mapa impossível ("garantir conectividade").
**Alternativas:** BSP puro para salas (mais estruturado, menos orgânico — mantido como opção
futura para ruínas), drunkard-walk (menos controlável).
**Por quê:** cada semente gera um mapa único e justo; e como o seam da Fase 3 já existia, este
gerador entrou **sem alterar** `WorldController`, renderer ou mineração — basta trocar o asset.

> Baús/eventos/ruínas são gravados como `WorldFeature` (marcadores por célula) em
> `WorldGrid.Features`; as fases de Loot/Eventos/NPCs leem essa lista e instanciam as entidades.

### ADR-009 — Inventário: modelo puro + UI separada (assembly próprio)
**Decisão:** o inventário é dividido em:
- **Modelo** (`Inventory`, `ItemStack`, `ItemDefinition`) — C# puro no assembly `Gameplay`,
  testável e serializável (pronto para o Save). Regras (stack, peso, `MoveOrSwap`) vivem aqui.
- **UI** (`InventoryView`, `InventorySlotUI`, `InventoryTooltip`) — assembly próprio
  **`DeepDigger.UI`** (uGUI), pura apresentação, observa o modelo via eventos `SlotChanged`/`Changed`.
**Por quê:** o loop *minerar → guardar* funciona e é testável **sem UI**; e um erro de versão de
package de UI isola-se no assembly `DeepDigger.UI` sem afetar modelo/mineração.
**Notas:** usei `UnityEngine.UI` (uGUI) + `Text` legado para não depender do setup de TMP Essentials
neste início (upgrade para TextMeshPro é local); a UI assume canvas *Screen Space – Overlay*.
O drag&drop usa os handlers de `EventSystems` + um ícone-fantasma que segue o cursor.

> ⚠️ **Namespace `Inventories` (plural), não `Inventory`:** a classe modelo se chama `Inventory`;
> um namespace `...Inventory` faria `Inventory` resolver para o namespace (CS0118) em código
> irmão de gameplay. Mesma regra do `Cameras` (ver Convenção de namespaces).

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

### Setup adicional da Fase 3 (mundo/mineração)

1. **BlockDefinitions:** em `Assets/ScriptableObjects/` → *Create → Deep Digger → World →
   Block Definition*. Crie ao menos um bloco de preenchimento (ex.: `Block_Stone`, Vida 2,
   Dureza 0, cor cinza) e opcionalmente um `Block_Border` (categoria *Indestructible*).
   Crie também minérios (Ferro Vida 5, Ouro Vida 10 Dureza 2, Adamantita Vida 50 Dureza 4…).
2. **Pickaxe:** *Create → Deep Digger → World → Pickaxe Definition* (ex.: `Pickaxe_Wood`,
   Tier 0, Dano 1, Swing 0.35, Alcance 1.8).
3. **Gerador:** *Create → Deep Digger → World → Generators → Flat*. Atribua `fillBlock`
   (e `borderBlock`), defina largura/altura.
4. **Mundo na cena:** crie um GameObject `World` com um componente **Grid** (cell size
   `1,1,0`) e o `WorldController`. Como filho, um GameObject `Tilemap` com:
   `Tilemap` + `TilemapRenderer` + `TilemapWorldRenderer` e (recomendado) `TilemapCollider2D`
   + `CompositeCollider2D` + `Rigidbody2D` (Body Type = *Static*, marque *Used By Composite*
   no TilemapCollider2D) para colisão com o jogador.
5. No `WorldController`: atribua o **gerador**, e (opcional) arraste o **Player** em
   *Player To Place* para nascer no bôlsão inicial.
6. **Player:** adicione o componente `MiningSystem` e atribua `InputReader`, `PlayerAim`,
   `EnergySystem`, `WorldController` (ou deixe achar sozinho) e a `PickaxeDefinition`.
7. **Play:** segure o **clique esquerdo** mirando um bloco dentro do alcance para minerar;
   blocos quebram, somem (com colisão) e a energia cai a cada golpe.

### Setup adicional da Fase 4 (geração procedural)

1. Crie os `BlockDefinition` dos minérios/perigos que quiser distribuir (ex.: `Block_Iron`,
   `Block_Gold`, `Block_Ruby`, `Block_Crystal`, `Block_Lava` com categoria *Hazard*).
2. *Create → Deep Digger → World → Generators → Procedural*. Atribua `baseRock`/`borderBlock`
   e preencha a lista **Ore Rules** — uma entrada por minério, com faixa de profundidade
   (0 = topo, 1 = fundo) e limiar (maior = mais raro). Ex.: Ferro `0.0–0.5`, Ouro `0.4–0.8`
   (limiar alto), Cristal `0.8–1.0`. **Coloque os mais raros/profundos no topo da lista.**
3. No `WorldController`, troque o campo **Generator** pelo `ProceduralWorldGenerator`
   (deixe *Randomize Seed* marcado para um mapa novo a cada partida). Nada mais muda.
4. **Play:** cada partida gera uma mina única — cavernas orgânicas conectadas, veios de
   minério mais valiosos quanto mais fundo, e marcadores de baú/evento prontos para as
   próximas fases.

### Setup adicional da Fase 5 (inventário)

1. **Package:** o `com.unity.ugui` foi adicionado ao manifest; ao abrir, o Package Manager
   resolve (e importa TextMeshPro junto — não é obrigatório usar TMP nesta fase).
2. **Itens:** *Create → Deep Digger → Items → Item Definition* para cada minério (ícone,
   descrição, stack, peso). Ex.: `Item_Iron`, `Item_Gold`.
3. **Ligar minério → item:** em cada `BlockDefinition` de minério, arraste o `ItemDefinition`
   correspondente no campo **Drop Item**. Ao minerar, o item entra direto no inventário.
4. **Player:** adicione o componente `InventoryComponent` (defina colunas/linhas/peso). O
   `MiningSystem` acha o inventário sozinho (mesmo GameObject) ou arraste no campo *Inventory*.
5. **UI (uGUI, canvas Screen Space – Overlay):**
   - Um `Canvas` com `GraphicRaycaster` e um `EventSystem` na cena.
   - Painel do inventário com um `GridLayoutGroup` (o *slotParent*).
   - **Prefab de slot:** um `Image` de fundo (*Raycast Target* ✔) + `InventorySlotUI`, com um
     `Image` filho para o ícone e um `Text` para a quantidade (referências atribuídas no prefab).
   - Um `Image` "dragIcon" no topo do canvas com *Raycast Target* **desmarcado**.
   - Um painel de `InventoryTooltip` (root + textos de nome/descrição/detalhe).
   - Adicione `InventoryView` e ligue *source*, *slotParent*, *slotPrefab*, *dragIcon*, *tooltip*.
6. **Play:** minere para encher a mochila; arraste itens entre slots (merge/troca) e passe o
   mouse para ver o tooltip.

---

## 4. Progresso do roadmap

| Fase | Descrição | Status |
|-----:|-----------|:------:|
| 1 | Projeto, git, packages, estrutura, asmdefs | ✅ |
| 2 | Movimento, Input, Câmera, Energia, Dash | ✅ |
| 3 | Sistema de blocos, mineração, tilemap | ✅ |
| 4 | Geração procedural | ✅ |
| 5 | Inventário | ✅ |
| 6 | Recursos (minérios/itens) | ⏳ próximo |
| 7–20 | Picaretas → Steam | ⬜ |
