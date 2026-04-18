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

4.4 Câmera — Shared Screen
  Câmera única compartilhada entre todos os players (mesma tela)
  Zoom dinâmico baseado na distância entre players:
    Players próximos → mais zoom (leitura de combate corpo a corpo)
    Players distantes → menos zoom (mapa visível, pressão de zona legível)
    Limites de zoom evitam que a câmera fique inutilizável nos extremos
  Posição centralizada entre todos os players ativos
  Confinada pelos limites do nível (nunca mostra fora do mapa)

  Impacto de design:
    Cria tensão de proximidade — se você se afasta, a câmera abre e o oponente vê mais
    Convergência forçada pela zona amplifica o efeito (espaço reduzido = zoom aumenta)
    Informação parcial preservada: zoom máximo ainda não mostra o mapa inteiro

4.5 Zone System (CRÍTICO)
  Tiles entram em estado:
    Safe → Warning → Dead
  Tempo por camada
  Jogador em tile morto recebe dano contínuo

  → Isso cria o "fechamento" do battle royale

4.6 Arena Transition System (CRÍTICO)

  Sistema responsável por simular o fechamento da zona através da transição física entre arenas conectadas.
  Substitui o dano contínuo do Zone System original pela Spike Wall (morte instantânea por contato).

  Estrutura Geral:
    A dungeon é composta por múltiplas arenas fixas (sem geração procedural)
    Cada arena é uma cena independente
    Arenas são conectadas linearmente através de portas
    A progressão sempre ocorre de uma arena maior para uma menor

  4.6.1 Arena Layout
    Arenas possuem tamanho decrescente
    Primeira arena:
      Maior espaço
      Maior densidade de loot
      Menor pressão inicial
    Arenas subsequentes:
      Menores
      Mais intensas
      Menor espaço de evasão

    Intenção de design:
      Criar sensação de "afunilamento"
      Forçar encontros mais frequentes

  4.6.2 Door System (Portas)
    Cada arena possui portas que conectam à próxima
    Estados das portas:
      Locked (fechadas)
      Open (abertas)
    Regras:
      Portas começam fechadas
      Permanecem fechadas durante a fase normal da arena
      Abrem apenas no momento de transição

  4.6.3 Arena Timer
    Cada arena possui um tempo de permanência
    Estados:
      Safe Phase — jogadores exploram livremente, spawn de inimigos normal
      Warning Phase — feedback visual/sonoro, preparação para transição
      Transition Phase — porta abre, Spike Wall inicia movimento

  4.6.4 Spike Wall (Zona Ativa)
    Elemento principal do sistema — substitui o dano de zona
    Comportamento:
      Surge na borda esquerda da arena
      Move continuamente para a direita
      Velocidade constante (ou progressivamente crescente)
    Regras:
      Contato = morte instantânea (one-hit kill)
      Não pode ser atravessada
      Empurra implicitamente o jogador para frente

  4.6.5 Arena Transition Flow
    Sequência exata:
      1. Timer da arena termina
      2. Portas se abrem
      3. Spike Wall começa a se mover
      4. Spawn de inimigos aumenta
      5. Jogadores são forçados a migrar
      6. Câmera começa a acompanhar o movimento da parede
      7. Jogadores entram na nova arena
      8. Spike Wall fecha completamente a arena anterior
      9. Cena anterior é descarregada

  4.6.6 Enemy Pressure Scaling
    Durante a transição:
      Spawn rate aumenta significativamente
      Tipos de inimigos podem mudar (mais agressivos)
    Regra importante:
      Inimigos são bound à arena de origem — NÃO atravessam portas
    Efeito de design:
      Pressão atrás (PvE) + Pressão à frente (PvP)

  4.6.7 Camera Behavior durante Transição
    Câmera continua sendo shared (mesma lógica de 4.4)
    Durante a transição (SpikeWall em movimento):
      Continua seguindo os jogadores normalmente com zoom dinâmico
      Confinada pelo TransitionBounds — um bounds maior que cobre as duas arenas
      Jogadores podem ver tanto a arena atual quanto a próxima enquanto se movem
    Quando a SpikeWall termina seu trajeto:
      Câmera transita suavemente do TransitionBounds para o CameraBounds da nova arena
      A transição de bounds é interpolada para evitar corte abrupto
    Intenção de design:
      Comunica urgência sem HUD
      Evita que jogador "fique parado"
      Permite visibilidade da nova arena durante a travessia — o jogador sabe para onde fugir

  4.6.8 Scene Streaming
    Cada arena é uma cena independente (Unity Additive Scene Loading)
    Durante a transição:
      Próxima arena é carregada em paralelo (additive)
      Ambas as arenas coexistem na memória durante a travessia
    Após fechamento completo da Spike Wall:
      Arena anterior é descarregada
    Benefícios:
      Performance
      Travessia fluida estilo Metroidvania (sem loading screen)
      Escalabilidade

  4.6.9 Persistent Scene
    Sistemas que nunca devem ser destruídos ficam numa cena separada (PersistentScene)
    Carregada como cena principal (índice 0 no Build Profile)
    Carrega a primeira arena automaticamente via GameBootstrap
    Conteúdo da PersistentScene:
      ArenaManager
      ArenaLoader
      Camera (CameraFitLevel)
      PlayerSpawner
      Canvas / HUD (DoorIndicator, DashUI)
      Global Light 2D
      EventSystem
    Conteúdo de cada arena (Arena1, Arena2, ...):
      Tilemaps (chão, paredes, decoração)
      CameraBounds (BoxCollider2D)
      Door (DoorController)
      SpikeWall (SpikeWallController)
      EnemySpawners
      ArenaContent (registro local dos sistemas da arena)
      Loot e props

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

8.2 Sistema de Mira e Acoplamento de Armas
  HandsPivot: objeto filho do player que rotaciona em direção ao alvo
  Suavização de rotação configurável
  Flip automático quando mira está à esquerda (localScale.y = -1)
  Sway (balanço) baseado na velocidade angular de rotação
  Dois weapon slots: RightWeaponSlot e LeftWeaponSlot
  AimDirection: propriedade pública com a direção de mira atual (usada por RangedWeapon)

  HandsPivot como camada de acoplamento (Weapon Holding Layer):
    HandsPivot funciona como uma camada invisível maior que o player
    Seu Transform Scale define o "raio" de acoplamento:
      Scale maior → armas aparecem mais distantes do centro do player
      Scale menor → armas aparecem mais próximas
    weaponSlot é filho do HandsPivot com localPosition (x, 0, 0) como ponto de ancoragem
    A arma equipada tem sempre localPosition = (0,0,0) — sem offset baked no prefab
    Resultado: armas de qualquer tamanho se comportam uniformemente;
      o ajuste de distância é feito uma vez no HandsPivot, não prefab a prefab

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
    Ao equipar, força localPosition = Vector3.zero e localRotation = Quaternion.identity
      → garante que a arma sempre parte do centro do weaponSlot, independente do prefab

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

  SpriteRenderer desacoplado do objeto raiz do player:
    O sprite do player pode estar num GameObject filho (ex: "Body") separado do raiz
    HitEffect fica no raiz do player (junto com Rigidbody2D e Collider)
    SpriteRenderer é exposto como campo serializável [SerializeField] no HitEffect
      → deve ser preenchido no Inspector apontando para o objeto filho com o sprite
    Fallback automático: se não preenchido, busca GetComponent → GetComponentInChildren
    Essa separação permite animar, escalar ou trocar o sprite independentemente
      sem afetar física, colisão ou lógica do player

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
  Clamp de posição por BoxCollider2D (levelBounds ou transitionBounds): câmera nunca mostra fora dos limites ativos
  Suavização via SmoothDamp (posição e zoom) — sem overshoot nos limites do nível
  Fallback: se nenhum player ativo, câmera mantém posição/zoom atual
  Campos: minOrthoSize, maxOrthoSize, padding, smoothTime

  TransitionBounds (extensão do CameraFitLevel):
    Cada arena possui um BoxCollider2D extra (TransitionBounds) que cobre o espaço da arena atual + próxima
    Definido em cada cena de arena e registrado via ArenaContent.TransitionBounds
    Ao estado Transition: câmera passa a usar o TransitionBounds como clamp em vez do levelBounds
      → permite que a câmera siga os jogadores para dentro da próxima arena
    Quando SpikeWall termina e SetBounds(arena nova) é chamado:
      CameraFitLevel cacheia o Bounds atual (TransitionBounds como struct) antes do unload da cena
      Inicia interpolação suave (MoveTowards) entre TransitionBounds → CameraBounds da nova arena
      Velocidade configurável via boundsTransitionSpeed no Inspector
    TransitionBounds deve ser marcado como Is Trigger para não interferir na física
    Última arena não precisa de TransitionBounds (campo vazio = câmera livre durante eventual Transition)

  Transition Push (extensão do CameraFitLevel):
    Referencia ArenaManager e escuta OnArenaStateChanged
    Quando estado = Transition:
      _currentOffset cresce suavemente via Lerp até maxTransitionOffset (em unidades do mundo, eixo X positivo)
      Velocidade de crescimento controlada por transitionOffsetSpeed
    Quando estado ≠ Transition:
      _currentOffset retorna suavemente a 0
    targetPos final = center dos players + Vector3(currentOffset, 0, 0)
    Clamp de bounds aplicado após o offset
    Campos: maxTransitionOffset, transitionOffsetSpeed

8.14 Arena Transition System — Implementação Técnica (completa)

  ArenaManager (script):
    State machine com enum ArenaState: Safe → Warning → Transition → Completed
    Timer por estado (safeDuration, warningDuration, transitionDuration) configurável no Inspector
    Avança automaticamente entre estados ao zerar o timer
    Evento público: System.Action<ArenaState> OnArenaStateChanged
      → todos os sistemas da arena se inscrevem nesse evento (porta, parede, câmera)
    Completed para o loop — sem transição além dele
    Campos: safeDuration, warningDuration, transitionDuration

  DoorController (script):
    Estados internos: Closed (collider ativo) / Open (collider desativo)
    Começa fechada no Start
    Escuta OnArenaStateChanged do ArenaManager via OnEnable/OnDisable
    Ao estado Transition → chama OpenDoor()
    Animação de abertura: deslize + fade simultâneos via Coroutine
      slideDirection: Vector2 configurável (ex: (0,1) = cima, (1,0) = direita)
      slideDistance: distância total percorrida até sumir
      animationDuration: duração da animação em segundos
      slideCurve: AnimationCurve para controle de aceleração (padrão EaseInOut)
    Porta some completamente ao final (alpha = 0, posição = closedPos + offset)
    CloseDoor() reseta posição e alpha instantaneamente
    Eventos públicos: OnDoorOpened, OnDoorClosed (usados pelo DoorIndicator)
    Campos: slideDirection, slideDistance, animationDuration, slideCurve

  DoorIndicator (script — HUD):
    Componente no objeto de seta dentro do Canvas (Screen Space - Overlay)
    Referencia DoorController, Camera e RectTransform da seta
    Escuta OnDoorOpened / OnDoorClosed via OnEnable/OnDisable
    Lógica por frame:
      Converte posição da porta para viewport
      Se fora da tela: exibe seta na borda da tela apontando para a porta
      Se visível: oculta a seta
    Posicionamento na borda: clamp proporcional ao halfW/halfH da tela
    Tratamento de porta atrás da câmera (viewport.z < 0): inverte direção
    Rotação: angle = Atan2(direction) − 90° + rotationOffset (ajustável no Inspector)
    Pulse: offset senoidal (Sin * pulseDistance) somado à edgePos antes de setar posição
    Campos: screenEdgePadding, pulseDistance, pulseSpeed, rotationOffset

  SpikeWallController (script):
    Começa inativo: Collider2D e SpriteRenderer desabilitados no Awake
    Escuta OnArenaStateChanged do ArenaManager via OnEnable/OnDisable
    Ao estado Transition → Activate():
      Habilita Collider2D e SpriteRenderer
      Liga flag _moving
    Movimento em FixedUpdate: rb.MovePosition na direção Vector2.right * moveSpeed
      Fallback para transform.position se não houver Rigidbody2D
    Rigidbody2D configurado como Kinematic, gravityScale = 0
    Colisão via OnTriggerEnter2D (collider deve ser Is Trigger):
      Player (PlayerMovement): SetActive(false) direto — bypassa invencibilidade do dash
      Inimigos (IDamageable): TakeDamage(float.MaxValue) — morte instantânea
      GetComponentInParent usado em ambos os casos (suporta hierarquia com filhos)
    Campos: arenaManager, moveSpeed, wallCollider, wallSprite

  ArenaContent (script — em cada arena):
    Registro local dos sistemas de uma arena
    Campos serializados: SpikeWallController, DoorController, BoxCollider2D (CameraBounds), BoxCollider2D (TransitionBounds)
    Propriedades públicas readonly: SpikeWall, Door, CameraBounds, TransitionBounds
    TransitionBounds: BoxCollider2D que cobre esta arena + a próxima; deve ser Is Trigger
      Arrastado para o campo via Inspector; última arena deixa o campo vazio
    Colocado num GameObject vazio na raiz de cada cena de arena
    Não possui lógica — é apenas um container de referências para o ArenaLoader

  ArenaLoader (script — PersistentScene):
    Gerencia o ciclo de vida das arenas durante a partida
    Campos: ArenaManager, CameraFitLevel, DoorIndicator, string[] arenaSceneNames
    No Start: registra o ArenaContent da primeira arena (já carregada pelo GameBootstrap)
    Ao estado Transition:
      1. Carrega próxima arena em modo Additive
      2. Aguarda SpikeWall da arena atual atingir endBoundaryX (OnWallReachedEnd)
      3. Descarrega arena anterior
      4. Chama RegisterArenaContent(nextScene):
           SetBounds → atualiza CameraFitLevel com o CameraBounds da nova arena
           SetTransitionBounds → atualiza CameraFitLevel com o TransitionBounds da nova arena
           SetDoor → atualiza DoorIndicator com a DoorController da nova arena
           Subscreve OnWallReachedEnd da nova SpikeWall
    Proteções: _loadingInProgress impede execução dupla; verifica isLoaded antes de load/unload

  GameBootstrap (script — PersistentScene):
    Carrega a primeira arena em modo Additive no Start
    Campo: firstArenaScene (nome da cena, ex: "Arena1")

  DoorController — atualização cross-scene:
    ArenaManager buscado via FindAnyObjectByType no Start se referência Inspector estiver vazia
    Permite que a porta funcione sem referência hardcoded para a PersistentScene

  SpikeWallController — atualização cross-scene:
    ArenaManager buscado via FindAnyObjectByType no Start se referência Inspector estiver vazia
    Evento OnWallReachedEnd disparado quando transform.position.x >= endBoundaryX
    Após disparar: para de se mover (_moving = false, _reachedEnd = true)

  DoorIndicator — atualização cross-scene:
    SetDoor(DoorController): troca a porta monitorada com subscribe/unsubscribe corretos
    Update verifica door == null (porta destruída ao descarregar cena) e esconde seta automaticamente

  CameraFitLevel — atualização cross-scene:
    SetBounds(BoxCollider2D): troca levelBounds, desativa _inTransition e inicia lerp suave de bounds
      Se vinha de uma transição: cacheia TransitionBounds como struct (antes do unload) e interpola para o novo levelBounds
    SetTransitionBounds(BoxCollider2D): atualiza o bounds de transição da arena atual
    ClearBounds(): zera levelBounds (câmera livre)
    Durante _inTransition: câmera usa _transitionBounds como clamp — segue players entre as duas arenas
    Ao entrar em Transition: _cachedTransitionBounds salva Bounds como struct — imune ao unload da cena
