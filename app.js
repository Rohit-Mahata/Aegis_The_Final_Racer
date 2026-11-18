const missions = [
  { title: "MIDNIGHT<br />TRANSFER", chapter: "RACE 01 / BLACKTOP DISTRICT", description: "A courier runs the docks past midnight. Cut him off without causing a wreck and find out who bought Mara's debt.", clue: "Courier list", prize: "$1,200" },
  { title: "GLASS &<br />GASOLINE", chapter: "RACE 02 / GLASSWORKS", description: "The contact knows Mara's real location. Beat his crew through the refinery and make him talk.", clue: "Dock manifest", prize: "$1,600" },
  { title: "RED MESA<br />RUN", chapter: "RACE 03 / RED MESA", description: "Climb above the city as the storm closes in. Every turn cuts closer to the Syndicate's hidden auction.", clue: "Vesper's seal", prize: "$2,200" },
  { title: "THE LAST<br />LIGHT", chapter: "FINAL RACE / VESPER TOWER", description: "One race remains. Win it before the clock expires and bring Mara home—whatever the cost.", clue: "The truth", prize: "$3,000" }
];

const state = AegisState.createInitialState();
const $ = (selector) => document.querySelector(selector);
const screens = [...document.querySelectorAll(".screen")];

function showScreen(id) {
  screens.forEach((screen) => screen.classList.toggle("active", screen.id === id));
}

function formatCredits() {
  $("#credits").textContent = `$${state.credits.toLocaleString()}`;
  $("#clueCount").textContent = `${state.clues.length} / 3`;
}

function showToast(message) {
  const toast = $("#toast");
  toast.textContent = message;
  toast.classList.add("show");
  setTimeout(() => toast.classList.remove("show"), 2200);
}

function openMission(index) {
  if (index > state.unlockedLocation) {
    showToast("Complete the previous race to unlock this route");
    return;
  }
  state.currentLocation = index;
  state.bet = Math.min(400, state.credits);
  const mission = missions[index];
  $("#missionChapter").textContent = mission.chapter;
  $("#missionTitle").innerHTML = mission.title;
  $("#missionDescription").textContent = mission.description;
  $("#prize").textContent = mission.prize;
  $("#clueReward").textContent = mission.clue;
  updateBet();
  showScreen("missionScreen");
}

function updateBet() {
  $("#betAmount").textContent = `$${state.bet}`;
  $("#decreaseBet").disabled = state.bet <= 100;
  $("#increaseBet").disabled = state.bet >= state.credits || state.bet >= 1000;
}

document.querySelectorAll(".location").forEach((button) => {
  button.addEventListener("click", () => openMission(Number(button.dataset.location)));
});

$("#continueButton").addEventListener("click", () => openMission(state.unlockedLocation));
$("#brandButton").addEventListener("click", () => showScreen("mapScreen"));
$("#backButton").addEventListener("click", () => showScreen("mapScreen"));
$("#decreaseBet").addEventListener("click", () => { state.bet = Math.max(100, state.bet - 100); updateBet(); });
$("#increaseBet").addEventListener("click", () => { state.bet = Math.min(1000, state.credits, state.bet + 100); updateBet(); });

function refreshMap() {
  document.querySelectorAll(".location").forEach((button, index) => {
    button.classList.toggle("locked", index > state.unlockedLocation);
  });
  formatCredits();
}

$("#startRaceButton").addEventListener("click", startRace);

let raceFrame;
let raceRunning = false;

function startRace() {
  showScreen("raceScreen");
  const canvas = $("#raceCanvas");
  const ctx = canvas.getContext("2d");
  const keys = new Set();
  let progress = 0;
  let drift = 0;
  let position = 3;
  let countdown = 3;
  let countdownActive = true;
  let lane = 0;
  let roadOffset = 0;
  $("#raceResult").classList.remove("show");

  const countdownTimer = setInterval(() => {
    countdown -= 1;
    if (countdown > 0) $("#countdown").textContent = countdown;
    else if (countdown === 0) $("#countdown").textContent = "GO";
    else {
      clearInterval(countdownTimer);
      $("#countdown").style.display = "none";
      countdownActive = false;
      raceRunning = true;
    }
  }, 800);

  function keyDown(event) {
    if (["ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", "Space"].includes(event.code)) event.preventEvent?.();
    if (["ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", "Space"].includes(event.code)) event.preventDefault();
    keys.add(event.code);
  }
  function keyUp(event) { keys.delete(event.code); }
  window.addEventListener("keydown", keyDown);
  window.addEventListener("keyup", keyUp);

  function drawRoad() {
    const w = canvas.width;
    const h = canvas.height;
    const sky = ctx.createLinearGradient(0, 0, 0, h * 0.58);
    sky.addColorStop(0, "#111827");
    sky.addColorStop(1, "#a54832");
    ctx.fillStyle = sky;
    ctx.fillRect(0, 0, w, h);

    ctx.fillStyle = "rgba(255, 204, 120, .12)";
    ctx.beginPath(); ctx.arc(w * 0.72, h * 0.23, 70, 0, Math.PI * 2); ctx.fill();

    ctx.fillStyle = "#161a1d";
    ctx.beginPath();
    ctx.moveTo(w * 0.42, h * 0.43); ctx.lineTo(w * 0.58, h * 0.43);
    ctx.lineTo(w * 0.93, h); ctx.lineTo(w * 0.07, h); ctx.closePath(); ctx.fill();

    ctx.strokeStyle = "rgba(255,255,255,.35)";
    ctx.lineWidth = 5;
    [-0.22, 0.22].forEach((offset) => {
      ctx.beginPath();
      ctx.moveTo(w * 0.5 + w * offset * 0.08, h * 0.43);
      ctx.lineTo(w * 0.5 + w * offset, h);
      ctx.stroke();
    });

    for (let i = 0; i < 12; i++) {
      const y = ((i * 70 + roadOffset) % (h * 1.1));
      const scale = y / h;
      ctx.fillStyle = "rgba(255, 255, 255, .7)";
      ctx.fillRect(w * (0.5 - 0.008), y, 14 * scale + 5, 28 * scale + 8);
    }

    ctx.fillStyle = "#e54a2a";
    ctx.fillRect(w * 0.5 - 18 + lane * 140, h - 150, 36, 90);
    ctx.fillStyle = "#090b0d";
    ctx.fillRect(w * 0.5 - 48 + lane * 140, h - 92, 96, 70);
    ctx.fillStyle = "#ff7052";
    ctx.fillRect(w * 0.5 - 38 + lane * 140, h - 70, 76, 12);
  }

  function tick() {
    if (countdownActive) {
      drawRoad();
      raceFrame = requestAnimationFrame(tick);
      return;
    }

    if (keys.has("ArrowLeft") || keys.has("KeyA")) lane = Math.max(-1.25, lane - 0.035);
    if (keys.has("ArrowRight") || keys.has("KeyD")) lane = Math.min(1.25, lane + 0.035);
    if (keys.has("ArrowUp") || keys.has("KeyW") || keys.has("Space")) progress += 0.46;
    else progress += 0.28;

    const drifting = (keys.has("ArrowLeft") || keys.has("ArrowRight") || keys.has("KeyA") || keys.has("KeyD")) &&
      (keys.has("ArrowUp") || keys.has("Space"));
    drift = drifting ? Math.min(100, drift + 1.8) : Math.max(0, drift - 1.1);
    roadOffset += drifting ? 18 : 10;
    progress = Math.min(100, progress);

    if (progress > 22 && position > 2) position = 2;
    if (progress > 48 && position > 1) position = 1;
    if (progress > 72 && position > 1 && drift > 20) position = 1;
    $("#position").textContent = position;
    $("#distanceValue").textContent = `${Math.floor(progress)}%`;
    $("#raceProgress").style.width = `${progress}%`;
    $("#driftFill").style.width = `${drift}%`;
    drawRoad();

    if (progress >= 100) {
      cancelAnimationFrame(raceFrame);
      window.removeEventListener("keydown", keyDown);
      window.removeEventListener("keyup", keyUp);
      finishRace(drift);
      return;
    }
    raceFrame = requestAnimationFrame(tick);
  }
  raceFrame = requestAnimationFrame(tick);
}

function finishRace(driftScore) {
  raceRunning = false;
  const won = driftScore >= 20 || state.currentLocation === 0;
  const result = AegisState.settleRace(state, won, state.bet);
  const mission = missions[state.currentLocation];
  if (won && state.clues.length < 3) AegisState.addClue(state, mission.clue);
  if (won) AegisState.advanceLocation(state);

  $("#resultEyebrow").textContent = won ? "RACE COMPLETE" : "THE NIGHT GOT CLOSER";
  $("#resultTitle").textContent = won ? "VICTORY" : "DEFEAT";
  $("#resultText").textContent = won
    ? `Clue recovered: ${mission.clue}. ${result > 0 ? `+$${result.toLocaleString()} won.` : ""}`
    : `You lost $${state.bet}. Chase the route again and learn the next turn.`;
  $("#raceResult").classList.add("show");
  refreshMap();
}

$("#continueResultButton").addEventListener("click", () => {
  $("#countdown").style.display = "grid";
  $("#countdown").textContent = "3";
  showScreen("mapScreen");
  refreshMap();
});

refreshMap();
