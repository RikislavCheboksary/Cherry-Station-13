// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._DV.CosmicCult.Components;
using Content.Goobstation.Shared.Bible; // Goobstation - Bible
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Damage;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Server.Atmos.Rotting;
using Content.Server.Administration.Systems;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicConversionSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!;
    [Dependency] private readonly CosmicGlyphSystem _cosmicGlyph = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RottingSystem _rotting = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphConversionComponent, TryActivateGlyphEvent>(OnConversionGlyph);
    }

    private void OnConversionGlyph(Entity<CosmicGlyphConversionComponent> uid, ref TryActivateGlyphEvent args)
    {
        var possibleTargets = _cosmicGlyph.GetTargetsNearGlyph(uid,
            uid.Comp.ConversionRange,
            entity => _cosmicCult.EntityIsCultist(entity));

        if (possibleTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-conditions-not-met"), uid, args.User);
            args.Cancel();
            return;
        }

        if (possibleTargets.Count > 1)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-too-many-targets"), uid, args.User);
            args.Cancel();
            return;
        }

        foreach (var target in possibleTargets)
        {
            if (_rotting.IsRotten(target)) //Goobstation: Prevents using space corpses.
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-rotting"), uid, args.User);
                args.Cancel();
            }
            if (HasComp<BibleUserComponent>(target))
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-chaplain"), uid, args.User);
                args.Cancel();
            }
            else if (uid.Comp.NegateProtection == false && HasComp<MindShieldComponent>(target))
            {
                _popup.PopupEntity(Loc.GetString("cult-glyph-target-mindshield"), uid, args.User);
                args.Cancel();
            }
            else
            {
                _stun.TryStun(target, TimeSpan.FromSeconds(4f), false);
                _rejuvenateSystem.PerformRejuvenate(target); //Goobstation: No one likes being brought into the antag gang dead, now do we?
                _cultRule.CosmicConversion(uid, target);
                var finaleQuery = EntityQueryEnumerator<CosmicFinaleComponent>(); // Enumerator for The Monument's Finale
                while (finaleQuery.MoveNext(out var monument, out var comp)
                    && comp.CurrentState == FinaleState.ActiveBuffer)
                {
                    comp.BufferTimer -= TimeSpan.FromSeconds(45);
                    _popup.PopupCoordinates(Loc.GetString("cosmiccult-finale-speedup"), Transform(monument).Coordinates, PopupType.Large);
                }
            }
        }
    }
}
