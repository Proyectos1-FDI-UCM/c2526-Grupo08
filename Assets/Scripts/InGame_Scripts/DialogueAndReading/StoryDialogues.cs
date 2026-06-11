//---------------------------------------------------------
// REFERENCIA DE TEXTOS DE HISTORIA — NO WAY DOWN
//
// Este archivo NO es un MonoBehaviour y NO se adjunta a ningún GameObject.
// Es documentación de código: contiene TODOS los textos del juego tal como
// deben introducirse en el Inspector de NarratorDialogue / DialogueSystem.
//
// CÓMO USARLO:
//   · Abre el archivo y busca la sección de la escena que estés montando.
//   · Copia el texto de cada línea en el campo "Text" del Inspector.
//   · Asigna el sprite indicado en "CharacterSprite".
//   · El campo "SpeakerName" vacío indica narración sin personaje visible.
//
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

/*
────────────────────────────────────────────────────────────────────────────────
  [LEVEL_1]  INTRO — AL ENTRAR AL EDIFICIO
  Componente : NarratorDialogue en Trigger_IntroNivel1
  Inspector  : RequireItem = None | PauseWhileActive = true | Transition = false
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : ""          CharacterSprite : ninguno
      "Cori perdió la cuenta de las veces que su vista había viajado de aquel
       edificio abandonado a la dirección que sujetaba en su mano.
       Nochebuena. Hoy de todos los días."

  #2  SpeakerName : ""          CharacterSprite : ninguno
      "Con suerte, las familias se reunían en fechas como esta.
       Todo su esfuerzo, todos sus secretos callados habían sido únicamente
       para este momento. Entró al edificio sin pensarlo dos veces."

  #3  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Inicio de la operación: ya estoy en el edificio."

  #4  SpeakerName : "Control"   CharacterSprite : [Sprite radio / superior]
      "Entendido. Si no encuentras a su líder antes de las seis,
       regresa tal y como acordamos. ¿Entendido?"

  #5  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Sí, señor."

  #6  SpeakerName : "Control"   CharacterSprite : [Sprite radio / superior]
      "Cori… espero tanto como tú que tu hermana realmente esté
       entre los rehenes."

  #7  SpeakerName : ""          CharacterSprite : ninguno
      "Cori cortó la transmisión. No quería pensar en la posibilidad
       de que ella no estuviera allí."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_1]  ENEMIGO ESPECIAL — OPCIÓN "AMENAZAR"
  Componente : DialogueSystem de Level_1 (campo DialogueLines del Inspector)
               Lo llama SpecialEnemyInteraction, no NarratorDialogue.
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "¿Qué te creías, que no te iba a ver?
       Tienes que ser primerizo para hacer algo tan imbécil.
       Dime qué hacías. Ahora."

  #2  SpeakerName : "Soldado"   CharacterSprite : [Sprite enemigo especial]
      "¡La jefa…! Hablaba con la jefa."

  #3  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "¿Con que la jefa, eh? Yo también quiero 'hablar' con ella.
       Tiene un botín que me interesa."

  #4  SpeakerName : "Soldado"   CharacterSprite : [Sprite enemigo especial]
      "Me niego. Si digo algo que no debo, estoy muerto."

  #5  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "¿Y? Ya lo estás de todas formas. Esta es tu última oportunidad."

  #6  SpeakerName : "Soldado"   CharacterSprite : [Sprite enemigo especial]
      "Ese tono que tanto odio… Me recuerdas a mi jefa.
       Mira, no puedo decirte dónde está. Pero en la sala de reuniones
       del final a la izquierda estuvo esta mañana y anotó algunas cosas."

  #7  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "No sé de quién se está burlando este cretino.
       Si de su jefa, o de mí."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_1 → LEVEL_2]  ASCENSOR — CINEMÁTICA DE TRANSICIÓN
  Componente : NarratorDialogue en el GameObject del ascensor
               (mismo collider que antes tenía LevelWin; LevelWin se elimina)
  Inspector  : RequireItem = Fusibles | RequiredAmount = 3
               FeedbackText = "Necesitas los 3 fusibles para activar el ascensor."
               PauseWhileActive = true | TriggerLevelTransitionOnEnd = true
               NextSceneName = "Level_2"
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : ""          CharacterSprite : ninguno
      "El ascensor crujió al ponerse en marcha.
       Cori contó los segundos en silencio mientras las paredes
       de metal oxidado ascendían a su alrededor."

  #2  SpeakerName : ""          CharacterSprite : ninguno
      "Segunda planta. El edificio empeoraba con cada piso:
       techos desmoronados, cables a la vista, y más guardias."

  #3  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Aquí arriba hay más movimiento. Tienen algo que proteger."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_2]  INTRO — AL LLEGAR A LA SEGUNDA PLANTA
  Componente : NarratorDialogue en Trigger_IntroNivel2
  Inspector  : RequireItem = None | PauseWhileActive = true | Transition = false
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : ""          CharacterSprite : ninguno
      "La segunda planta era un caos. Trozos de techo caídos,
       cables a la vista. Alguien había luchado aquí antes que ella."

  #2  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Necesito la tarjeta de acceso para llegar a la zona confidencial.
       Aquí tiene que haber algo."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_2]  DOCUMENTO DE LORE — CARPETA CON EXPERIMENTOS
  Componente : DocumentReader en el prefab del documento
               Rellenar el array "Pages" del DocumentReader (no NarratorDialogue).
────────────────────────────────────────────────────────────────────────────────

  Página 1 :
      "INFORME CONFIDENCIAL — Experimentos con cristal mágico.
       Organizaciones de primer nivel, tanto aliadas como enemigas,
       adquieren grandes cantidades de este mineral.
       Su composición exacta es desconocida, pero su potencial
       como material para armamento es incuestionable."

  Página 2 :
      "Las pruebas confirman que el cristal amplifica la energía cinética
       en un factor de entre 4 y 7 según la pureza de la muestra.
       Los prototipos de armas basadas en él superan en eficiencia
       a cualquier tecnología convencional conocida."

  Página 3 (nota manuscrita) :
      "Nota personal —
       Si continuamos así, dominaremos a las demás organizaciones
       con nuestras armas en las próximas dos semanas.
       Jamás pensaba que iba a escribir esto, pero por primera vez
       estoy orgullosa de mis secuaces.
       En la reserva a veces me gusta contemplar aspectos más banales
       de la piedra. Su color… es igual que su color favorito."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_2]  MONÓLOGO POST-DOCUMENTO
  Componente : NarratorDialogue en Trigger_PostDocumento (junto a la salida de la sala)
  Inspector  : RequireItem = None | PauseWhileActive = false | Transition = false
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Conseguí información sobre experimentos con el mineral.
       Querían dominar a todas las organizaciones con estas armas."

  #2  SpeakerName : "Control"   CharacterSprite : [Sprite radio / superior]
      "Buen trabajo, Cori. Sigue inspeccionando hasta encontrar
       a los rehenes. No pierdas el objetivo principal de vista."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_2 → LEVEL_BOSS]  ASCENSOR — CINEMÁTICA DE TRANSICIÓN
  Componente : NarratorDialogue en el GameObject del ascensor de Level_2
  Inspector  : RequireItem = Cards | RequiredAmount = 2
               FeedbackText = "Necesitas las dos mitades de la tarjeta de acceso."
               PauseWhileActive = true | TriggerLevelTransitionOnEnd = true
               NextSceneName = "Level_Boss"
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : ""          CharacterSprite : ninguno
      "Tercera planta. El ascensor se detuvo con un golpe seco.
       Desde aquí Cori podía oír algo… ¿respiración? ¿Pasos?"

  #2  SpeakerName : ""          CharacterSprite : ninguno
      "Los rehenes estaban cerca. Y también lo estaba quien los retenía."

  #3  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Ya casi. Aguanta."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_BOSS]  INTRO — ANTES DE ENTRAR A LA SALA DEL BOSS
  Componente : NarratorDialogue en Trigger_IntroBoss (delante de la puerta)
  Inspector  : RequireItem = None | PauseWhileActive = true | Transition = false
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : ""          CharacterSprite : ninguno
      "Al otro lado de esa puerta estaba la persona que había dado
       órdenes a todos los que Cori había derrotado esta noche."

  #2  SpeakerName : ""          CharacterSprite : ninguno
      "La persona por la que había entrado a este mugriento edificio."

  #3  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "Raven… ¿qué te ha pasado?"


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_BOSS]  FINAL BUENO — Cori derrota a Raven
  Activado por BossManager en código cuando la vida del boss llega a 0.
  Ejemplo:
      dialogueSystem.SetLines(bossGoodEndingLines); // lista SerializeField en BossManager
      Time.timeScale = 0f;
      dialogueSystem.StartDialogue(() => LevelManager.Instance.OnBossDeath());
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "¿R-Raven…? ¿De verdad eres tú?"

  #2  SpeakerName : "Raven"     CharacterSprite : [Sprite Raven]
      "¿Te parece que tengo cara de alguien más?"

  #3  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "No… Pero viéndote es como si no te conociera en absoluto."

  #4  SpeakerName : "Raven"     CharacterSprite : [Sprite Raven]
      "¿Te has acobardado? Acaba conmigo si eres tan valiente. ¡Ven aquí!"

  #5  SpeakerName : "Cori"      CharacterSprite : [Sprite Cori]
      "¡Basta! Entré a este mugriento edificio con el único objetivo
       de traerte de vuelta, y me niego a salir de él sin cumplirlo."

  #6  SpeakerName : "Raven"     CharacterSprite : [Sprite Raven]
      "Veo que tu terquedad tan solo ha aumentado desde que saliste
       de la policía…"

  #7  SpeakerName : ""          CharacterSprite : ninguno
      "Sin líder, la organización de Raven fue desmantelada a lo largo
       de esa madrugada. Los rehenes fueron liberados.
       Raven y Cori ahora trabajan juntas, y planean hacerlo durante
       mucho más tiempo."


────────────────────────────────────────────────────────────────────────────────
  [LEVEL_BOSS]  FINAL MALO — Cori muere en el combate
  Activado por LevelManager cuando la vida del jugador llega a 0 en la escena boss.
  En LevelManager.OnPlayerDeath() para la escena boss:
      dialogueSystem.SetLines(bossBadEndingLines);
      Time.timeScale = 0f;
      dialogueSystem.StartDialogue(() => { panelDeath.SetActive(true); });
────────────────────────────────────────────────────────────────────────────────

  #1  SpeakerName : ""          CharacterSprite : ninguno
      "El resto de la noche transcurrió de forma silenciosa.
       Ante el cuerpo inerte de Cori, Raven no pudo más que observar."

  #2  SpeakerName : ""          CharacterSprite : ninguno
      "Tiró sus armas y cayó de rodillas.
       Raven era consciente de que su hermana podría llegar hasta ella,
       y pensó que estaba dispuesta a acabar con ella si se cruzaba en su camino."

  #3  SpeakerName : ""          CharacterSprite : ninguno
      "Y ahora que se había hecho realidad, el mundo perdió su color
       antes de que ella pudiera darse cuenta.
       Nadie supo nada más de la organización de Raven."

*/

// Sin código ejecutable. Este archivo no debe adjuntarse a ningún GameObject.