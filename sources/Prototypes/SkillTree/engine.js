/* Pure planner model; this format never reads or writes a game save. */
(function (root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.SkillTreeEngine = api;
})(typeof globalThis !== 'undefined' ? globalThis : this, function () {
  'use strict';
  const FORMAT = 'HELLSCRIPT_SKILL_TREE_PLANNER_V1';
  const clone = value => JSON.parse(JSON.stringify(value));
  function context(data, classId) {
    const hero = data.classes.find(c => c.id === classId);
    if (!hero) throw new Error('CLASS');
    return { hero, nodes: hero.nodes, byId: Object.assign(Object.create(null), Object.fromEntries(hero.nodes.map(n => [n.id, n]))),
      ultimateCandidates: hero.nodes.filter(n => n.level >= data.ultimatePrerequisite.from && n.level <= data.ultimatePrerequisite.to) };
  }
  function empty(data, classId, level = 1) {
    context(data, classId);
    return { format: FORMAT, skillRevision: 2, classId, level, stack: [], equipped: [] };
  }
  function evaluate(data, state) {
    const ctx = context(data, state.classId), ranks = {}, available = {}, invested = {};
    for (const id of state.stack) invested[id] = (invested[id] || 0) + 1;
    for (const node of ctx.nodes) {
      const all = node.all.every(r => (ranks[r.id] || 0) >= r.rank);
      const directSource = !node.oneOf?.length || node.oneOf.some(r => (ranks[r.id] || 0) >= r.rank);
      const gate = node.kind !== 'ultimate' || ctx.ultimateCandidates.some(n => (ranks[n.id] || 0) >= 1);
      available[node.id] = state.level >= node.level && all && directSource && gate;
      ranks[node.id] = available[node.id] ? 1 + (invested[node.id] || 0) : 0;
    }
    return { ...ctx, ranks, available, ultimateReady: ctx.ultimateCandidates.some(n => ranks[n.id] >= 1), spent: state.stack.length,
      budget: Math.max(0, state.level - 1), remaining: Math.max(0, state.level - 1) - state.stack.length,
      unlocked: Object.values(available).filter(Boolean).length };
  }
  function slots(data, state, kind) {
    return kind === 'passive' ? data.passiveSlotLevels.filter(l => state.level >= l).length : kind === 'ultimate' ? (state.level >= data.ultimateUnlockLevel ? 1 : 0) : data.slots.active;
  }
  function normalize(data, input) {
    input = migrate(input);
    const ctx = context(data, input.classId);
    const level = Math.max(1, Math.min(data.levelCap, Math.trunc(Number(input.level) || 1)));
    const state = empty(data, input.classId, level);
    for (const id of Array.isArray(input.stack) ? input.stack.slice(0, 200) : []) {
      const node = ctx.byId[id], view = evaluate(data, state);
      if (node && view.available[id] && view.ranks[id] < node.maxRank && view.remaining > 0) state.stack.push(id);
    }
    const view = evaluate(data, state);
    for (const id of Array.isArray(input.equipped) ? input.equipped : []) {
      const node = ctx.byId[id];
      if (!node || !view.available[id] || state.equipped.includes(id)) continue;
      if (state.equipped.filter(i => ctx.byId[i].kind === node.kind).length < slots(data, state, node.kind)) state.equipped.push(id);
    }
    return state;
  }
  function invest(data, input, id) {
    const state = normalize(data, input), view = evaluate(data, state), node = view.byId[id];
    if (!node || !view.available[id]) throw new Error('LOCKED');
    if (view.ranks[id] >= node.maxRank) throw new Error('MAX_RANK');
    if (!view.remaining) throw new Error('POINTS');
    state.stack.push(id); return state;
  }
  function refund(data, input, id) {
    const state = clone(input), index = state.stack.lastIndexOf(id);
    if (index < 0) return normalize(data, state);
    state.stack.splice(index, 1);
    return normalize(data, state);
  }
  function equip(data, input, id) {
    const state = normalize(data, input), view = evaluate(data, state), node = view.byId[id];
    if (!node || !view.available[id]) throw new Error('LOCKED');
    if (state.equipped.includes(id)) { state.equipped = state.equipped.filter(i => i !== id); return state; }
    if (node.kind === 'ultimate') state.equipped = state.equipped.filter(i => view.byId[i].kind !== 'ultimate');
    if (state.equipped.filter(i => view.byId[i].kind === node.kind).length >= slots(data, state, node.kind)) throw new Error('SLOTS');
    state.equipped.push(id); return state;
  }
  function pathPlan(data, input, id) {
    const initial = normalize(data, input), { byId, ultimateCandidates } = context(data, initial.classId);
    function ensure(state, wanted, rank, visiting) {
      const node = byId[wanted];
      if (!node || rank > node.maxRank) throw Object.assign(new Error('NODE'), { id: wanted });
      if (node.level > state.level) throw Object.assign(new Error('LEVEL'), { id: wanted, level: node.level });
      if (visiting.has(wanted)) throw new Error('CYCLE');
      if ((evaluate(data, state).ranks[wanted] || 0) >= rank) return state;
      const nextVisiting = new Set([...visiting, wanted]);
      let next = clone(state);
      for (const requirement of node.all) next = ensure(next, requirement.id, requirement.rank, nextVisiting);
      if (node.oneOf?.length && !node.oneOf.some(r => evaluate(data,next).ranks[r.id] >= r.rank)) {
        const choices = [], failures = [];
        for (const r of node.oneOf) {
          try { choices.push(ensure(next,r.id,r.rank,nextVisiting)); } catch(error) { failures.push(error); }
        }
        if (!choices.length) throw failures[0] || new Error('LOCKED');
        choices.sort((a,b) => a.stack.length-b.stack.length); next=choices[0];
      }
      if (node.kind === 'ultimate' && !evaluate(data, next).ultimateReady) {
        const choices = [], failures = [];
        for (const candidate of ultimateCandidates) {
          try { choices.push(ensure(next, candidate.id, 1, nextVisiting)); } catch (error) { failures.push(error); }
        }
        if (!choices.length) throw failures[0] || new Error('LOCKED');
        choices.sort((a,b) => a.stack.length - b.stack.length); next = choices[0];
      }
      if (!evaluate(data,next).available[wanted]) throw new Error('LOCKED');
      while (evaluate(data, next).ranks[wanted] < rank) next.stack.push(wanted);
      return next;
    }
    try {
      const next = ensure(initial, id, 1, new Set());
      const cost = next.stack.length - initial.stack.length;
      return { ok: next.stack.length <= initial.level - 1, state: next, cost, error: next.stack.length > initial.level - 1 ? 'POINTS' : null };
    } catch (error) { return { ok: false, cost: 0, error: error.message, id: error.id, level: error.level }; }
  }
  function applyPath(data, input, id) {
    const plan = pathPlan(data, input, id);
    if (!plan.ok) throw new Error(plan.error);
    return normalize(data, plan.state);
  }
  function example(data, classId, index) {
    const config = context(data, classId).hero.examples[index];
    let state = empty(data, classId, config.level);
    for (const id of [...config.targets, ...config.equip]) state = applyPath(data, state, id);
    for (const id of config.equip) state = equip(data, state, id);
    return state;
  }
  function ancestors(data, classId, id) {
    const { byId } = context(data, classId), found = new Set();
    function walk(key) { for (const r of [...byId[key].all,...(byId[key].oneOf||[])]) if (!found.has(r.id)) { found.add(r.id); walk(r.id); } }
    if (byId[id]) walk(id); return found;
  }
  function serialize(data, input) { return JSON.stringify(normalize(data, input), null, 2); }
  function migrate(input) {
    if (input.skillRevision !== undefined) return input;
    const next=clone(input), prefix={Warrior:'W',Ranger:'A',Mage:'M'}[input.classId];
    if (next.equipped?.includes(prefix+'18')) {
      next.stack=next.stack?.map(id=>id===prefix+'P18'?prefix+'P19':id);
      next.equipped=next.equipped.map(id=>id===prefix+'P18'?prefix+'P19':id);
    }
    next.skillRevision=2;return next;
  }
  function parse(data, text) {
    const raw = JSON.parse(text);
    if (!raw || raw.skillRevision !== undefined && raw.skillRevision !== 2) throw new Error('FORMAT');
    const input = migrate(raw);
    if (!input || input.format !== FORMAT || !Number.isInteger(input.level) || input.level < 1 || input.level > data.levelCap || !Array.isArray(input.stack) || !Array.isArray(input.equipped)) throw new Error('FORMAT');
    const state = normalize(data, input);
    if (JSON.stringify(state.stack) !== JSON.stringify(input.stack) || JSON.stringify(state.equipped) !== JSON.stringify(input.equipped)) throw new Error('FORMAT');
    return state;
  }
  return { FORMAT, clone, context, empty, evaluate, normalize, slots, invest, refund, equip, pathPlan, applyPath, example, ancestors, serialize, parse };
});
