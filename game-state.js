(function (root, factory) {
  const api = factory();
  if (typeof module === "object" && module.exports) module.exports = api;
  root.AegisState = api;
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
  function createInitialState() {
    return {
      credits: 2400,
      clues: [],
      currentLocation: 0,
      unlockedLocation: 0,
      bet: 400
    };
  }

  function settleRace(state, won, bet) {
    const prize = state.currentLocation === 3 ? 3000 : [1200, 1600, 2200, 3000][state.currentLocation];
    if (won) {
      state.credits += prize + bet;
      return prize + bet;
    }
    state.credits -= bet;
    return -bet;
  }

  function addClue(state, clue) {
    if (!state.clues.includes(clue)) state.clues.push(clue);
  }

  function advanceLocation(state) {
    if (state.unlockedLocation < 3) state.unlockedLocation += 1;
  }

  return { createInitialState, settleRace, addClue, advanceLocation };
});
