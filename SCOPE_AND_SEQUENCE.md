# HOT TO MARK - Scope & Sequence Document

## Project Overview

**Hot to Mark** is a stunt driving simulation game where the player takes on the role of a professional stunt driver on a film set. The player must drive a vehicle to a precise mark, stop accurately, then reverse back to the starting position ("one") -- all while being filmed. The game captures the unique pressure and skill of real stunt driving work.

**Target Platform:** iOS (iPhone) via Unity
**Current Prototype:** HTML5 Canvas (browser-based, keyboard controls)
**Genre:** Precision driving / simulation

---

## Core Game Loop

```
1. RECEIVE INSTRUCTIONS → Player sees mark distance, challenge conditions
2. DRIVE TO MARK        → Accelerate, steer, manage speed
3. STOP ON MARK         → Brake precisely within 5% of the exact mark
4. HONK TWICE           → Signal the crew before reversing (mandatory)
5. REVERSE TO ONE       → Drive backward to starting position
6. SCORE & GRADE        → Performance breakdown with penalties
```

---

## User Stories (Existing in Prototype)

### US-1: First-Person Cockpit View
> As a player, I see the game from the driver's seat POV so I feel like I'm actually in the car.

**Acceptance Criteria (all implemented):**
- [x] Dashboard visible across the bottom of the screen
- [x] Steering wheel centered at bottom with 3-spoke design and horn button
- [x] Driver's hands visible on the wheel at 9-and-3 positions
- [x] Hands rotate with the steering wheel
- [x] Side windows visible on left and right with A-pillars
- [x] Rearview mirror centered at top with road reflection
- [x] Windshield with slight tint overlay
- [x] Dashboard vents on left and right sides
- [x] Instrument cluster with speedometer in center of dash

### US-2: Vehicle Controls & Physics
> As a player, I can accelerate, brake, and steer the car so I can drive to the mark.

**Acceptance Criteria (all implemented):**
- [x] Throttle input (W / Up Arrow) accelerates the car
- [x] Brake input (S / Down Arrow) decelerates the car
- [x] Steering input (A/D / Left/Right Arrows) turns the wheel
- [x] Steering wheel visually rotates up to 45 degrees
- [x] Speed capped at 60 MPH forward, 25 MPH reverse
- [x] Drag and friction naturally slow the car
- [x] Smooth steering interpolation (not instant snap)
- [x] Speedometer needle reflects current speed
- [x] Digital MPH readout on dashboard
- [x] Gear indicator shows D (drive) or R (reverse)
- [x] Distance traveled displayed on dash in feet

### US-3: The Road & Environment
> As a player, I see a realistic road with perspective so I feel movement and depth.

**Acceptance Criteria (all implemented):**
- [x] Road rendered with vanishing-point perspective
- [x] Yellow dashed center line scrolls with movement
- [x] White road edge lines
- [x] Green ground/grass on both sides
- [x] Blue sky with clouds at the horizon
- [x] Road visually shifts when steering (lateral offset)

### US-4: The Mark (Target)
> As a player, I can see the mark I need to stop on so I can plan my braking.

**Acceptance Criteria (all implemented):**
- [x] Red tape mark visible on the road ahead
- [x] "MARK" label floats above the tape
- [x] Mark grows larger with perspective as car approaches
- [x] Mark changes color intensity when within 5% threshold
- [x] Mark distance is randomized each take (150-350 feet)
- [x] Stopping within 5% of the mark is required for accuracy score
- [x] Car auto-detects stop when speed < 0.5 MPH near the mark

### US-5: Stopping & Mark Accuracy Scoring
> As a player, I receive a score based on how precisely I stopped on the mark.

**Acceptance Criteria (all implemented):**
- [x] Accuracy calculated as percentage (distance from mark / 5% threshold)
- [x] Mark accuracy contributes up to 40 points of total score
- [x] "MISSED!" shown if accuracy is 0%
- [x] HUD shows "ON MARK!" message with accuracy percentage when stopped
- [x] Car stops completely when mark is hit

### US-6: Horn Honk Before Reversing
> As a player, I must honk the horn twice before reversing, just like a real stunt driver signals the crew.

**Acceptance Criteria (all implemented):**
- [x] Space bar triggers horn honk
- [x] Horn plays an audible sound (square wave via Web Audio API)
- [x] Honk count displayed on HUD (X/2)
- [x] Instructions displayed: "Honk horn TWICE (Space) to begin reverse"
- [x] After 2 honks, car automatically shifts to Reverse after 0.5s delay
- [x] Penalty of -10 points if player somehow reverses without honking twice

### US-7: Reverse to One (Starting Position)
> As a player, I must drive in reverse back to my starting position ("one") to complete the take.

**Acceptance Criteria (all implemented):**
- [x] Gear shifts to R (red indicator) after honking
- [x] Same throttle/brake/steer controls work in reverse
- [x] Green "ONE" mark visible on road during reverse
- [x] HUD shows distance remaining to start position
- [x] HUD shows elapsed time with "HURRY!" warning after 15 seconds
- [x] Take completes when car stops within 2 feet of starting position
- [x] Return accuracy scored (up to 20 points)

### US-8: Reverse Penalties
> As a player, I am penalized for unsafe or slow reversing, adding challenge to the return.

**Acceptance Criteria (all implemented):**
- [x] -10 point penalty if reverse takes longer than 20 seconds
- [x] -10 point penalty if reverse speed exceeds 15 MPH
- [x] -10 point penalty if horn was not honked twice
- [x] All penalties shown in results breakdown with red text
- [x] Max reverse speed tracked and displayed

### US-9: Picture-in-Picture Camera View
> As a player, I can see what the film camera is seeing in a small PIP window, so I understand how my driving looks on screen.

**Acceptance Criteria (all implemented):**
- [x] PIP window in upper-right corner (210x140px)
- [x] Red border with "REC" indicator (blinking red dot)
- [x] Camera label "CAM A - WIDE"
- [x] Shows wide-angle view of the road from the film camera's position
- [x] Car visible in PIP, growing as it approaches the mark
- [x] Red mark line visible in camera view
- [x] Running timecode displayed (MM:SS:FF at 24fps)
- [x] Hidden during menu and results screens

### US-10: BTS Film Crew
> As a player, I can see behind-the-scenes crew members along the road, making the film set feel alive.

**Acceptance Criteria (all implemented):**
- [x] 8 crew members positioned along both sides of the road
- [x] Crew types: grip, camera operator, director, sound, PA, safety
- [x] Each type has distinct color coding
- [x] Camera operators have visible camera on tripod
- [x] Director has a chair with "DIR" label
- [x] Safety crew has yellow vest markings
- [x] Crew scales with perspective (smaller when far, larger when near)
- [x] Crew appears/disappears based on distance from car

### US-11: Scoring & Results Screen
> As a player, I see a detailed score breakdown after each take so I can understand how I performed and improve.

**Acceptance Criteria (all implemented):**
- [x] "TAKE COMPLETE" header
- [x] Total score out of 100 with letter grade (A+ through F)
- [x] Mark Accuracy breakdown (X/40 pts)
- [x] Return to One breakdown (X/20 pts)
- [x] Mode-specific performance score (X/40 pts)
- [x] All penalties listed individually in red
- [x] Stats bar: Top Speed, Total Time, Mark Distance
- [x] "Back to Menu" button to play again

### US-12: Challenge Modes
> As a player, I can choose different challenge modes that test different stunt driving skills.

**Acceptance Criteria (all implemented):**

**Standard Take:**
- [x] Balanced scoring: time + smoothness combined for 40 pts

**Speed Run:**
- [x] Complete the entire take as fast as possible
- [x] Score bonus based on total time (40 - time * 1.5)

**Smooth Operator:**
- [x] Minimize jerky throttle and steering inputs
- [x] Jerk accumulation tracked throughout the take
- [x] Smoothness percentage calculated and scored (X/40 pts)

**Exact MPH:**
- [x] Target MPH displayed (randomly set 15-34 MPH)
- [x] Checkpoint at 60% of mark distance
- [x] Speed measured at checkpoint crossing
- [x] Accuracy: 100 - |actual - target| * 5
- [x] HUD shows target, checkpoint distance, and result after passing

### US-13: Main Menu
> As a player, I can select which challenge mode to play from a clear menu screen.

**Acceptance Criteria (all implemented):**
- [x] Game title "HOT TO MARK" with subtitle "The Stunt Driving Game"
- [x] Four mode buttons: Standard Take, Speed Run, Smooth Operator, Exact MPH
- [x] Each mode has a brief description below its button
- [x] Orange/black film-industry color scheme
- [x] Menu accessible after results via "Back to Menu"

---

## Build Stages (iOS / Unity Rebuild)

The game will be rebuilt in Unity for iOS in 8 logical stages. Each stage produces a testable, playable build. No stage depends on assets or systems that haven't been built in a prior stage.

---

### STAGE 1: Static Cockpit & Touch Input Foundation
**Goal:** Establish the first-person view and prove touch controls work.
**Test:** Player can see the cockpit. Tapping gas/brake zones changes on-screen values. Tilting or swiping steers the wheel.

| Task | Details |
|------|---------|
| 1.1 | Create Unity project (URP), configure for iOS portrait/landscape |
| 1.2 | Build 3D cockpit interior: dashboard, steering wheel, windshield frame, A-pillars |
| 1.3 | Place camera at driver's eye position looking forward through windshield |
| 1.4 | Add driver's hands (rigged or static) gripping wheel at 9-and-3 |
| 1.5 | Create touch control UI: gas pedal zone (right), brake pedal zone (left) |
| 1.6 | Implement steering: tilt-to-steer (accelerometer) OR drag-to-steer (swipe on wheel) |
| 1.7 | Animate steering wheel rotation based on input (max 45 degrees) |
| 1.8 | Animate hands following wheel rotation |
| 1.9 | Add rearview mirror (static reflection placeholder) |
| 1.10 | Add side window geometry with tinted glass material |

**User Stories Covered:** US-1 (partial)

**Test Plan:**
- [ ] Cockpit renders at 60fps on iPhone 12+
- [ ] Touch gas/brake registers input
- [ ] Steering responds to tilt or swipe
- [ ] Wheel and hands animate smoothly
- [ ] No visual clipping or z-fighting

---

### STAGE 2: Road, Movement & Car Physics
**Goal:** Car moves forward on a road. Speed, drag, and friction feel right.
**Test:** Player can accelerate down a road, see speed on the speedometer, and brake to a stop.

| Task | Details |
|------|---------|
| 2.1 | Create road environment: straight road, grass, sky/skybox |
| 2.2 | Add lane markings (dashed yellow center, solid white edges) |
| 2.3 | Implement car physics: acceleration, braking, drag, friction |
| 2.4 | Speed capped at 60 MPH forward |
| 2.5 | Build working speedometer on dashboard (analog needle + digital readout) |
| 2.6 | Add gear indicator on dashboard (D / R) |
| 2.7 | Add distance-traveled display (feet) |
| 2.8 | Road scrolls/moves beneath the car with perspective |
| 2.9 | Steering causes lateral movement (clamped to road width) |
| 2.10 | Vanishing point shifts with lateral position |
| 2.11 | Add engine sound that varies with speed/throttle |

**User Stories Covered:** US-2, US-3

**Test Plan:**
- [ ] Car accelerates smoothly from 0
- [ ] Car decelerates with brake and coasts with drag
- [ ] Speedometer matches actual speed
- [ ] Road perspective feels natural
- [ ] Steering keeps car within road boundaries
- [ ] Engine sound pitch matches speed

---

### STAGE 3: The Mark & Stopping Mechanic
**Goal:** Player has a target to stop on. Accuracy is measured.
**Test:** Player drives to the mark, stops, and sees accuracy percentage.

| Task | Details |
|------|---------|
| 3.1 | Generate random mark distance each take (150-350 feet) |
| 3.2 | Render mark as red tape/line across the road |
| 3.3 | Add "MARK" label above tape, scaling with perspective |
| 3.4 | Mark changes color when within 5% threshold zone |
| 3.5 | HUD element: distance to mark (countdown in feet) |
| 3.6 | Detect stop: speed < 0.5 MPH within mark zone |
| 3.7 | Calculate mark accuracy: distance from mark vs 5% threshold |
| 3.8 | Display "ON MARK!" message with accuracy percentage |
| 3.9 | Freeze car in place after mark stop |
| 3.10 | Handle overshoot: auto-stop if car passes mark + 20ft at low speed |

**User Stories Covered:** US-4, US-5

**Test Plan:**
- [ ] Mark is visible and readable from 300 feet
- [ ] Stopping exactly on mark shows ~100% accuracy
- [ ] Stopping 5% away shows ~0% accuracy
- [ ] Overshooting 20+ feet still triggers stop
- [ ] Distance HUD counts down correctly
- [ ] Different mark distances each game

---

### STAGE 4: Horn, Reverse & Return to One
**Goal:** Complete the core game loop -- drive, stop, honk, reverse, score.
**Test:** Player completes a full take from start to finish and sees a score.

| Task | Details |
|------|---------|
| 4.1 | Add horn button on touch UI (or tap steering wheel center) |
| 4.2 | Play horn sound effect on tap |
| 4.3 | Track honk count, display on HUD (X/2) |
| 4.4 | After 2 honks, auto-shift to reverse with 0.5s delay |
| 4.5 | Implement reverse physics (speed capped at 25 MPH, negative direction) |
| 4.6 | Render green "ONE" mark at starting position during reverse |
| 4.7 | HUD: show distance to start, elapsed reverse time |
| 4.8 | HUD: show "HURRY!" warning after 15 seconds |
| 4.9 | Detect return: car stops within 2 feet of starting position |
| 4.10 | Calculate return accuracy |
| 4.11 | Apply penalties: no honk (-10), too slow > 20s (-10), too fast > 15 MPH (-10) |
| 4.12 | Track reverse max speed |
| 4.13 | Build results screen: total score / 100, letter grade A+ through F |
| 4.14 | Results breakdown: mark accuracy (X/40), return accuracy (X/20), performance (X/40) |
| 4.15 | Results: list all penalties in red |
| 4.16 | Results: show stats -- top speed, total time, mark distance |
| 4.17 | "Play Again" and "Menu" buttons on results screen |

**User Stories Covered:** US-6, US-7, US-8, US-11

**Test Plan:**
- [ ] Horn sound plays on button tap
- [ ] Cannot reverse without honking twice
- [ ] Penalty applied if somehow bypassed
- [ ] Reverse drives car backward
- [ ] "ONE" mark visible and approaching during reverse
- [ ] Timer and distance update in real time
- [ ] Penalty triggers at correct thresholds
- [ ] Results screen shows accurate breakdown
- [ ] Score math is correct (40 + 20 + 40 - penalties = total)
- [ ] Full take completes without crashes or soft-locks

---

### STAGE 5: BTS Crew & Film Set Atmosphere
**Goal:** The road feels like a real film set with crew members visible.
**Test:** Player can see distinct crew types along the road as they drive.

| Task | Details |
|------|---------|
| 5.1 | Create crew member 3D models or animated sprites (6 types) |
| 5.2 | Grip: dark clothes, standing with equipment |
| 5.3 | Camera operator: standing behind camera on tripod |
| 5.4 | Director: seated in director's chair with "DIR" marking |
| 5.5 | Sound: blue clothes, holding boom mic |
| 5.6 | PA: green clothes, standing with walkie-talkie |
| 5.7 | Safety: orange vest with yellow reflective stripes |
| 5.8 | Position 8 crew members along both sides of the road |
| 5.9 | Crew scales and appears/disappears based on distance |
| 5.10 | Add ambient set sounds: walkie-talkie chatter, muffled voices |
| 5.11 | Optional: crew reacts when car passes (turning heads) |

**User Stories Covered:** US-10

**Test Plan:**
- [ ] All 6 crew types visually distinguishable
- [ ] Crew visible from appropriate distances
- [ ] No pop-in artifacts
- [ ] Performance stays at 60fps with all crew rendered
- [ ] Ambient audio adds to atmosphere without drowning out engine

---

### STAGE 6: PIP Camera View
**Goal:** Upper-right corner shows what the film camera sees.
**Test:** Player sees their car approaching the mark from the film camera's perspective.

| Task | Details |
|------|---------|
| 6.1 | Place a secondary camera in the scene (film camera position, wide angle) |
| 6.2 | Render to a RenderTexture displayed in upper-right UI panel |
| 6.3 | PIP frame: black border, red "REC" dot, "CAM A - WIDE" label |
| 6.4 | Running timecode overlay (MM:SS:FF at 24fps) |
| 6.5 | Film camera sees: road, mark, car approaching, crew on sides |
| 6.6 | Car visible in PIP growing as it drives toward camera |
| 6.7 | Hide PIP during menu and results screens |
| 6.8 | Optional: slight film grain or color grade on PIP to differentiate from gameplay view |

**User Stories Covered:** US-9

**Test Plan:**
- [ ] PIP renders at acceptable framerate (30fps minimum in PIP)
- [ ] Car is visible and recognizable in PIP
- [ ] Timecode counts correctly
- [ ] REC indicator visible
- [ ] PIP hides/shows at correct times
- [ ] No significant performance drop from second camera

---

### STAGE 7: Challenge Modes & Menu
**Goal:** All four game modes playable with mode-specific scoring.
**Test:** Each mode applies its unique scoring rules correctly.

| Task | Details |
|------|---------|
| 7.1 | Build main menu screen: title, subtitle, 4 mode buttons with descriptions |
| 7.2 | Orange/black film-industry visual theme |
| 7.3 | **Standard Take** mode: score = time component (20 pts) + smoothness (20 pts) |
| 7.4 | **Speed Run** mode: score = 40 - (total time * 1.5), minimum 0 |
| 7.5 | **Smooth Operator** mode: track throttle and steering jerk accumulation |
| 7.6 | Smoothness percentage = 100 - jerkAccum * 2, scored as X/40 |
| 7.7 | **Exact MPH** mode: set random target (15-34 MPH), place checkpoint at 60% of mark |
| 7.8 | Show target MPH and checkpoint distance on HUD |
| 7.9 | Measure speed at checkpoint crossing |
| 7.10 | Exact MPH accuracy = 100 - |actual - target| * 5 |
| 7.11 | Show checkpoint result on HUD after passing |
| 7.12 | Results screen shows mode-specific scoring breakdown |
| 7.13 | Menu accessible after every take via "Back to Menu" |

**User Stories Covered:** US-12, US-13

**Test Plan:**
- [ ] All 4 modes selectable and launch correctly
- [ ] Standard: both time and smoothness affect score
- [ ] Speed Run: faster times yield higher scores
- [ ] Smooth Operator: jerky inputs reduce score, smooth inputs reward
- [ ] Exact MPH: checkpoint detected, speed measured, accuracy correct
- [ ] Results screen adapts to show mode-specific scoring
- [ ] Menu returns cleanly after every take

---

### STAGE 8: Polish, Audio & iOS Optimization
**Goal:** Game feels finished. Runs smoothly on target iPhones. Ready for TestFlight.
**Test:** Full play session on device with no hitches, good audio, haptics.

| Task | Details |
|------|---------|
| 8.1 | Engine audio system: RPM-based pitch, idle, acceleration, deceleration |
| 8.2 | Tire screech sound on hard braking |
| 8.3 | Horn sound effect (realistic car horn, not synthesized beep) |
| 8.4 | Ambient film set audio: distant crew chatter, walkie-talkies, "quiet on set" |
| 8.5 | "Action!" voice call at start of each take |
| 8.6 | "Cut!" voice call when stopped on mark |
| 8.7 | Haptic feedback: engine vibration, brake pulse, horn tap |
| 8.8 | Visual polish: dashboard materials (leather, plastic, chrome) |
| 8.9 | Windshield reflections and subtle dirt/tint |
| 8.10 | Shadow and lighting pass (time of day?) |
| 8.11 | Rearview mirror: real-time reflection using rear camera + RenderTexture |
| 8.12 | Performance optimization: LOD, occlusion culling, draw call batching |
| 8.13 | Target: 60fps on iPhone 12 and newer |
| 8.14 | iOS build settings: provisioning, icons, launch screen |
| 8.15 | TestFlight deployment |

**User Stories Covered:** All (polish pass)

**Test Plan:**
- [ ] 60fps sustained on iPhone 12
- [ ] All audio plays without clipping or latency
- [ ] Haptics fire at correct moments
- [ ] No visual artifacts on notched/Dynamic Island iPhones
- [ ] Battery usage acceptable (30+ minutes play)
- [ ] TestFlight build installs and runs

---

## Future Stages (Post-Launch Backlog)

These are NOT in scope for the initial build but are logical next steps:

| ID | Feature | Description |
|----|---------|-------------|
| F-1 | **Multiple Vehicles** | Different cars with unique handling (sedan, SUV, muscle car) |
| F-2 | **Multiple Sets** | Different filming locations (city street, desert highway, parking structure) |
| F-3 | **Career Mode** | Progress through increasingly difficult takes across a "film shoot" |
| F-4 | **Leaderboards** | Game Center integration for each mode |
| F-5 | **Replay System** | Watch your take from the film camera angle after completing |
| F-6 | **Director Notes** | Director gives verbal feedback ("Too fast!", "Tighter to the mark!") |
| F-7 | **Weather/Time of Day** | Rain, night shoots, golden hour -- each affecting visibility and grip |
| F-8 | **Multi-Mark Takes** | Drive to mark A, pause, then drive to mark B -- complex choreography |
| F-9 | **Walkie-Talkie Comms** | Hear AD calling "Rolling!", "Speed!", "Action!" with proper film protocol |
| F-10 | **Obstacle Avoidance** | Other vehicles or crew in the road path that must be avoided |

---

## Technology Stack

| Component | Choice | Rationale |
|-----------|--------|-----------|
| Engine | Unity 2022 LTS (URP) | Best iOS game engine, huge ecosystem, render textures for PIP |
| Language | C# | Unity standard |
| Rendering | Universal Render Pipeline | Good visuals, optimized for mobile |
| Physics | Unity Physics / custom | Wheel Colliders or custom arcade physics |
| Audio | FMOD or Unity Audio | Layered engine sounds, spatial audio |
| Touch | Unity Input System | Modern input handling, accelerometer access |
| Haptics | Core Haptics (iOS native plugin) | Fine-grained haptic control |
| Build | Xcode + TestFlight | Standard iOS deployment |
| Version Control | Git | Standard |

---

## Stage Dependency Map

```
Stage 1: Cockpit + Touch
    |
Stage 2: Road + Physics
    |
Stage 3: Mark + Stopping
    |
Stage 4: Horn + Reverse + Scoring  ← CORE GAME LOOP COMPLETE
   / \
  /   \
Stage 5    Stage 6           ← Can be built in parallel
BTS Crew   PIP Camera
  \   /
   \ /
Stage 7: Challenge Modes + Menu
    |
Stage 8: Polish + iOS Deploy       ← Ship to TestFlight
```

**Milestone: Playable Core** = After Stage 4, the full game loop works. This is the most critical milestone. Stages 5-6 add atmosphere. Stage 7 adds variety. Stage 8 ships it.
