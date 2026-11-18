const test = require("node:test");
const assert = require("node:assert/strict");
const stateApi = require("../game-state.js");

test("initial state starts with no clues and 2400 credits", () => {
  const state = stateApi.createInitialState();
  assert.equal(state.credits, 2400);
  assert.deepEqual(state.clues, []);
});

test("a win awards the race prize and wager", () => {
  const state = stateApi.createInitialState();
  const payout = stateApi.settleRace(state, true, 400);
  assert.equal(payout, 1600);
  assert.equal(state.credits, 4000);
});

test("a loss removes only the wager", () => {
  const state = stateApi.createInitialState();
  const payout = stateApi.settleRace(state, false, 400);
  assert.equal(payout, -400);
  assert.equal(state.credits, 2000);
});

test("clues are unique and locations advance once", () => {
  const state = stateApi.createInitialState();
  stateApi.addClue(state, "Courier list");
  stateApi.addClue(state, "Courier list");
  stateApi.advanceLocation(state);
  stateApi.advanceLocation(state);
  assert.deepEqual(state.clues, ["Courier list"]);
  assert.equal(state.unlockedLocation, 2);
});
