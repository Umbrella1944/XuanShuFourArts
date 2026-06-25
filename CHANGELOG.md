# Changelog

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
