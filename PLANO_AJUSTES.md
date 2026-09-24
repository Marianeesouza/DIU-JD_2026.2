# Plano de Ajustes — DIU_JD2026.1

Verificação geral de bugs e melhorias. Este documento acompanha a execução (✅ feito / ⏳ pendente).

## Decisões confirmadas
- **Mira do ataque:** voltar ao design — direção pelo **mouse cursor** (não pelo movimento).
- **Escopo:** todas as fases (Fase 1 + 2 + 3).

## Arquivos-alvo
`Player.cs`, `PlayerAttack.cs`, `UIManager.cs`, `EnemyChase.cs`, `EnemySlime.cs`, `BaseCharacter.cs`, `PlayerStates/*`, `EnemyZombie.cs`, `EnemyWildfire.cs`, `EnemyBoss.cs`, `SpawnManager.cs`, `EnemyAttack.cs`, cena `SampleScene.unity`, prefabs Orc/Skeleton/Slime/Troll/Wildfire (se necessário).

---

## Fase 1 — Congelamentos e erros (crítico)

### 1.1 Evitar soft-lock `isAttacking` na troca de forma
**Arquivo:** `Assets/Scripts/Player/Player.cs`
- Em `ApplyForm()` e `RevertForm()`, antes de trocar o Animator/forma: chamar `OnAttackEnd()` e `playerAttack?.DisableHitbox()` se `isAttacking`.
- Defesa em profundidade: forçar finalização (permitir transformar durante ataque, sem travar).

### 1.2 Fallback de timeout para Animation Events ausentes
**Arquivo:** `Player.cs`
- Timer de segurança `attackEndFallback` (2s): se `isAttacking` e o evento não chegou, forçar `OnAttackEnd()` + `DisableHitbox()`.
- Mesma proteção para `OnDeathEnd`: se morte não disparar o evento em ~3s, chamar `ShowGameOver()`/reload por código.

### 1.3 Hitbox presa após dano cancelar ataque
**Arquivo:** `Player.cs` → `HandleDamageTaken()`
- Chamar `playerAttack?.DisableHitbox()` ao cancelar o ataque por dano.

### 1.4 Input legado no UIManager
**Arquivo:** `Assets/Scripts/UI/UIManager.cs`
- Trocar `Input.GetKeyDown(KeyCode.Escape)` por `Keyboard.current.escapeKey.wasPressedThisFrame` (com null-check), pois `activeInputHandler: 1` (só Input System novo).

---

## Fase 2 — Bugs funcionais (inimigos / player / UI)

### 2.1 `EnemyChase` (Orc): essência + coliders
**Arquivo:** `Assets/Scripts/Enemies/EnemyChase.cs` (`HandleDeath`)
- Adicionar `TransformationManager.Instance?.AddEssenceOnKill(GetConfig())`.
- Trocar `GetComponent<Collider2D>().enabled = false` por loop em `GetComponents<Collider2D>()` com null-check (padrão `EnemyWildfire`).

### 2.2 Respawn do Slime preso na animação de morte
**Arquivo:** `Assets/Scripts/Enemies/EnemySlime.cs` (`RespawnRoutine`)
- Ao reativar: `animator.Rebind(); animator.Update(0f);` e limpar triggers (`Death`/`Damage`).

### 2.3 UIManager: assinar eventos + barras de HUD
**Arquivo:** `UIManager.cs` + cena
- Em `Start()`: localizar `HealthSystem` do Player e `TransformationManager.Instance`; assinar `OnHealthChanged → UpdateHealthBar` e `OnEssenceChanged → UpdateEssenceBar`; desinscrever em `OnDestroy`.
- Aplicar estado inicial.
- **Cena:** adicionar GameObject `UIManager` em `SampleScene.unity` (não existe hoje).

### 2.4 Perda de HP ao ciclar transformação
**Arquivo:** `Player.cs` (`ApplyForm`/`RevertForm`) + `HealthSystem.cs`
- Guardar estado pré-transformação; ao reverter, restaurar `currentHealth` somando o max perdido (`min(baseMax, current + maxPerdido)`).
- Adicionar `HealthSystem.SetCurrentHealth(int)`.

### 2.5 `BaseCharacter.maxHealth` ignorado
**Arquivo:** `Assets/Scripts/Base/BaseCharacter.cs`
- Em `Awake()`: `healthSystem.MaxHealth = maxHealth` (SpawnManager sobrescreve depois no Instantiate — ordem segura).

### 2.6 Mira do ataque = mouse cursor
**Arquivo:** `Player.cs` (`OnAttack`)
- Direção: `Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue())` → normalizada até o jogador (fallback `facingDirection`).
- Saltodo Warg usa a mesma direção.

---

## Fase 3 — Melhorias e limpeza

### 3.1 `PlayerAttack`: acertar só pela hitbox
- ~~Exigir `other == hitboxCollider`~~ — gate invertido (bloqueava todo dano); **removido na Fase 4**.

### 3.2 State Pattern: dash e transições
- `OnDash()`: `SetState(PlayerStates.Dash)`; ao terminar: voltar `Idle`/`Walk`.
- `HandleDamageTaken` e `OnAttackEnd`: transitar de `Attack` para `Idle`/`Walk`.

### 3.3 Ranges de detecção no `EnemyConfig`
- Reaproveitar `chaseDistance`; trocar hardcoded: Zombie 8f, Wildfire 12f, Boss 30f → `ChaseDistance`; atualizar `.asset`s para manter valores atuais.

### 3.4 Tags `Player` nos filhos de forma
- Remover tag `Player` de `FormBat` e `FormWarg` na cena; manter só na raiz.

### 3.5 `EnemyAttack`: não gastar cooldown sem dano
- Checar `!IsInvulnerable && !IsDead` antes de dar dano **e** setar cooldown.

### 3.6 `SpawnManager`
- Cache de `PickPrefab()` só quando for spawnar.
- Só diminuir `currentInterval` quando um spawn de fato ocorrer.

### 3.7 Limpeza de código morto
- Remover `FleeStrategy.cs`, `BaseEnemy.SetSpeedMultiplier`, `HealthHUD.cs`.
- `EnemyConfig.damageImmunityDuration` → ligar via `HealthSystem.SetDamageCooldown` em `BaseEnemy.Awake`.
- `TransformationData.attackCooldown` → aplicar cooldown mínimo entre ataques no `Player.OnAttack`.
- `UIManager.OnDestroy`: limpar `Instance`.
- Strings → `AnimationHashes` nos states (`"IsAttacking"`, `"IsMoving"`, `"IsDead"`).

### 3.8 `ShowVictory`
- Sem chamada (condição de vitória fora do escopo) — pendência de design.

---

## Ordem de execução
1. ✅ Fase 1 (1.1 → 1.4) — destrava jogador.
   - 1.1 ✅ `ApplyForm`/`RevertForm` forçam `OnAttackEnd()` antes de trocar Animator.
   - 1.2 ✅ Fallback `attackEndTimer` (2s) e `deathEndTimer` (3s) em `Player.Update`.
   - 1.3 ✅ `HandleDamageTaken` cancela a hitbox (`CancelHitbox` desde a Fase 4).
   - 1.4 ✅ `UIManager` usa `Keyboard.current.escapeKey`.
   - Extra: `OnDash` agora seta `PlayerStates.Dash` e transição de volta ao final do dash.
2. ✅ Fase 2 (2.1 → 2.6) — inimigos + UI + mira mouse.
   - 2.1 ✅ `EnemyChase.HandleDeath`: `AddEssenceOnKill` + desativa todos os coliders com null-check via `GetComponents`.
   - 2.2 ✅ Slime: `ResetTrigger` + `Rebind`/`Update(0f)` no respawn.
   - 2.3 ✅ `UIManager.WireHud()` assina `OnHealthChanged`/`OnEssenceChanged`; `OnDestroy` desinscreve e limpa `Instance`; GameObject `UIManager` adicionado à cena com `healthSlider` apontando ao slider `HealthBar`.
   - 2.4 ✅ `transformClampLoss` em `ApplyForm`/`RevertForm` + `HealthSystem.SetCurrentHealth`.
   - 2.5 ✅ `BaseCharacter.Awake` aplica `maxHealth` ao `HealthSystem` + `Initialize()`.
   - 2.6 ✅ `GetAimDirection()` — mira pelo mouse (`Mouse.current` + `Camera.main`), fallback `facingDirection`.
3. ✅ Fase 3 (3.1 → 3.7) — melhorias/limpeza.
    - 3.1 ✅ `PlayerAttack`: gate da hitbox (revisto/removido na Fase 4 — lógica invertida).
   - 3.2 ✅ `OnDash` entra em `PlayerStates.Dash`; fim do dash / `OnAttackEnd` / `HandleDamageTaken` transistem para Idle/Walk/Transformed.
   - 3.3 ✅ `detectionRange` hardcoded removido; Zombie/Wildfire/Boss usam `ChaseDistance`; assets atualizados (Zombie=8, Wildfire=12, Troll=30).
   - 3.4 ✅ Tag `Player` removida de `FormBat` e `FormWarg` na cena.
   - 3.5 ✅ `EnemyAttack` só aplica cooldown se o dano efetivamente entrou (checa invulnerável/morto).
   - 3.6 ✅ `SpawnManager`: `HasAnyPrefab()` barato por frame; `SpawnEnemy` retorna bool; intervalo só reduz quando spawn ocorre.
   - 3.7 ✅ Removidos `FleeStrategy.cs`, `SetSpeedMultiplier`, `HealthHUD.cs` (+ component da cena); `damageImmunityDuration` → `HealthSystem.SetDamageCooldown`; `attackCooldown` aplicado no `Player.OnAttack`; hashes nos states; `UIManager.OnDestroy` já ok (Fase 2).
4. ✅ Verificação (abrir Unity, compilar, play test).
   - Compilação: `recompile_status` → `completed`, `failed:false`, `errors:[]` (Tundra build success).
   - Smoke test Play: entrou/saiu do Play sem NRE/`error CS`/`MissingComponent` no Editor.log.
   - Pendente (manual, no Editor): play test completo da seção "Verificação" (ataque→transformar, essência, slime, hitbox mouse, Esc, barras).
5. ✅ Fase 4 — bugs de colisão/dano (relato do jogador).
   - 4.1 ✅ `PlayerAttack.ProcessHit`: removido gate invertido `other != hitboxCollider` (se `hitboxCollider != null`, bloqueava **todo** dano; cache ficava null por Hitbox inativa no `Start`).
   - 4.2 ✅ Janela de hit por timer `hitboxActiveDuration = 0.15s` em `PlayerAttack`; `DisableHitbox` cedo (evento 0,05s) não encerra o swing; `attackOffset` **mantido 0,05** (calibrar no Play se precisar).
   - 4.3 ✅ `EnemyWildfire.ExplodeOnPlayer` → `HealthSystem.ForceTakeDamage` (ignora i-frame/cooldown) e só então `Die()` — kamikaze nunca morre “de graça”.
   - 4.4 ✅ `EnemySlime` morte/respawn: `GetComponents<Collider2D>()` (todos os 2 colliders).
   - 4.5 ✅ Este documento.
   - 4.6 ✅ Slime: 1ª morte → respawn **no local da morte** (`deathPosition`); 2ª morte → **permanente** (sem novo respawn).

## Verificação
1. Unity → compilar sem erros no Console.
2. Play test: atacar→transformar no meio (não congela); Orc rende essência; Slime renasce andando; sem NRE na morte; hitbox segue o mouse; Warg cheio→reverter sem perder HP; Esc pausa sem erro de Input; barras atualizam.
3. Play test Fase 4: Slime toma 3 hits (iminidade 0,5s entre eles); Wildfire ao encostar **sempre** tira dano e morre (mesmo durante dash); hitbox permanece ~0,15s após o clique.
4. Revisar diff antes de qualquer commit (não commitar sem pedido).
