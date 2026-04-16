SHORT GDD — I DRAW A DUNGEON

1. CORE CONCEPT

Battle royale local multiplayer em dungeon.

Loop:

Spawn → Explorar → Loot → Combater → Sobreviver → Zona fecha → Último vivo

2. CORE MECHANIC (ÁTOMO)

Mover → Lootar → Engajar → Resolver combate → Reposicionar

(Loop contínuo, sem reset — estado persiste)

3. GAME LOOP (MATCH)
Spawn
  Players espalhados na dungeon em spawn points predefinidos
  Sem equipamento inicial
Exploration
  Navegação por tiles/conectores
  Fog of war (opcional)
Loot Phase (em paralelo)
  Armas (melee/ranged)
  Consumíveis
  Buffs temporários
Combat
  Encontros emergentes
  PvP direto (players causam dano entre si)
  PvE (inimigos perseguem o player mais próximo)
Zone Pressure
  Dungeon colapsa (tiles desativam)
  Força convergência
Endgame
  Espaço reduzido
  Combate inevitável
Victory
  Último player vivo

4. SYSTEMS

4.1 Combat
  HP fixo base
  Armas:
    Melee (alto risco, alto dano)
    Ranged (controle de espaço)
  Cooldowns simples
  Hit confirm imediato
  Inimigos como ameaça ativa:
    Perseguem e atacam o player mais próximo automaticamente
    Criam pressão constante que força movimento e decisão
    Complementam a zona de colapso como fonte de perigo paralela ao PvP

  PvP:
    Players causam dano entre si com melee e projéteis
    Dash atravessa outros players (invencibilidade + passthrough físico)
    Knockback aplicado em hits PvP (igual PvE)

  Tipos de inimigo:
    Melee:
      Persegue o player mais próximo e ataca corpo a corpo ao entrar no alcance
      Para quando dentro da stopDistance para evitar overlap
      Força de separação entre inimigos do mesmo tipo evita empilhamento

    Ranged:
      Mantém distância preferencial do player mais próximo (faixa min–max)
      Se muito perto → recua; se muito longe → avança; na faixa ideal → para e atira
      Mira rotaciona continuamente em direção ao player (AimPivot)
      Projéteis atingem apenas o player — não causam dano a outros inimigos

4.2 Loot
  Spawn distribuído por tiles
  Raridade simples:
    Common / Strong
  Sem inventário complexo (slots limitados: 2–3)

4.3 Dungeon
  Grid modular (baseado nos assets)
  Tipos de tile:
    Sala aberta
    Corredor
    Sala com props
  Conexões previsíveis (legibilidade > realismo)

4.4 Zone System (CRÍTICO)
  Tiles entram em estado:
    Safe → Warning → Dead
  Tempo por camada
  Jogador em tile morto recebe dano contínuo

  → Isso cria o "fechamento" do battle royale

5. PLAYER STATE
  HP
  Arma ativa
  Dash (mobilidade/escape):
    Burst de velocidade em curta duração
    Invencibilidade durante o movimento — janela de escape de dano
    Passthrough físico com outros players e inimigos durante o dash
    Cooldown que limita uso frequente
  Habilidade (1 slot)
  Buff ativo (opcional)

  Sem progressão persistente dentro da match além disso.

6. RULES (FORMAIS)
  1 vida
  Sem respawn
  Sem reset
  Informação parcial (visão limitada)

  Sistema orientado a emergência (Rules of Play — sistemas geram comportamento complexo)

7. DESIGN CONSTRAINTS
  Match: 5–10 min
  Alta densidade de encontro (não pode "ficar vazio")
  Loot suficiente para evitar snowball extremo
  Zona sempre resolve stalemate

---

8. IMPLEMENTAÇÃO TÉCNICA (estado atual)

Engine: Unity 2D — Universal Render Pipeline (URP)
Input: Unity Input System (novo)
Multiplayer: Local co-op (split input, mesma tela)
Repositório: https://github.com/HouliJouli/i-draw-a-dungeon

8.1 Player
  Movimento top-down via Rigidbody2D (WASD / left stick)
  Sem gravidade
  Diagonal normalizada (sem vantagem de velocidade)
  Dash (Espaço / botão Sul):
    Velocidade, duração e cooldown configuráveis
    Durante o dash, movimento normal desativado
    Segue a última direção de movimento
    Invencibilidade + IgnoreLayerCollision com Enemies, Projectiles e Player durante o dash
    postDashInvincibility: janela de invencibilidade após o dash terminar
  IsKnockedBack: flag que pausa rb.linearVelocity durante knockback (PvP e PvE)

8.2 Sistema de Mira
  HandsPivot: objeto filho do player que rotaciona em direção ao alvo
  Suavização de rotação configurável
  Flip automático quando mira está à esquerda
  Sway (balanço) baseado na velocidade angular de rotação
  Dois weapon slots: RightWeaponSlot e LeftWeaponSlot
  AimDirection: propriedade pública com a direção de mira atual (usada por RangedWeapon)

  Input de mira por player (sem Mouse.current global):
    OnLook(InputValue) recebe Vector2 via PlayerInput (Send Messages)
    Keyboard&Mouse: aimInput é posição de tela → convertida para world via Camera.ScreenToWorldPoint
    Gamepad: aimInput é direção do right stick → usada diretamente como direção
    Fallback: se PlayerInput não estiver configurado, lê Mouse.current.position

8.3 Sistema de Armas
  Arquitetura modular:
    Weapon (classe base abstrata): dano, alcance, cooldown, durabilidade
    MeleeWeapon: implementação corpo a corpo
    RangedWeapon: implementação à distância
    WeaponHolder: gerencia equip/desequip no player
    IDamageable: interface implementada por qualquer alvo que recebe dano

  Weapon (base):
    Campos: damage, attackRange, attackCooldown, maxUses (0 = infinito)
    TryAttack() respeita cooldown antes de chamar PerformAttack()
    ConsumeUse() decrementa usos; ao zerar, chama WeaponHolder.BreakCurrentWeapon()
    BreakCurrentWeapon() reequipa a arma padrão automaticamente

  WeaponHolder:
    Gerencia um slot de arma ativo (CurrentWeapon)
    Instancia o prefab como filho do weaponSlot
    Equipa uma arma padrão no Start (defaultWeaponPrefab)
    Troca de arma destrói a anterior e instancia a nova

  ShortSword (MeleeWeapon):
    Animação de swing em arco via rotação do transform
    Janela ativa de hit (activeStart / activeEnd normalizados)
    HashSet por ataque garante que cada alvo seja atingido uma só vez
    Skip de self-damage via ownerCollider em DetectHits
    Overshoot ao final do swing
    Squash/stretch proporcional à escala original da arma
    Hit stop (~0.04s) ao confirmar hit
    DetectHits via OverlapCircleAll filtrado por LayerMask (Enemies + Player)

  Bow (RangedWeapon):
    Dispara projétil na direção de HandsPivot.AimDirection
    Flecha nockada visível antes do disparo (projétil com Rigidbody2D e Collider desativados)
    Renock automático após o cooldown do ataque
    Recoil: animação de recuo do transform ao disparar (recoilDistance, recoilDuration)
    Consome uso via ConsumeUse() ao disparar

  Projectile:
    Movimento via Rigidbody2D Kinematic com linearVelocity na direção do disparo
    Campos: speed, damage, maxDistance
    Auto-destrói ao atingir maxDistance ou ao colidir com qualquer objeto
    Ao colidir: aplica TakeDamage via IDamageable e aciona HitEffect se presente
    Ignora colisão com o Collider do owner (quem disparou) via Physics2D.IgnoreCollision
    targetTag (opcional): quando definido, só causa dano em objetos com aquela tag
      → usado pelos inimigos ranged para restringir dano ao player ("Player")
    Projéteis do player não têm targetTag → acertam qualquer IDamageable (PvP + PvE)

  Layer setup necessário:
    Players devem estar na layer "Player"
    IgnoreLayerCollision("Projectiles", "Default") — evita que projéteis explodam no ambiente
    Projéteis acertam layer "Player" normalmente

8.4 Sistema de Pickup de Armas
  WeaponPickup: componente colocado em GameObjects de arma no mundo
  Highlight de proximidade: quando o player entra no trigger, a arma pulsa em escala e cor (configuráveis)
  Coleta via input (OnCollect): OverlapCircleAll ao redor do player, equipa a primeira arma encontrada
  Ao coletar: chama WeaponHolder.EquipWeapon(prefab) e destrói o pickup

8.5 Feedback de Hit
  HitEffect (componente nos inimigos e no player):
    Flash de cor configurável ao receber dano
    Scale punch (squash no impacto) configurável
    Knockback via Rigidbody2D na direção oposta ao atacante
    Duração e força do knockback configuráveis
    Seta IsKnockedBack em Enemy e PlayerMovement durante o knockback
  Hit stop na arma melee ao confirmar hit (WaitForSecondsRealtime)

8.6 UI
  DashUI:
    Imagem radial (Radial 360, Fill Origin Bottom)
    Aparece ao usar o dash, some antes do cooldown terminar
    Alpha controlado por CanvasGroup

8.7 Inimigos

  Enemy (Melee):
    Implementa IDamageable
    HP configurável no Inspector
    Recebe dano, destrói o GameObject ao morrer
    Compatível com HitEffect (flash + knockback automáticos)

    Targeting:
      RefreshTarget() chamado a cada FixedUpdate
      Busca todos os GameObjects com tag "Player" ativos
      Seleciona o mais próximo como alvo atual
      Troca de alvo automática se outro player ficar mais próximo ou o atual morrer

    Movimento:
      Persegue o player alvo via Rigidbody2D (MovePosition — suave)
      Para quando dentro do stopDistance (evita overlap)
      Separação entre inimigos: OverlapCircleAll detecta vizinhos Enemy,
        aplica força de afastamento proporcional à proximidade (evita empilhamento)
      Campos: moveSpeed, stopDistance, separationRadius, separationForce

    Sistema de Ataque:
      Ataca o player alvo quando dentro do attackRange
      Dano aplicado via IDamageable (TakeDamage)
      Cooldown entre ataques configurável
      Verifica IsInvincible do player antes de aplicar dano e feedback visual
      Feedback ao atacar: aciona HitEffect.TriggerHit no player (flash + knockback)

  RangedEnemy:
    Implementa IDamageable
    HP configurável no Inspector
    Compatível com HitEffect (flash + knockback automáticos)

    Targeting:
      RefreshTarget() chamado a cada Update
      Mesma lógica de proximidade do Enemy melee

    Movimento por distância:
      Três estados baseados na distância ao player alvo:
        < minDistance → recua (move na direção oposta ao player)
        > maxDistance → avança (move em direção ao player)
        Entre min e max → para e atira
      Separação entre inimigos (Enemy e RangedEnemy) via OverlapCircleAll
      Campos: moveSpeed, minDistance, maxDistance, separationRadius, separationForce

    Mira:
      AimPivot filho do GameObject raiz, posicionado no centro (local 0,0)
      Rotaciona suavemente em direção ao player alvo a cada Update (Atan2 + LerpAngle)
      FirePoint é filho do AimPivot — aponta sempre para o player
      Campo: aimSpeed

    Sistema de Ataque:
      Instancia projectilePrefab no FirePoint na direção do player
      Usa Projectile.Init(direction, ownerCollider, targetTag: "Player")
        → projéteis do inimigo só causam dano ao player, ignoram outros inimigos
      Verifica IsInvincible do player antes de disparar
      Campos: projectilePrefab, firePoint, attackCooldown

8.8 Player — Health
  PlayerMovement implementa IDamageable
  HP configurável no Inspector (maxHealth)
  TakeDamage reduz HP e loga no console
  Ao morrer: desativa o GameObject
  IsInvincible: propriedade pública que bloqueia TakeDamage e HitEffect durante o dash

8.9 Dash — Invencibilidade e Passthrough
  Durante o dash, isInvincible = true
  TakeDamage retorna imediatamente se isInvincible está ativo
  SetDashInvincibility ativa IgnoreLayerCollision para:
    Player x Enemies
    Player x Projectiles
    Player x Player (passthrough PvP durante o dash)
  Invencibilidade estende-se além do dash: ao terminar o dashDuration, inicia invincibilityTimer
  postDashInvincibility: duração da invencibilidade após o dash (configurável no Inspector)
  Ao zerar o invincibilityTimer: isInvincible = false e colisões reativadas

8.10 EnemySpawner
  Componente independente adicionado a um GameObject vazio na cena
  Usa BoxCollider2D como área de spawn (bounds)
  Spawn inicial: spawna initialEnemyCount inimigos no Start em posições aleatórias dentro dos bounds
  Wave Spawn (opcional, toggle no Inspector):
    A cada waveCooldown segundos, spawna enemiesPerWave inimigos
    Pode ser desligado via enableWaveSpawn sem alterar os valores
  Campos expostos: enemyPrefab, spawnArea, initialEnemyCount, enableWaveSpawn, waveCooldown, enemiesPerWave

8.11 PlayerSpawner
  Componente independente adicionado a um GameObject vazio na cena
  Instancia cada player prefab em seu respectivo spawn point no Start
  playerPrefabs[i] → spawnPoints[i] (correspondência por índice)
  Slots nulos são pulados sem erro
  Campos: playerPrefabs (array), spawnPoints (array de Transform)

8.12 Multiplayer — Input por Player
  Cada player prefab tem um componente PlayerInput
  Action Asset compartilhado: InputSystem_Actions
  Behavior: Send Messages — callbacks roteados apenas para componentes do mesmo GameObject
  Player 1: Default Scheme = Keyboard&Mouse
    Move: WASD / Arrow Keys
    Look: <Mouse>/position → convertido para world direction em HandsPivot
    Attack: Mouse Left Button
    Dash: Space
    Collect: E
  Player 2: Default Scheme = Gamepad
    Move: Left Stick
    Look: Right Stick → direção usada diretamente em HandsPivot
    Attack: Button West
    Dash: Button South
    Collect: Button West

8.13 Câmera — CameraFitLevel
  Câmera ortográfica única compartilhada entre todos os players
  Posição: centro entre todos os players ativos (tag "Player")
  Zoom dinâmico baseado na maior distância entre qualquer par de players:
    orthographicSize = Clamp(maxDist / 2 + padding, minOrthoSize, maxOrthoSize)
  Players mortos (inativos) são ignorados no cálculo
  Clamp de posição por BoxCollider2D (levelBounds): câmera nunca mostra fora dos limites do nível
  Suavização via Lerp com smoothSpeed configurável
  Fallback: se nenhum player ativo, câmera mantém posição/zoom atual
