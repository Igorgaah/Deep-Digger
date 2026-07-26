# ⛏️ Deep Digger

> Desça o mais fundo possível em uma mina gerada proceduralmente, colete minérios raros, sobreviva a criaturas subterrâneas e volte à superfície para melhorar seus equipamentos antes de tentar uma nova expedição.

**Deep Digger** é um jogo de mineração e exploração *roguelike* top-down, feito na **Unity 6 LTS**. Cada descida é uma aposta: quanto mais fundo você vai, mais riqueza e experiência encontra — mas também mais monstros, escuridão e perigo. A tensão está sempre na mesma decisão: *"Volto agora ou tento encontrar só mais um veio de ouro?"*

---

## 🎮 Loop principal

```text
Entrar na mina
        ↓
Explorar
        ↓
Minerar recursos
        ↓
Encontrar tesouros
        ↓
Enfrentar ou fugir de monstros
        ↓
Escolher continuar descendo
        ↓
Risco aumenta
        ↓
Morrer OU escapar
        ↓
Usar ouro para upgrades
        ↓
Nova descida
```

O ciclo é viciante porque o jogador está sempre pensando: *"Só mais um pouco... talvez encontre um minério melhor."*

---

## 🕹️ Controles

| Ação | Comando |
|------|---------|
| Andar | `WASD` |
| Mirar | Mouse |
| Minerar / Atacar | Clique esquerdo |
| Esquiva | `Espaço` |
| Correr (consome energia) | `Shift` |

---

## 💎 Recursos

| Profundidade | Minérios |
|--------------|----------|
| **Superfície** | Pedra, Carvão, Ferro, Cobre |
| **Mais fundo** | Ouro, Rubi, Safira, Esmeralda |
| **Abismo** | Mithril, Obsidiana, Cristal, Adamantita |

---

## 🌑 Progressão de profundidade

| Profundidade | Área |
|--------------|------|
| `0–100m` | Mina abandonada |
| `100–300m` | Cavernas |
| `300–600m` | Lagos subterrâneos |
| `600–1000m` | Ruínas |
| `1000m+` | Abismo |

Cada área muda **inimigos**, **música**, **iluminação**, **minérios** e **decoração**.

---

## 👹 Monstros

| Região | Criaturas |
|--------|-----------|
| **Superfície** | Rato gigante, Morcego |
| **Cavernas** | Aranhas, Slimes |
| **Profundo** | Golem, Minhoca gigante, Fantasma |
| **Abismo** | Demônios, Criaturas de cristal |

---

## ⛏️ Sistema de mineração

Cada bloco possui **Vida**, **Dureza**, **Tipo** e **Drop**. A ferramenta usada muda a velocidade:

```text
Pedra  — Vida: 2   → Picareta de madeira: 2 golpes
Ouro   — Vida: 10  → Picareta de madeira: muito lenta
                   → Picareta de ferro:   rápida
```

### 🔨 Um diferencial: mina escavável

Em vez de o mapa existir inteiro desde o início, **quase tudo começa como rocha sólida** e o jogador abre seus próprios caminhos. Isso cria decisões interessantes (*"vale a pena cavar até aquele brilho?"*), torna cada partida realmente diferente e combina perfeitamente com a fantasia de ser um minerador — além de abrir espaço para explosivos, terremotos, salas escondidas e ferramentas especiais no futuro.

---

## ⬆️ Upgrades

| Equipamento | Melhorias |
|-------------|-----------|
| **Picareta** | Velocidade, dano, alcance |
| **Lanterna** | Brilho, bateria |
| **Mochila** | Capacidade |
| **Botas** | Velocidade |
| **Armadura** | Defesa |
| **Detector** | Mostra minérios raros |

---

## ⚡ Energia

Tudo consome energia — e comida a recupera.

| Ação | Custo |
|------|-------|
| Minerar | −2 |
| Correr | −1/s |
| Golpear | −5 |

---

## ⚖️ Sistema de risco

Quanto mais fundo o jogador vai:

- ✔ Mais ouro
- ✔ Mais experiência
- ✔ Mais monstros
- ✔ Mais escuridão
- ✔ Menos recursos de cura

A decisão central é sempre: *"Volto agora ou tento encontrar mais um veio de ouro?"*

---

## 🎲 Eventos aleatórios

Durante uma partida pode acontecer: **desmoronamento**, **tremor**, **lago de lava**, **baú escondido**, **comerciante perdido**, **sala secreta**, **elevador antigo** ou **altar misterioso**.

---

## ☠️ Roguelike

Ao morrer, o jogador **perde** minérios e itens temporários, mas **mantém** ouro, upgrades permanentes, novas picaretas e habilidades desbloqueadas. Assim, cada tentativa deixa o jogador mais forte.

---

## 🌳 Árvore de habilidades

```text
Mineração
├── +20% velocidade
├── +50 mochila
└── Chance de mineração dupla

Combate
├── +Vida
├── +Ataque
└── Esquiva

Exploração
├── Mais visão
├── Detector
└── Passos silenciosos
```

---

## 🗺️ Geração procedural

Cada mapa é uma matriz de blocos escaváveis:

```text
################
####.....#######
###........#####
##.....O....####
###.........####
#####....########
```

**Legenda:** `#` = Pedra · `.` = Espaço · `O` = Minério

Sobre essa base são adicionados cavernas, rios, lava, salas, corredores e ruínas.

---

## 🎨 Estilo gráfico

Pixel art 2D (top-down), inspirado em **Core Keeper**, **Forager**, **Terraria** (mineração), **Moonlighter** (visual) e **Enter the Gungeon** (movimentação). Reduz o tempo de produção e combina muito bem com geração procedural.

---

## 🛠️ Tecnologias

Projeto feito na **Unity 6 LTS** (`6000.3.x`):

- **Input System** para os controles
- **Tilemap** para o cenário
- **ScriptableObjects** para minérios, inimigos e equipamentos
- **Object Pooling** para otimizar desempenho
- **Pathfinding** simples (A* / navegação em grid) para inimigos
- **Salvamento em JSON** para progresso permanente

---

## 🗓️ Roadmap (MVP)

| Semana | Foco |
|--------|------|
| **1** | Movimentação, câmera, sistema de mineração, tilemap |
| **2** | Geração procedural, minérios, inventário |
| **3** | Monstros, combate, vida |
| **4** | Loja, upgrades, retorno à superfície |
| **5** | Sons, interface, partículas, polimento |
| **6** | Balanceamento, correção de bugs, versão de teste |

---

## 🚀 Como rodar

1. Instale a **Unity 6 LTS** (`6000.3.10f1` ou compatível) via [Unity Hub](https://unity.com/download).
2. Clone este repositório:
   ```bash
   git clone https://github.com/Igorgaah/Deep-Digger.git
   ```
3. No Unity Hub, clique em **Add** e selecione a pasta do projeto.
4. Abra o projeto e pressione **Play** no Editor.

> As pastas `Library/`, `Temp/`, `Logs/` e `UserSettings/` são geradas automaticamente pela Unity e não fazem parte do versionamento.

---

## 📄 Licença

A definir.
