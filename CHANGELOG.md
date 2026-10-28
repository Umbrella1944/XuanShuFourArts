# Changelog

## 1.1.0.0

- Added per-character growth driven by Consummate, reincarnations, lifespan-relative age and learned combat skills.
- Kept shared skill configuration at slider strength and applied growth at each wielder's runtime consumers, including Practice, active values, damage steps, recovery and Inner Qi gains.
- Added an NPC Blood-Rift and Normal Attack bonus toggle, enabled by default; Taiwu and NPCs use the same effects and growth algorithm.
- Fixed Stamina damage conversion discarding non-integer damage remainders.
- Added rate-limited diagnostics and an optional debug logging toggle; separated Debug and Release build outputs.
- Added tooltip lifecycle cleanup and state-based refresh to reduce repeated layout work.
- Added Korean localization for settings, arts, effects and tooltips; CNH uses Chinese text and unknown languages fall back to English.

## 1.0.3.0

- Added independent strength settings for all four arts and the four-piece set bonus, from 10% to 300% in 10% steps; all defaults remain 100%.
- Connected the settings to Practice and active bonuses, Body/Mind and Inner Qi benefits, passive healing, Stamina damage conversion, injury recovery, Blood-Rift chance/power and set bonuses.
- Kept costs, requirements, grade, grids, durations, stack limits and effect eligibility rules unchanged.
- Added bilingual settings descriptions and dynamic Chinese/English effect and set tooltips. Full restart is required after changes.
- Preserved missing-setting defaults and bounded loaded values; used fractional healing progress and probabilistic fractional injury recovery.
- Local build and offline validation completed. The author published the Workshop update on 2026-09-29; in-game UI/combat validation remains pending.

## 1.0.2.0

- Updated supported game version to 1.1.23.
- Adapted all four arts to the current 16-bit direct/reverse effect ID fields, preventing config initialization failures.
- Replaced the removed combat flag with current native Normal Attack state checks for Blood-Rift eligibility.
- Made the removed legacy equipment-screen adapter optional; the current equipment screen retains all five set-progress refresh hooks.
- Preserved all four arts, set bonuses, balance values, and Chinese/English tooltips.

## 1.0.1.9

- Updated supported game version to 1.0.45.
- Increased the four-piece enhancement for Profound Breath Returning-Frame Sutra's passive recovery from 1 active tick per 7 auto-heal updates to 1 active tick per 5 auto-heal updates.
- Updated the four-piece tooltip text for Wounds Return to Pivot from +42.9% to +100%.
- Improved Blood-Rift Pursuit forced-hit compatibility by avoiding negative hit-odds sentinel values during special-effect checks.

## 1.0.1.8

- Added a Normal Attack eligibility ticket for Blood-Rift triggering.
- Blood-Rift can now only roll from a legitimate player Normal Attack sequence, preventing free-attack or pursuit event tails from retriggering the passive repeatedly.
- Reduced Blood-Rift Pursuit power per stack from 200% to 150%.

## 1.0.1.7

- Fixed Blood-Rift Pursuit sometimes resolving as only a single follow-up hit.
- Blood-Rift Pursuit now follows the vanilla Normal Attack pursuit chain during its own free attack, while keeping the original safety gates for fallen characters, phase changes, fight-back states, and pursuit-forbid effects.

## 1.0.1.6

- Removed the four-piece command-priority override so vanilla Normal Attack chains keep their original priority, visuals, and damage flow.
- Blood-Rift now rolls once after a complete Normal Attack sequence resolves, as long as at least one hit landed during that sequence.
- Updated Blood-Rift and four-piece descriptions to match the current behavior.

## 1.0.1.5

- Fixed Blood-Rift pursuit control incorrectly suppressing vanilla/weapon pursuit whenever Myriad-Devouring Blood-Rift Fist was equipped.
- Vanilla pursuit is now only suppressed during the Blood-Rift Pursuit free attack itself.

## 1.0.1.4

- Updated supported game version to 1.0.29.
- Improved Blood-Rift trigger compatibility with Normal Attack chains across weapon types.
- Changed the Blood-Rift four-piece assurance to begin after 4 failed triggers, gain +15% trigger chance per further failure, and cap at 100%.
- Raised the four arts' secondary Practice Effect attributes from 30% to 40%.
- Lowered Myriad-Devouring Blood-Rift Fist's active casting power while keeping its original two-step injury structure.

## 1.0.1.2

- Improved Blood-Rift Normal Attack trigger compatibility across weapon attack chains.
- Blood-Rift now rolls at most once per Normal Attack sequence, while no longer requiring `pursueIndex == 0`.

## 1.0.1.1

- Restored Myriad-Devouring Blood-Rift Fist's flavor description.
- Clarified the in-game Blood-Rift passive text so the base fist passive and four-piece set enhancements are separated correctly.
- Updated the four-piece tooltip text to describe only the actual set enhancements: Normal Attack Power, trigger assurance, and practitioner-command priority.

## 1.0.1.0

- Reworked Myriad-Devouring Blood-Rift Fist into an independent backend passive.
- Added Blood-Rift stacks: each stack increases the next Blood-Rift Pursuit's power by 200%, up to 10 stacks for the whole battle.
- Blood-Rift Pursuit now always hits, always crits, and cannot trigger bounce damage. Normal Attacks still trigger bounce damage normally.
- Added four-piece trigger assurance for Blood-Rift: after repeated failed triggers, trigger chance gradually increases up to 45%.
- Added practitioner-command priority over Normal Attacks while the four-piece set is active.
- Adjusted four-piece set values:
  - Four Arts Resonance reduced from 20% to 15%.
  - Blood-Rift Pursuit power per stack reduced from 300% to 200%.
  - Secondary base attributes on the four arts reduced to 30%.
- Adjusted Yin-Yang Form-Restoring Art injury recovery on maimed body parts from 3 stacks to 1 stack.
- Fixed Shadow-Reflecting Wandering Step description by removing the incorrect continuous Stamina recovery wording.
- Added low-tier breakthrough burden compatibility for the four core arts only.
- Further decoupled all four core passive effects into this mod's independent effect shells and backend logic.
- Improved set tooltip placement and suppressed the large set tooltip during combat.
- Polished English localization for Blood-Rift Pursuit stack consumption, interruption loss, and bounce-damage immunity.
- Updated supported game version to 1.0.24.
- Kept workshop detail-image list empty in release metadata to avoid replacing manually arranged workshop images.

## 1.0.0.9

- Added the Four Arts set bonus system.
- Added initial in-game set bonus tooltip with Alt detail mode.

## 1.0.0.8

- Baseline local source snapshot.
