# TODO do Usuário — Tarefas no Editor Unity (DIU3)

> Executar **depois** que o código das Etapas 4–7 estiver pronto e o Unity compilar sem erros.
> Sem sala secreta. Pickups (essência/cura) ficam espalhados pelo mapa.

---

## 1. ScriptableObjects

- [x] **Transformation Data (Morcego)** — `Create → Game → Transformation Data`
  - Nome: `BatData`
  - `moveSpeedMultiplier`: 1.5 | `duration`: 10 | `damageMultiplierReceived`: 1.2
  - `postTransformationDebuffDuration`: 2 | `postDebuffSpeedMultiplier`: 0.7
  - `canFlyOverObstacles`: ✓ | `attackDamage`: 1 | `attackRange`: 1
- [x] **Transformation Data (Warg)** — mesmo menu, nome `WargData`
  - `moveSpeedMultiplier`: 1 | `duration`: 8 | `hasLeapAttack`: ✓ | `leapForce`: 8
  - `attackDamage`: 2 | `attackRange`: 1.2 | `loseDash`: (opcional)
- [x] **Enemy Essence Reward** × 5 — `Create → Game → Enemy Essence Reward`
  - Zombie=10, Wildfire=25, Slime=8, Orc=15, Troll=100
- [x] **Enemy Config** × 5 — `Create → Game → Enemy Config` (Zombie, Wildfire, Slime, Troll, Orc)
  - Troll: `maxHealth`=500, `damage`=20, `chaseSpeed` baixo (~1)
  - Preencher `enemyType` corretamente em cada config

---

## 2. Player no Inspector (SampleScene)

- [x] Adicionar componentes `TransformationManager` e `PlayerFormController` ao GameObject **Player**
- [x] No `TransformationManager`: arrastar os 5 `EnemyEssenceReward` para a lista `essenceRewards`
- [x] Criar child objects do Player:
  - `FormHuman` (o visual atual do player — reparentar se necessário)
  - `FormBat` (SpriteRenderer + Animator, sprite em `Sprites/Characters/Beasts/Bat/`)
  - `FormWarg` (SpriteRenderer + Animator, sprite em `Sprites/Characters/Beasts/Warg/`)
  - Comece `FormBat` e `FormWarg` desativados (o controller ativa na troca)
- [x] No `PlayerFormController`: arrastar `FormHuman`/`FormBat`/`FormWarg` e os SOs `BatData`/`WargData`

---

## 3. Animators das formas

- [X] Criar `BatAnimator.controller` (FlyIdle, Attack, Dmg, Die) — espelhe a estrutura do `PlayerAnimator`
- [X] Criar `WargAnimator.controller` (Idle, Walk, Attack, Jump, Dmg, Die)
- [x] Atribuir cada controller ao Animator de `FormBat`/`FormWarg`
- [x] (Opcional) Adicionar parâmetros `IsMoving`, `IsAttacking`, `IsTakingDamage`, `IsDead`, `MoveX`, `MoveY` para bater com `AnimationHashes`

---

## 4. Prefabs dos inimigos (5)

> Padrão de referência: `Assets/Prefabs/Orc.prefab` + `Assets/Animations/OrcAnimator.controller`.
> Sprites já estão fatiados (Multi, 32×32). Clips `.anim` e controllers **não existem** para os 4 novos.
> Parâmetros do Animator DEVEM usar os nomes de `AnimationHashes` (senão o script não aciona nada).

### 4.0 Corrigir `enemyType` nos configs (1 min)

Nos 5 EnemyConfig, o campo `enemyType` está `None` (0). Corrigir no Inspector:

| Asset | enemyType |
|-------|-----------|
| `ZombieConfig` | Zombie |
| `WildFireConfig` | Wildfire |
| `SlimeConfig` | Slime |
| `TrollConfig` | Troll |
| `OrcConfig` (ou `EnemyConfigOrc`) | Orc |

(Sem isso a essência por kill cai no default e o lookup fica inconsistente.)

- [x] Configs com `enemyType` correto

### 4.1 Criar os clips `.anim` (uma vez por inimigo)

Pasta: `Assets/Animations/Enemies/{Zombie|Wildfire|Slime|Troll}/`

1. Seletor Project → botão direito → **Create → Animation** (ou Window → Animation → Animation, com o SpriteRenderer do objeto selecionado).
2. Com o asset `.anim` aberto, arrastar **todos os sub-sprites** da sheet correspondente (já fatiados: `ZombieIdle_0…`, etc.) para a timeline — vira keyframes de `m_Sprite`.
3. Ajustar **Sample Rate** (campo em cima da timeline) conforme o `_AnimationInfo.txt` de cada pasta:

| Inimigo | Clips a criar | Sample Rate |
|---------|---------------|-------------|
| **Zombie** | `ZombieIdle`, `ZombieWalk`, `ZombieAttack`, `ZombieDmg`, `ZombieDie` | Idle/Walk = **5** (200ms); Attack/Dmg/Die = **10** (100ms) |
| **Wildfire** | `WildfireIdle`, `WildfireFly`, `WildfireDmg`, `WildfireDie` | Idle/Fly = **5**; Dmg/Die = **10** |
| **Slime** | `SlimeIdle`, `SlimeJumpAttack`, `SlimeDmg`, `SlimeDie` | Idle/Jump = **5**; Dmg/Die = **10** |
| **Troll** | `TrollIdle`, `TrollWalk`, `TrollAttack`, `TrollDmg`, `TrollDie`, `TrollJump` | Idle/Walk = **5**; Attack/Jump/Dmg/Die = **10** |

4. Em cada clip: **Loop Time** ✓ apenas em Idle/Walk/Fly; Attack/Dmg/Die/Jump = Loop ✗.

- [x] Clips do Zombie
- [x] Clips do Wildfire
- [x] Clips do Slime
- [x] Clips do Troll

Eu troquei o Zombie pelo Skeleton porque eu achei ele mais diferente. O comportamento do Skeleton deve ser o mesmo do Zombie.

### 4.2 Criar os Animator Controllers

Pasta: `Assets/Animations/Enemies/` (ou ao lado do `OrcAnimator.controller`).

1. **Create → Animator Controller** → nome: `ZombieAnimator`, `WildfireAnimator`, `SlimeAnimator`, `TrollAnimator`.
2. Em **Parameters**, adicionar (mesma lista para os 4):

| Nome | Tipo |
|------|------|
| `IsMoving` | Bool |
| `MoveX` | Float |
| `MoveY` | Float |
| `Damage` | **Trigger** |
| `Death` | **Trigger** |

> Os scripts escrevem `MoveX`/`MoveY` mesmo em inimigos single-direction; os params precisam existir para não logar warning. Não precisa de blend tree — sheets são para-cima/baixo simples.

3. Em **States**, criar:
   - `Idle` (default) — clip Idle
   - `Walk` (ou `Fly` no Wildfire) — clip Walk/Fly
   - `Damage` — clip Dmg
   - `Death` — clip Die
4. **Transitions**:
   - `Idle` → `Walk`: condição `IsMoving` = true
   - `Walk` → `Idle`: condição `IsMoving` = false
   - **Any State** → `Damage`: trigger `Damage` (✓ Can Transition To Self)
   - `Damage` → `Idle`: **✓ Has Exit Time** (Exit Time ≈ 1, Duration 0.1)
   - **Any State** → `Death`: trigger `Death`
   - `Death`: sem saída
5. (Opcional Troll) se quiser animar o Jump: estado `Jump` ligado a um trigger extra `Attack` — o `EnemyBoss` atual só usa `Damage`/`Death`/`IsMoving`/`MoveX`/`MoveY`, então Jump fica decorativo por enquanto.

- [x] `SkeletonAnimator`
- [x] `WildfireAnimator`
- [x] `SlimeAnimator`
- [x] `TrollAnimator`

### 4.3 Montar cada prefab (repetir 4×)

1. Cena vazia (ou fora do mapa): **GameObject → Create Empty** → nome `Zombie` / `Wildfire` / `Slime` / `Troll`.
2. **Tag = Enemy**, **Layer = Enemy (7)**, **Scale = 10, 10, 10** (mesmo do Orc).
3. Componentes na ordem (Add Component):

| # | Componente | Configuração |
|---|------------|--------------|
| 1 | **Animator** | Controller = o criado no 4.2 |
| 2 | **SpriteRenderer** | Sprite = primeiro frame Idle; **Sorting Order = 1**; Material = sprite-default (igual Orc) |
| 3 | **Rigidbody2D** | Body Type = Dynamic; **Gravity Scale = 0**; Constraints = **Freeze Rotation Z** |
| 4 | **BoxCollider2D** | **Is Trigger ✗** (corpo sólido) — Size ≈ `0.04, 0.04` no scale 10 (ajustar ao sprite) |
| 5 | **BoxCollider2D** (2º) | **Is Trigger ✓** (hitbox do ataque) — Size ≈ `0.08, 0.09`, Offset levemente à frente |
| 6 | Script do inimigo | Zombie→`EnemyZombie` · Wildfire→`EnemyWildfire` · Slime→`EnemySlime` · Troll→`EnemyBoss` |
| 7 | **HealthSystem** | Max Health = valor do config (Zombie/Wildfire/Orc baixo; **Troll = 500**) |
| 8 | **EnemyAttack** | Sem campo exposto (lê damage/cooldown do config via `BaseEnemy`) |

4. No script do inimigo (campo **Config**): arrastar o SO correspondente (`ZombieConfig`, `WildFireConfig`, `SlimeConfig`, `TrollConfig`).
5. Só pode existir **UM** script que herda de `BaseEnemy` por prefab (Zombie/Wildfire/Slime/Boss — **não** colocar `EnemyChase` junto).
6. **Slime**: campo `Respawn Delay` = 5 (default ok).
7. **Troll**: em `EnemyBoss → Orc Prefab`, arrastar `Assets/Prefabs/Orc.prefab`.
8. Selecionar o objeto → **Assets → Create → Prefab** (ou arrastar da Hierarchy para `Assets/Prefabs/`) → salvar como `Zombie.prefab`, `Wildfire.prefab`, `Slime.prefab`, `Troll.prefab`.
9. Apagar o objeto da cena (o Troll volta no 4.4).

- [x] `Zombie.prefab`
- [x] `Wildfire.prefab`
- [x] `Slime.prefab`
- [x] `Troll.prefab`

### 4.4 Atualizar Orc + colocar na cena

- [x] `EnemyConfigOrc` / `OrcConfig`: `enemyType` = **Orc** (se ainda estiver None)
- [x] `Orc.prefab` já está ok (EnemyChase + HealthSystem + EnemyAttack + 2 colliders) — só conferir config
- [ ] Arrastar **Troll.prefab** para a posição fixa do Boss na SampleScene (fora dos spawn points)
- [ ] Conferir no Troll da cena: `Orc Prefab` preenchido

### 4.5 Checklist rápido de sanidade (por prefab)

- [x] Tag `Enemy`, Layer 7, escala 10
- [x] Animator controller certo (params `IsMoving`/`Damage`/`Death` presentes)
- [x] 2 BoxCollider2D: um sólido + um isTrigger
- [x] Rigidbody2D gravity 0 + freeze rot Z
- [x] Script BaseEnemy com Config SO arrastado
- [x] HealthSystem presente; EnemyAttack presente
- [x] Prefab salvo em `Assets/Prefabs/`

---

## 5. SpawnManager (SampleScene)

- [ ] No `SpawnManager`: arrastar os prefabs para o array `enemyPrefabs` (Zombie, Wildfire, Slime — pesos conforme desejar)
- [ ] Ajustar `spawnPoints` se necessário

---

## 6. UI (Canvas único)

- [ ] Criar um GameObject `UI` vazio na cena com o componente `UIManager`
- [ ] Criar child panels (todos inactive exceto HUD, exceto quando UIManager mostrar):

| Panel | Elementos | Script |
|-------|-----------|--------|
| `HUD` | Slider HP, Slider Essência, Texto/TMP nome da forma, Texto timer | `HUDController` |
| `MainMenu` | Título + botão Start | `MainMenuUI` |
| `Pause` | botões Continuar/Reiniciar/Sair | `PauseMenuUI` |
| `GameOver` | botões Reiniciar/Sair | `GameOverUI` |
| `Victory` | botões Reiniciar/Sair | `VictoryUI` |

- [ ] Atribuir todas as referências (sliders, textos, botões) nos scripts
- [ ] Ligar botões no OnClick → métodos públicos dos scripts
- [ ] Remover o componente `HealthHUD` antigo do Canvas da cena (HUDController substitui)

---

## 7. Coletáveis espalhados

- [ ] Criar prefab `EssencePickup`: SpriteRenderer + Collider2D (isTrigger) + script `EssencePickup` (ex: 50 essência)
- [ ] Criar prefab `HealthPickup`: mesmo padrão + `HealthPickup` (ex: cura 25)
- [ ] Espalhar 3–5 de cada pelo mapa (sem sala secreta)

---

## 8. Verificação (Etapa 8)

Após tudo acima, jogar e validar:

- [ ] Zombie persegue com alcance curto
- [ ] Wildfire persegue → colide → morre; **NÃO** persegue se player transformado
- [ ] Slime morre → respawna após delay
- [ ] Troll invoca Orcs com HP < 50%
- [ ] Essência enche com qualquer abate
- [ ] Transformação consome 50 e dura tempo fixo (10s Morcego / 8s Warg)
- [ ] Morcego voa sobre abismos
- [ ] Warg faz leap com knockback
- [ ] Pickups de essência/cura funcionam
- [ ] Pause congela o jogo (ESC/P)
- [ ] Game Over e Vitória funcionam
- [ ] HUD atualiza em tempo real

---

**Ordem sugerida:** 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8.
