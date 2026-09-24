# Plano de Implementação — DIU3: Metamorfose e Sistema de Recompensas

## Resumo do Projeto

Jogo 2D Top-Down casual com mecânicas de **Absorção e Metamorfose**. O jogador coleta essências derrotando inimigos, transformando-se em criaturas com vantagens e desvantagens para superar obstáculos e derrotar o Boss Final.

---

## Definições de Design

### Formas de Transformação

| Tecla | Forma | Custo | Duração | Vantagem | Debuff |
|-------|-------|-------|---------|----------|--------|
| 1 | Humano | — | — | Base | — |
| 2 | Morcego | 50 essência | 10s | +50% velocidade, voa sobre abismos | +20% dano recebido; exaustão 2s pós-form (-30% vel) |
| 3 | Warg | 50 essência | 8s | Leap attack com knockback, +50% dano corpo a corpo | -20% vida máxima |

### Sistema de Essências

- Barra universal (0-100)
- Cada inimigo derrotado adiciona essência (valor configurável via ScriptableObject)
- Transformação consome 50 essência de uma vez
- Volta ao humano automaticamente ao acabar o tempo

### Inimigos

| Inimigo | Comportamento | Essência |
|---------|---------------|----------|
| Zombie | Chase + Wander, detecção curta | 10 |
| Wildfire | Kamikaze: persegue → colide → dano alto → morre. NÃO persegue se jogador transformado | 25 |
| Slime | Wander lento, dano por colisão, respawna após morte | 8 |
| Troll (Boss) | HP alto, dano alto, lento, invoca Orcs quando HP < 50% | 100 |
| Orc (Minions) | Chase + Wander, spawnados pelo Boss | 15 |

### Boss Final

- **Troll** com HP alto (500), dano alto (20), velocidade muito baixa
- Invoca 3-5 Orcs quando HP < 50%
- Ataque corpo a corpo com AoE

### Sprites dos Inimigos

| Inimigo | Pasta | Animações |
|---------|-------|-----------|
| Zombie | Undead/Zombie/ | Idle, Walk, Attack, Dmg, Die |
| Wildfire | Undead/Wildfire/ | Idle, Fly, Dmg, Die |
| Slime | Slimes/Green_Slime/ | Idle, JumpAttack, Dmg, Die |
| Troll | Monsters/Troll/ | Idle, Walk, Attack, Dmg, Die, Jump |
| Orc | Orc/ | Idle, Walk, Attack, ChargedAttack, Dmg, Die, Jump |

### Sprites das Transformações

| Forma | Pasta | Animações |
|-------|-------|-----------|
| Morcego | Beasts/Bat/ | FlyIdle, Attack, Dmg, Die, Sleep |
| Warg | Beasts/Warg/ | Idle, Walk, Attack, Jump, Dmg, Die |

### UI

- Cena única + overlays via UIManager singleton
- Main Menu → Gameplay → Pause/Game Over/Victory
- Pause congela o jogo (Time.timeScale = 0)
- HUD: HP bar, barra de essência, indicador de transformação + timer

### Controles de Transformação

- Tecla 1 = Humano
- Tecla 2 = Morcego
- Tecla 3 = Warg
- Tilemap: construção manual no Editor

---

## Etapas de Implementação

### Etapa 1 — ScriptableObjects e Enums

**Objetivo:** Criar a base de dados que todo o resto vai referenciar.

**Arquivos novos:**
- `Assets/Scripts/Config/TransformationData.cs` — SO com atributos de cada forma
- `Assets/Scripts/Config/EnemyType.cs` — Enum: None, Zombie, Wildfire, Slime, Troll, Orc
- `Assets/Scripts/Config/EnemyEssenceReward.cs` — SO: enemyType + essenceValue

**Arquivos modificados:**
- `Assets/Scripts/Config/EnemyConfig.cs` — Adicionar campo `EnemyType enemyType`

**Critério de conclusão:** Scripts compilam sem erro, SOs podem ser criados no Editor via menu.

---

### Etapa 2 — Sistema de Essências (TransformationManager)

**Objetivo:** Barra universal de essência que enche com abates.

**Arquivos novos:**
- `Assets/Scripts/Player/TransformationManager.cs`
  - `int currentEssence` (0-100)
  - `void AddEssence(int amount)`
  - `bool CanTransform()` → currentEssence >= 50
  - `void ConsumeTransformation()` → currentEssence -= 50
  - Evento `OnEssenceChanged(int current, int max)` → para a UI

**Critério de conclusão:** EssenceManager funcional em isolado (teste via Console).

---

### Etapa 3 — Controller de Formas do Player (PlayerFormController)

**Objetivo:** Gerenciar as visualizações das transformações via child GameObjects.

**Arquivos novos:**
- `Assets/Scripts/Player/PlayerFormController.cs`
  - Referências serializadas: `GameObject formHuman`, `formBat`, `formWarg`
  - `TransformationData[] formsData` (array dos SOs)
  - `void ActivateForm(int index)` → desativa todas, ativa a indexada
  - `void DeactivateAllForms()` → volta ao humano
  - `int CurrentFormIndex` (propriedade)

**Critério de conclusão:** Forms trocam visualmente ao chamar ActivateForm() via Inspector.

---

### Etapa 4 — Transformação do Player

**Objetivo:** Integrar transformação no Player com inputs, timers e debuffs.

**Arquivos modificados:**
- `Assets/Scripts/Player/Player.cs`
  - Adicionar: `TransformationManager`, `PlayerFormController`
  - `TransformationData currentForm`, `float transformationTimer`, `float debuffTimer`
  - `bool IsTransformed` (propriedade pública)
  - Input `OnFormChange(InputValue)` → teclas 1/2/3
  - `TryTransform(int formIndex)` → verifica essência, consome, aplica
  - `ApplyForm(TransformationData)` → ativa child, ajusta stats, inicia timer
  - `RevertForm()` → desativa child, volta stats, aplica debuff
  - `OnDeath()` → chama `UIManager.Instance.ShowGameOver()`
  - Movement: aplica `moveSpeedMultiplier` e `movementSpeedPenalty`

- `Assets/Scripts/Player/PlayerAttack.cs`
  - Propriedades públicas `int AttackDamage` e `float AttackRange` (getter/setter dinâmico)

**Arquivos novos:**
- `Assets/Scripts/States/PlayerStates/PlayerTransformedState.cs`

**Critério de conclusão:** Player transforma em Morcego (voa) e Warg (leap), volta ao humano após duração, debuff aplica.

---

### Etapa 5 — Inimigos Variantes

**Objetivo:** Criar os 5 tipos de inimigo com comportamentos distintos.

**Arquivos novos:**
- `Assets/Scripts/Enemies/EnemyZombie.cs` — Chase + Wander, detecção curta (copia/adapta EnemyChase)
- `Assets/Scripts/Enemies/EnemyWildfire.cs` — Kamikaze: persegue → colide → dano alto → morre. Checa `Player.IsTransformed`
- `Assets/Scripts/Enemies/EnemySlime.cs` — Wander lento, dano por colisão, respawna após morte (coroutine)
- `Assets/Scripts/Enemies/EnemyBoss.cs` — Troll: HP alto, lento, invoca Orcs quando HP < 50%

**Arquivos modificados:**
- `Assets/Scripts/Enemies/SpawnManager.cs` — Suporte a tipos variados de inimigo

**Critério de conclusão:** Todos os inimigos funcionam com comportamentos esperados, Boss invoca minions.

---

### Etapa 6 — UI Completa

**Objetivo:** Menus, pause, game over, vitória e HUD.

**Arquivos novos:**
- `Assets/Scripts/UI/UIManager.cs` — Singleton, gerencia painéis Canvas
- `Assets/Scripts/UI/MainMenuUI.cs` — Tela inicial → Start
- `Assets/Scripts/UI/PauseMenuUI.cs` — ESC/P → Time.timeScale = 0; Continuar/Reiniciar/Sair
- `Assets/Scripts/UI/GameOverUI.cs` — Derrota → Reiniciar/Sair
- `Assets/Scripts/UI/VictoryUI.cs` — Vitória → Reiniciar/Sair
- `Assets/Scripts/UI/HUDController.cs` — HP bar, barra de essência, indicador de transformação + timer

**Arquivos removidos/substituídos:**
- `Assets/Scripts/HealthHUD.cs` → integrado no HUDController

**Critério de conclusão:** Fluxo completo: Main Menu → Gameplay → Pause/Game Over/Victory, HUD atualiza em tempo real.

---

### Etapa 7 — Collectibles Espalhados

**Objetivo:** Coletáveis espalhados pelo mapa (sem sala secreta — sala removida do escopo).

**Arquivos novos:**
- `Assets/Scripts/Collectibles/EssencePickup.cs` — Coletável que adiciona essência (ex: 50 essência = 1 transformação garantida)
- `Assets/Scripts/Collectibles/HealthPickup.cs` — Coletável que cura o jogador

**Critério de conclusão:** Coletáveis espalhados pelo mapa concedem essência ou cura ao encostar.

---

### Etapa 8 — Integração e Testes

**Objetivo:** Validar todo o sistema junto.

**Checklist:**
- [ ] Zombie persegue com alcance curto
- [ ] Wildfire persegue → colide → morre; NÃO persegue se transformado
- [ ] Slime morre → respawna após delay
- [ ] Troll Boss invoca Orcs com HP < 50%
- [ ] Essência universal enche com qualquer abate
- [ ] Transformação consome 50 e dura tempo fixo
- [ ] Morcego voa sobre abismos
- [ ] Warg faz leap com knockback
- [ ] Pickups de essência/cura funcionam
- [ ] Pause congela o jogo
- [ ] Game Over e Vitória funcionam
- [ ] HUD atualiza em tempo real

---

## Resumo de Arquivos

| Etapa | Novos | Modificados |
|-------|-------|-------------|
| 1 | TransformationData.cs, EnemyType.cs, EnemyEssenceReward.cs | EnemyConfig.cs |
| 2 | TransformationManager.cs | — |
| 3 | PlayerFormController.cs | — |
| 4 | PlayerTransformedState.cs | Player.cs, PlayerAttack.cs |
| 5 | EnemyZombie.cs, EnemyWildfire.cs, EnemySlime.cs, EnemyBoss.cs | SpawnManager.cs |
| 6 | UIManager.cs, MainMenuUI.cs, PauseMenuUI.cs, GameOverUI.cs, VictoryUI.cs, HUDController.cs | HealthHUD.cs (substituído) |
| 7 | EssencePickup.cs, HealthPickup.cs | — |
| 8 | — | Integração |

**Total: ~17 scripts novos, ~5 modificados**

> **Nota:** `SecretRoomTrigger.cs` (sala secreta) foi removido do escopo a pedido do usuário. `EssencePickup` e `HealthPickup` ficam como coletáveis espalhados pelo mapa. Ver também `TODO_USUARIO.md` para as tarefas manuais no Editor.
