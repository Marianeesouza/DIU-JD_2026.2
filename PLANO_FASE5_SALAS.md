# Plano de Implementação — Fase 5: Salas, Portas e Progressão

Documento de implementação da Fase 5 (sistema de salas). Executado junto com `PLANO_AJUSTES.md` (✅ por item).

## Objetivo

Fluxo multi-cena: **SampleScene (sala 1) → Room2 → Room3Boss**.

- Sala rastreia inimigos vivos (Slime conta como vivo até morrer **duas vezes**).
- Portas ativas/enquanto houver ≥1 vivo; quando todos mortos → desativam (GO/layer `Doors`).
- Exit (layer/tag `ExitFloor`) carrega a próxima sala quando o player encosta (sala limpa).
- HP/essência persistem entre cenas; Retry restaura o checkpoint da entrada da sala.

## Decisões confirmadas

| Item | Decisão |
|------|---------|
| Estratégia | Multi-cenas |
| Salas | `SampleScene` → `Room2` → `Room3Boss` |
| Template | Clonar `SampleScene` (usuário pinta tilemaps depois) |
| Persistência | Estática (`PlayerProgress`) — HP, essência, checkpoint por sala |
| Layers (user) | 9 = `ExitFloor`, 10 = `Doors`; tags `ExitFloor`, `Doors` |
| Portas | GO `Doors` (layer 10, tag `Doors`) — liga/desliga `SetActive` |
| Sem inimigos | Portas abrem no `Start` da sala |

## Arquivos

### Novos

| Arquivo | Papel |
|---------|--------|
| `Assets/Scripts/Enemies/RoomController.cs` | Conta vivos (`FindGameObjectsWithTag("Enemy")`); portas ligadas se ≥1 não-permanente |
| `Assets/Scripts/Scenes/SceneExit.cs` | Trigger no `ExitFloor`; se sala limpa → `LoadScene(nextSceneName)` |
| `Assets/Scripts/Core/PlayerProgress.cs` | Estático: HP/essência vivos + checkpoint da sala; `Capture`, `Apply`, `SaveCheckpoint`, `RestoreCheckpoint`, `Clear` |
| `PLANO_FASE5_SALAS.md` | Este documento |

### Editados

| Arquivo | Mudança |
|---------|---------|
| `BaseCharacter.cs` | `public virtual bool IsPermanentlyDead => isDead;` |
| `EnemySlime.cs` | `override IsPermanentlyDead => hasRespawned && isDead;` |
| `Player.cs` | `Start`: aplicar progresso se houver; morte/retry usam checkpoint |
| `TransformationManager.cs` | `Start`: aplicar essência persistida (hoje zera em 0) |
| `UIManager.cs` | `OnStartButton`/`OnMainMenuButton` → `PlayerProgress.Clear()` |
| `EditorBuildSettings.asset` | + `Room2.unity`, `Room3Boss.unity` |

## Lógica

### 1. Vivos na sala (`RoomController`)

```
encontrar todos com tag Enemy
vivo = BaseCharacter != null && !IsPermanentlyDead
portas.SetActive(vivo > 0)   // liga se alguém vivo
```

- Slime 1ª morte: `isDead && !hasRespawned` → **vivo** (sala travada).
- Slime 2ª morte: `IsPermanentlyDead` → não conta.
- Inimigo destruído (Wildfire, etc.): fora da busca → não conta.
- Invocados (boss) com tag `Enemy` entram sozinhos na busca.
- Avalia no `Start` + `Update` (barato; poucos inimigos).

### 2. Exit (`SceneExit`)

- `OnTriggerEnter2D`: só tag `Player`.
- Se houver `RoomController` na sala e não limpa → ignora.
- `PlayerProgress.Capture()` → `SceneManager.LoadScene(nextSceneName)`.
- `nextSceneName` vazio + sala limpa → `UIManager.ShowVictory()` (sala do boss).

### 3. Persistência (`PlayerProgress`)

| Momento | Ação |
|---------|------|
| Exit limpo | `Capture()` (HP + essência atuais) |
| `Player.Start` | Se `HasProgress` → restaurar HP; senão default |
| `TransformationManager.Start` | Se `HasProgress` → restaurar essência; senão 0 |
| Ao aplicar na sala | `SaveCheckpoint()` (entrada da sala) |
| `OnRetryButton` | `RestoreCheckpoint()` antes do reload |
| `OnStartButton` / menu | `Clear()` (jogo novo) |

### 4. Cenas

1. Montar `SampleScene` (collider trigger no ExitFloor, `SceneExit`, `RoomController`).
2. Clonar → `Room2.unity`, `Room3Boss.unity` (GUID novo em cada `.meta`).
3. Em cada clone: `SceneExit.nextSceneName` (`Room2` → `Room3Boss`, boss → vazio).
4. Registrar as 3 cenas no Build Settings.
5. Room2/Room3Boss: layout/enimigos = o do template; usuário edita depois.

## Setup de cena (SampleScene)

1. **ExitFloor**: `BoxCollider2D` **IsTrigger** + componente `SceneExit` (`nextSceneName = "Room2"`).
2. **RoomController**: novo GO `Room` (ou no root), tag/livre; `doors` → GO `Doors` (auto-find por tag se vazio).
3. **Doors**: permanece ativo no load; `RoomController` desliga quando limpo.

## Ordem de execução

1. ⏳ Criar este `.md`.
2. ⏳ `IsPermanentlyDead` em `BaseCharacter` + `EnemySlime`.
3. ⏳ `PlayerProgress.cs`.
4. ⏳ `RoomController.cs`.
5. ⏳ `SceneExit.cs`.
6. ⏳ Wire: `Player`, `TransformationManager`, `UIManager`.
7. ⏳ Setup SampleScene (colliders/ componentes) via Unity CLI.
8. ⏳ Clonar cenas (GUID novo) + `EditorBuildSettings`.
9. ⏳ `recompile` + smoke Play (sem NRE / `error CS`).
10. ⏳ ✅ neste doc + `PLANO_AJUSTES.md`.

## Riscos / notas

- **Unity aberto**: preferir CLI (`open_scene`, `add_component`, `save_scene`) a editar YAML à mão.
- **TransformationManager.Start zera essência** — ordem vs `Player.Start` não garantida; aplicar dentro do próprio `Start` via `PlayerProgress`.
- **Tag `Enemy`**: confirmar que todos os inimigos têm a tag (SpawnManager/boss).
- **Collider exit**: sem `IsTrigger`, `OnTriggerEnter2D` nunca dispara.
- **Game over**: Retry mantém HP/essência do checkpoint (entrada da sala), não da morte.
- **Não commitar** sem pedido explícito.

## Verificação

1. Compila sem erros.
2. Sala 1: portas fechadas; matar todos (slime ×2) → portas somem; encostar no exit → `Room2`.
3. Room2: HP/essência iguais à saída da sala 1; limpar → exit → `Room3Boss`.
4. Boss: exit vazio → Victory (ou só limpar se exit não usado).
5. Matar player → Retry → HP/essência da **entrada** da sala.
6. Main menu / Start → progresso zerado.
